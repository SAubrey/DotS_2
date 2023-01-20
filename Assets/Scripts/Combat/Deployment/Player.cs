using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    public virtual int CalcHpRemaining(int dmg) { return Mathf.Max(Health - dmg, 0); }
    public int Health = 100;
    public int HealthMax { get; protected set; }
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

    }

    void Update()
    {
        if (CamSwitcher.I.current_cam != CamSwitcher.BATTLE)
            return;

        DrawingArrow = Controller.I.DrawArrow.phase == InputActionPhase.Performed;
        UpdateTimers(Time.deltaTime);

/*
        if (Controller.I.FireArrow.triggered)
        {
            Vector3 target = Statics.GetScreenCenterWorldPos(CamSwitcher.I.BattleCamCaster, LayerMaskGround);
            if (target != Vector3.zero)
                ShootArrow(LayerMaskTarget, target, 40f);
        }
        */
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

    public void UpdateTimers(float dt)
    {

        if (BlockTimer.Increase(dt))
        {
            Blocking = false;
        }

        if (DrawingArrow && DrawTime < DrawTimeMax)
        {
            DrawTime += dt;
            DrawCharge = DrawTime / DrawTimeMax;

        } else if (!DrawingArrow) {
            if (DrawCharge > 0)
            {
                Vector3 target = Statics.GetScreenCenterWorldPos(CamSwitcher.I.BattleCamCaster, LayerMaskGround);
                if (target != Vector3.zero) 
                {
                    ShootArrow(LayerMaskTarget, target, 40f);
                }
                DrawTime = 0;
                DrawCharge = 0;
            }
        }
    }

    private void SetDrawCharge()
    {

    }

    public void MeleeAttack(LayerMask layerMask)
    {
        //AnimateAttackEffect();
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
    
    public void ToggleDustPs(Vector2 v)
    {
        if (v.magnitude > 0)
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

    public void Die() 
    {
        // Game over scenario

    }

    public virtual int GetAttackDmg() { return AttackDmg; }
    public virtual int GetDefense() { return Defense; }
    public virtual int GetHealth() { return Health; }
}
