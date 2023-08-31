using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// A slot does not store any data about its unit, and is just a MonoBehaviour housing.
public class Slot : AgentBody
{
    private static readonly Color ColorHealthbar = new Color(.8f, .1f, .1f, .45f);
    private static readonly Color ColorStatbarBg = new Color(.4f, .4f, .4f, .3f);
    private static readonly Color ColorEquipmentText = new Color(1f, .67f, .32f, 1f);
    
    protected Camera Cam;
    public GameObject Frame;
    public Slider Healthbar;
    public Image HealthbarBg, HealthbarFill;
    public GameObject HealthbarObj;

    public TextMeshProUGUI TName, THp;

    public Unit Unit { get; protected set; }
    public Group Group;
    public ParticleSystem PSDust, PSBlood;
    public Transform SlotPointTransform;
    [SerializeField] private Transform ArrowOriginTransform;
    public Deployment Deployment;
    [SerializeField] public GameObject MeleeAttackPoint;
    [SerializeField] public Collider MeleeTriggerBox;
    [SerializeField] public GameObject PrefabArrow;
    public Animator Animator;
    private GameObject CharacterModel;
    [SerializeField] private GameObject SwordsmanPrefab, PolearmPrefab, RangerPrefab, MagePrefab, CenterCharPrefab;
    [SerializeField] private GameObject EyelessPrefab;
    public event Action<Vector2> OnVelocityChange;
    private Slot LockedOnTarget;
    public AIBrain Brain;
    protected float BlockSpeed = 1.8f;
    protected float ArrowLaunchSpeed = 70f;
    private bool Dead = false;

    protected override void Awake()
    {
        base.Awake();
        Cam = GameObject.Find("BattleCamera").GetComponent<Camera>();
        FaceUIToCam();
        gameObject.SetActive(false);
    }

    protected virtual void Start()
    {
        HealthbarFill.color = ColorHealthbar;
        HealthbarBg.color = ColorStatbarBg;
        OnVelocityChange += ToggleDustPs;
        PSDust.Pause();

        Agent.updatePosition = false;
        Agent.updateRotation = false;
    }

    Vector2 SmoothDeltaPosition = Vector2.zero;
    Vector2 Velocity = Vector2.zero;
    Vector3 worldDeltaPosition;
    Vector3 PreviousPosition, CurrentPosition = new Vector3();
    protected virtual void FixedUpdate() {
        FaceUIToCam();
        if (Unit.IsDead)
        {
            return;
        }
        OnVelocityChange(Agent.velocity);
        Move();
    }

    private void Move() 
    {
        worldDeltaPosition = Agent.nextPosition - transform.position;
        MaxSpeed = DetermineMoveSpeed(); // Agent speed is its max speed after acceleration.
/*
        if (Vector3.Distance(SlotPointTransform.position, transform.position) > 1f)
        {
            Agent.SetDestination(SlotPointTransform.position);
        }*/

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2 (dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime/0.15f);
        SmoothDeltaPosition = Vector2.Lerp(SmoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            Velocity = SmoothDeltaPosition / Time.deltaTime;

        UpdateAnimatorVelocity();
        Rotate(Velocity);
        GenerateMovementEffects(Velocity.magnitude);

        if (Agent.hasPath) // & arrived
            Agent.acceleration = (Agent.remainingDistance < 4f) ? 4 * MaxAcceleration : MaxAcceleration;
        transform.position = Agent.nextPosition;
    }

    private void UpdateAnimatorVelocity()
    {
        // Update velocity animation based on previous frame. Useful?
        Vector3 v = (transform.position - PreviousPosition) / Time.deltaTime;
        v = transform.InverseTransformDirection(v); // Convert to local space.
        PreviousPosition = transform.position;

        // Send animator speed from 0-1 relative to max speed
        //Vector2 normalVelX = new Vector2(Velocity.x, MaxSpeed).normalized;
        //Vector2 normalVelY = new Vector2(Velocity.y, MaxSpeed).normalized;
        Vector2 normalVelX = new Vector2(v.x, MaxSpeedStatic).normalized;
        Vector2 normalVelY = new Vector2(v.z, MaxSpeedStatic).normalized;
        if (Animator != null) {
            Animator.SetFloat("VelocityX", normalVelX.x, .1f, Time.deltaTime);
            Animator.SetFloat("VelocityZ", normalVelY.x, .1f, Time.deltaTime);
            Animator.SetFloat("Velocity", Velocity.magnitude, .1f, Time.deltaTime);
        }
    }

    private float DetermineMoveSpeed() 
    {
        if (Unit.Blocking)
        {
            return BlockSpeed;
        } else if (Unit.IsPlayer)
        {
            return Player.I.Sprinting ? Player.I.PlayerController.SprintSpeed : Player.I.PlayerController.MoveSpeed;
        }
        return MaxSpeed;
    }

    private float rotationVelocity;
    private void Rotate(Vector2 velocity)
    {
        Vector3 targetDirection = new Vector3();
        // Correct for rotation error by aligning with deployment.
       /* if (Agent.remainingDistance < Agent.stoppingDistance)
        {
            //transform.rotation = Deployment.transform.rotation;
            if (Deployment != null)
                transform.rotation = Quaternion.Lerp(transform.rotation, Deployment.transform.rotation, .15f);
            return;
        }
        //float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);
			//transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);

        if (LockedOnTarget != null)
        {
            Statics.RotateToPoint(transform, LockedOnTarget.transform.position);
        } else {
            Statics.RotateWithVelocity(transform, Agent.velocity);
        }*/

        // If targeting an enemy, always face it. Otherwise rotate to the direction of travel.
        if (Brain.Target)
        {
            targetDirection = Brain.GetDirectionToTarget();
            
        } else
        {
            //Statics.RotateWithVelocity(transform, Agent.velocity);
            targetDirection = Agent.velocity.normalized;
            
        }
        float targetRotation = Mathf.Atan2(targetDirection.x, targetDirection.z) * Mathf.Rad2Deg;// + _mainCamera.transform.eulerAngles.y;
        float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref rotationVelocity, .12f);
		transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
        
    }

