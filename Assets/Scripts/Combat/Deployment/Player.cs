using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System;

public class Player : MonoBehaviour
{
    public static Player I { get; private set; }
    [SerializeField] public GameObject PrefabArrow;
    [SerializeField] private Transform ArrowOriginTransform;
    [SerializeField] public GameObject MeleeAttackPoint;
    private Vector3 MeleeAttackHalfSize = new Vector3(2.5f, 2.5f, 2.5f);
    public Animator Animator;
    public ParticleSystem PSDust;
    public Slider Healthbar;
    public LayerMask LayerMaskTarget;
    public LayerMask LayerMaskGround;
    public StarterAssets.ThirdPersonController PlayerController;


    // Passed damage should have already accounted for possible defense reduction.
    public virtual int GetPostDmgState(int dmgAfterDef)
    {
        return CalcHpRemaining(dmgAfterDef) > 0 ? 1 : 0;
    }

    public virtual float CalcHpRemaining(int dmg) { return Mathf.Max(Health - dmg, 0); }
    public float Health = 100;
    public float HealthMax { get; protected set; } = 100;
    protected bool _blocking = false;
    public bool Blocking {
        get { return _blocking; }
        set 
        {
            _blocking = value;
            Animator.SetBool("Block", value);
        }
    }
    protected float BlockTime = 1f;
    protected Timer BlockTimer = new Timer(1);
    protected float TimeArrowDrawMax = 1f;
    protected float TimeArrowDraw = 0f;
    protected Timer TimerArrowDraw = new Timer(1f, true);
    private bool _DrawingArrow = false;
    public bool DrawingArrow { 
        get { return _DrawingArrow; }
        set
        {
            Animator.SetBool("DrawingArrow", value);
            if (value && value != _DrawingArrow)
            {
                SoundManager.I.playerAudioPlayer.DrawBow();
            }
            _DrawingArrow = value;
        }
    }
    
    private float _DrawCharge = 0f;
    private float DrawCharge {
        get { return _DrawCharge; }
        set 
        {
            _DrawCharge = value;
            DrawPowerSlider.value = DrawCharge;
            Animator.SetFloat("DrawCharge", DrawCharge);
        }
    }
    public Slider DrawPowerSlider;
    private float DrawTime = 0f;
    private float DrawTimeMax = 1f;

    // Stamina
    public Slider staminabar;
    public float StamMax = 100f;
    private float _Stamina = 100f;
    public float Stamina
    {
        get { return _Stamina; }
        set
        {
            _Stamina = Mathf.Max(value, 0f);
            if (staminabar != null)
            {
                UpdateSlider(staminabar, StamMax, _Stamina);
            }
        }
    }
    private Timer StamRegenTimer = new Timer(.0005f);
    private bool CanRegenStamina {
        get { return !DrawingArrow && !Blocking && !Sprinting; }
    }
    protected float StamRegenAmount = 5f;
    protected float StamAttackCost = 20f;
    protected float StamRangeCost = 10f;
    protected float StamBlockCost = 20f;
    protected float StamSprintCost = 10f;
    public bool Sprinting = false;

    public Slider Manabar;
    private float ManaMax = 100f;
    private float _Mana = 100f;
    public float Mana
    {
        get { return _Mana; }
        set
        {
            _Mana = Mathf.Max(value, 0f);
            if (Manabar != null)
            {
                UpdateSlider(Manabar, ManaMax, Mana);
            }
        }
    }

    protected float SmoothSpeed = .125f;
    protected int AttackDmg;
    protected int Defense;
    protected float BlockRatingBase = .25f;
    protected float BlockRatingAdditional = 0f;
    protected float BlockRating {
        get 
        {
            return Mathf.Max(0f, 1f - (BlockRatingBase + BlockRatingAdditional));
        }
        set { return; }
    }

    public event Action<Vector3> OnPositionChange;
    private Vector3 _position;
    public Vector3 Position { 
        get 
        {
            return transform.position;
        } 
        protected set 
        {
            if (value == _position)
                return;
            _position = value;
            if (OnPositionChange != null)
                OnPositionChange(Position);
        } 
    }