    // Melee attack collided with enemy
    /*void OnTriggerEnter(Collider collider)
    {
        Debug.Log("Obj in trigger zone");
        if (gameObject.layer == collider.gameObject.layer || 
        !(Statics.ColliderIsLayer(collider, "Player") || 
        Statics.ColliderIsLayer(collider, "Enemy") || 
        Statics.ColliderIsLayer(collider, "Slot")))
        {
            MeleeTriggerBox.enabled = false;
            return;
        }

        Slot s = collider.GetComponentInParent<Slot>();
        if (s != null)
        {
                Unit u = s.Unit;
                if (u != null)
                {
                    u.TakeDamage(Unit.GetAttackDmg());
                }
        }

        Player p = collider.GetComponent<Player>();
        if (p != null)
        {
            p.TakeDamage(Unit.GetAttackDmg());
        }

        MeleeTriggerBox.enabled = false;
    }*/

    public void OnCollisionEnter(Collision collision)
    {
        Collider collider = collision.collider;
        Debug.Log("Obj in trigger zone");
        if (gameObject.layer == collider.gameObject.layer || 
        !(Statics.ColliderIsLayer(collider, "Player") || 
        Statics.ColliderIsLayer(collider, "Enemy") || 
        Statics.ColliderIsLayer(collider, "Slot")))
        {
            MeleeTriggerBox.enabled = false;
            return;
        }

        Slot s = collider.GetComponentInParent<Slot>();
        if (s != null)
        {
                Unit u = s.Unit;
                if (u != null)
                {
                    u.TakeDamage(Unit.GetAttackDmg());
                }
        }

        Player p = collider.GetComponent<Player>();
        if (p != null)
        {
            p.TakeDamage(Unit.GetAttackDmg());
        }
        Unit.MeleeAttack();
        MeleeTriggerBox.enabled = false;
    }

    public void AnimationEventMeleeAttack()
    {
        Debug.Log("ANIMATION EVENT TRIGGERED ######################################################");
        
    }

    // Add a unit to the empty slot.
    public virtual bool Fill(Unit u)
    {
        if (u == null)
            return false;
        gameObject.SetActive(true);
        SetUnit(u);
        InitUI(u);
        InstantiateCharacterModel(DetermineCharacterModel(u));
        Animator = CharacterModel.GetComponent<Animator>();
        gameObject.layer = LayerMask.NameToLayer(Unit.MyMask);

        if (u.IsPlayer)
        {
            Brain = gameObject.AddComponent<AIBrainPlayer>();
        } else 
        {
            Brain = gameObject.AddComponent<AIBrainEnemy>();
        }
        Unit.Brain = Brain;
        Brain.Slot = this;
        Brain.enabled = true;
        MaxSpeed = u.Speed;
        MaxSpeedStatic = u.Speed;
        return true;
    }

    // Full slots below the removed slot will be moved up if validated.
    public Unit Empty(bool validate = true)
    {
        Unit removedUnit = Unit;
        if (Unit != null)
        {
            Unit.SetSlot(null);
            Unit = null;
        }
        gameObject.SetActive(false);

        SetActiveUI(false);
        SetNameT("");
        GameObject.Destroy(CharacterModel);
        Animator = null;
        PSDust.Stop();
        if (validate)
            Group.ValidateUnitOrder();
        return removedUnit;
    }