     private void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Health = HealthMax;
        Mana = ManaMax;
        Stamina = StamMax;
    }

    void Update()
    {
        if (CamSwitcher.I.current_cam != CamSwitcher.BATTLE)
            return;
     
        Position = transform.position;
        ManageInput();
        UpdateStamina(Time.deltaTime);
        ToggleDustPs(PlayerController.GetVelocityMagnitude());
    }

    private void ManageInput()
    {
        if (Stamina < 5f)
            return; 

        DrawingArrow = Controller.I.DrawArrow.phase == InputActionPhase.Performed;
        Sprinting = !Blocking && !DrawingArrow && Controller.I.sprint;

        if (Controller.I.Block.phase == InputActionPhase.Performed && !DrawingArrow)
        {
            //Block(true);
        }
        else if (Controller.I.Block.phase == InputActionPhase.Canceled) 
        {
            //Block(false);
        }
    }

    protected void Init()
    {
        Stamina = StamMax;
        Mana = ManaMax;
        UpdateSlider(staminabar, StamMax, Stamina);
    }

    public void UpdateStamina(float dt)
    {
        RegenStamina(StamRegenAmount);

        if (BlockTimer.Increase(dt))
        {
            Blocking = false;
        }

        Sprint();
        DrawBow();
    }

    private void Sprint()
    {
        if (Sprinting)
        {
            if (Stamina >= StamSprintCost * Time.deltaTime)
            {
                Stamina -= StamSprintCost * Time.deltaTime;

            } else
            {
                Sprinting = false;
            }
        }
    }

    private void DrawBow()
    {
        if (DrawingArrow && DrawTime < DrawTimeMax)
        {
            if (Stamina > StamRangeCost * Time.deltaTime)
            {
                if (DrawTime < DrawTimeMax) // Draw
                {
                    DrawTime += Time.deltaTime;
                    DrawCharge = DrawTime / DrawTimeMax;
                    Stamina -= StamRangeCost * Time.deltaTime;
                } else { // Hold fully drawn bow
                    Stamina -= StamRangeCost / 2f * Time.deltaTime;
                }
            } else
            {
                DrawingArrow = false;
            }
        }

        // Release arrow
        if (!DrawingArrow && DrawCharge > 0) {
            Vector3 target = Statics.GetScreenCenterWorldPos(CamSwitcher.I.BattleCamCaster, LayerMaskGround);
            if (target != Vector3.zero) 
            {
                ShootArrow(LayerMaskTarget, target, 40f);
            }
            DrawTime = 0;
            DrawCharge = 0;
        }
    }

    public void ShootArrow(LayerMask mask, Vector3 targetPos, float attackDmg)
    {
        GameObject a = Instantiate(PrefabArrow, ArrowOriginTransform);
        a.transform.localPosition = Vector3.zero;
        Arrow arrowScript = a.GetComponent<Arrow>();
        arrowScript.Fly(ArrowOriginTransform.position, targetPos, attackDmg, 170f);

        if (Animator != null)
            Animator.SetTrigger("Attack");
    }

    public void RegenStamina(float amount)
    {
        if (Stamina >= StamMax - amount * Time.deltaTime || !CanRegenStamina)
            return;
        //stamina += StaticOps.GetAdjustedIncrease(stamina, amount, MAX_STAMINA);
        Stamina += amount * Time.deltaTime;
    }

    public void MeleeAttack(LayerMask layerMask)
    {
        Animator.SetTrigger("Attack");
        Collider[] hits = Physics.OverlapBox(MeleeAttackPoint.transform.position, MeleeAttackHalfSize, Quaternion.identity, layerMask);
        
        foreach (Collider h in hits)
        {
            Slot s = h.GetComponent<Slot>();
            if (s == null)
                continue;
            Unit u = s.Unit;
            if (u == null)
                continue;
            u.TakeDamage(GetAttackDmg());
        }
    }

    public virtual int TakeDamage(int dmg)
    {
        int finalDmg = CalcDmgTaken(dmg, false);
        int state = GetPostDmgState(finalDmg);
        Health = (int)CalcHpRemaining(finalDmg);
        UpdateHealthbar();

        // Don't trigger an out of sync animation.
        if (!Animator.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage"))
        {
            Animator.SetTrigger("TakeDamage");
        }
        Debug.Log("Player took " + finalDmg + " from " + dmg + " with " + Health + " hp remaining.");

        if (state == 0)
        {
            Die();
        }
        return state;
    }

    protected virtual int CalcDmgTaken(int dmg, bool piercing = false)
    {
        if (Blocking && !piercing)
        {
            dmg = (int)(dmg * BlockRating);
        }
        return dmg > 0 ? dmg : 0;
    }
    
    public void ToggleDustPs(float velMagnitude)
    {
        if (velMagnitude > .1f)
        {
            PSDust.Play();
        } else 
        {
            PSDust.Pause();
        }
    }

    
    public void UpdateHealthbar()
    {
        Healthbar.maxValue = HealthMax;
        Healthbar.value = Health;
       // THp.text = Health.ToString();
        if (Animator != null)
            Animator.SetFloat("Health", Health);
    }

    public void UpdateSlider(Slider slider, float maxValue, float value)
    {
        slider.maxValue = maxValue;
        slider.value = value;
    }

    public void Die() 
    {
        // Game over scenario

    }

    public virtual int GetAttackDmg() { return AttackDmg; }
    public virtual int GetDefense() { return Defense; }
    public virtual float GetHealth() { return Health; }
}