    protected virtual void SetUnit(Unit u)
    {
        if (u == null)
            return;
        Unit = u.IsPlayer ? u as PlayerUnit : u as Enemy;
        Unit.SetSlot(this);
    }

    private void LockOn(Slot slot) 
    {
        LockedOnTarget = slot;
    }

    private void GenerateMovementEffects(float velocity)
    {
        if (velocity <= 2f)
        {
            PSDust.gameObject.SetActive(false);
        } else 
        {
            PSDust.gameObject.SetActive(true);
        }
    }

    private GameObject DetermineCharacterModel(Unit unit)
    {   
        if (unit.CombatStyle == global::Unit.Style.Sword)
        {
            return SwordsmanPrefab;
        } 
        else if (unit.CombatStyle == global::Unit.Style.Polearm)
        {
            return PolearmPrefab;
        }
        else if (unit.CombatStyle == global::Unit.Style.Mage)
        {
            return MagePrefab;
        }
        else if (unit.CombatStyle == global::Unit.Style.Range)
        {
            return RangerPrefab;
        } 
        else if (unit.CombatStyle == global::Unit.Style.Claw)
        {
            return EyelessPrefab;
        }
        return CenterCharPrefab;
    }

    private void InstantiateCharacterModel(GameObject modelPrefab) {
        CharacterModel = Instantiate(modelPrefab, gameObject.transform);
        Animator = CharacterModel.GetComponent<Animator>();
    }

    public void ShootArrow(LayerMask mask, Vector3 targetPos, float attackDmg)
    {
        GameObject a = Instantiate(PrefabArrow, ArrowOriginTransform);
        //a.transform.position = ArrowOriginTransform.position;
        a.transform.localPosition = Vector3.zero;
        Arrow arrowScript = a.GetComponent<Arrow>();
        arrowScript.Fly(ArrowOriginTransform.position, targetPos, attackDmg, ArrowLaunchSpeed);

        if (Animator != null)
            Animator.SetTrigger("Attack");
    }

    public void InitUI(Unit u)
    {
        ShowEquipmentBoosts();
        SetNameT(Unit.GetName());
        SetActiveUI(true);
        UpdateTextUI();
    }

    public void ToggleDustPs(Vector2 v)
    {
        if (IsEmpty)
        {
            return;
        }
        if (v.magnitude > 0)
        {
            PSDust.Play();
        } else 
        {
            PSDust.Pause();
        }
    }

    // Updated when a boost is removed or applied,
    // or an attribute is activated or deactivated.
    public void UpdateTextUI()
    {
        UpdateHealthbar();
    }

    public void UpdateHealthbar()
    {
        Healthbar.maxValue = Unit.GetDynamicMaxHealth();
        Healthbar.value = Unit.Health;
        THp.text = BuildHealthString(Unit.Health, 0);
        if (Animator != null)
            Animator.SetFloat("Health", Unit.Health);
    }

    public string BuildHealthString(float hp, float previewDamage)
    {
        //float hp = GetUnit().health; // This will already include the boost but not the bonus.
        float hp_boost = Unit.get_stat_buff(global::Unit.HEALTH)
            + Unit.GetBonusHealth();

        string str = (hp + Unit.get_bonus_from_equipment(global::Unit.HEALTH)).ToString();
        if (previewDamage > 0)
            str += " (-" + previewDamage.ToString() + ")";
        if (hp_boost > 0)
            str += " (+" + hp_boost.ToString() + ")";
        return str;
    }

    public void SetActiveUI(bool state)
    {
        HealthbarObj.SetActive(state);
    }

    private void ShowEquipmentBoosts()
    {
        if (Unit.IsEnemy)
            return;

        EquipmentInventory ei = TurnPhaser.I.GetDisc(GetPunit().OwnerID).EquipmentInventory;
        THp.color = ei.get_stat_boost_amount(Unit.ID, Unit.HEALTH) > 0 ?
            ColorEquipmentText : Color.white;
    }

    public virtual void FaceUIToCam()
    {
        Frame.transform.LookAt(Cam.transform);
        Frame.transform.forward *= -1;
    }

    public bool HasPunit
    {
        get
        {
            if (Unit == null) return false;
            return Unit.IsPlayer;
        }
    }

    public bool HasEnemy
    {
        get
        {
            if (Unit == null) return false;
            return Unit.IsEnemy;
        }
    }

    public bool HasUnit
    {
        get { return Unit != null ? true : false; }
    }

    public bool IsEmpty
    {
        get { return Unit == null ? true : false; }
    }

    public PlayerUnit GetPunit()
    {
        return HasPunit ? Unit as PlayerUnit : null;
    }

    public Enemy GetEnemy()
    {
        return HasEnemy ? Unit as Enemy : null;
    }

    private void SetNameT(string txt)
    {
        TName.text = txt;
    }
}
