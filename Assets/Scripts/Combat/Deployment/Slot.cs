﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.AI;

public class Slot : PhysicsBody
{
    private static readonly Color ColorHealthbar = new Color(.8f, .1f, .1f, .45f);
    private static readonly Color ColorStatbarBg = new Color(.4f, .4f, .4f, .3f);
    private static readonly Color ColorEquipmentText = new Color(1f, .67f, .32f, 1f);
    
    public SpriteRenderer SpriteUnit;
    protected Camera Cam;
    public GameObject Frame;
    public Slider Healthbar;
    public Image HealthbarBg, HealthbarFill;
    public GameObject HealthbarObj;

    public TextMeshProUGUI TName;
    public TextMeshProUGUI THp;

    protected Unit Unit;
    public Group Group;
    public AnimationPlayer AnimationPlayer;
    public ParticleSystem PSDust;
    public Transform SlotPointTransform;
    [SerializeField] private Transform ArrowOriginTransform;
    public Deployment Deployment;
    [SerializeField] public GameObject MeleeAttZone;
    [SerializeField] public GameObject PrefabArrow;
    public event Action<Vector2> OnVelocityChange;
    public Animator Animator;
    private GameObject CharacterModel;
    [SerializeField] private GameObject SwordsmanPrefab, PolearmPrefab, RangerPrefab, MagePrefab, CenterCharPrefab;
    private NavMeshAgent Agent;
    private float MaxVel = 20f;
    private float MaxAcceleration = 20f;

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
        Agent = GetComponent<NavMeshAgent>();

        Agent.updatePosition = false;
        Agent.updateRotation = false;
        MaxVel = Agent.speed;
        MaxAcceleration = Agent.acceleration;
    }

    Vector2 SmoothDeltaPosition = Vector2.zero;
    Vector2 Velocity = Vector2.zero;
    protected virtual void FixedUpdate() {
        OnVelocityChange(Agent.velocity);
        Move();
        Rotate();        
        FaceUIToCam();
    }

    private void Move() 
    {
        Vector3 worldDeltaPosition = Agent.nextPosition - transform.position;

        if (Vector3.Distance(SlotPointTransform.position, transform.position) > 1f)
        {
            Agent.SetDestination(SlotPointTransform.position);
        }

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

        Vector2 normalVelX = new Vector2(Velocity.x, MaxVel).normalized;
        Vector2 normalVelY = new Vector2(Velocity.y, MaxVel).normalized;
        if (Animator != null) {
            Animator.SetFloat("VelocityX", normalVelX.x, .1f, Time.fixedDeltaTime);
            Animator.SetFloat("VelocityZ", normalVelY.x, .1f, Time.fixedDeltaTime);
            Animator.SetFloat("Velocity", Velocity.magnitude, .1f, Time.fixedDeltaTime);
        }

        if (Agent.hasPath) // & arrived
            Agent.acceleration = (Agent.remainingDistance < 4f) ? 4 * MaxAcceleration : MaxAcceleration;
        transform.position = Agent.nextPosition;
    }

    private void Rotate()
    {
        // Correct for rotation error by aligning with deployment.
        if (Agent.remainingDistance < Agent.stoppingDistance)
        {
            //transform.rotation = Deployment.transform.rotation;
            transform.rotation = Quaternion.Lerp(transform.rotation, Deployment.transform.rotation, .15f);
        } else {
            RotateWithDirection(Agent.velocity);
        }
    }

    public virtual bool Fill(Unit u)
    {
        if (u == null)
            return false;
        gameObject.SetActive(true);
        SetUnit(u);
        InitUI(u);
        InstantiateCharacterModel(DetermineCharacterModel(u));
        Animator = CharacterModel.GetComponent<Animator>();
        if (u.IsPlayer)
        {
            gameObject.layer = LayerMask.NameToLayer("Player");
        } else 
        {
            gameObject.layer = LayerMask.NameToLayer("Enemy");
        }
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
        PSDust.Play();
        if (validate)
            Group.ValidateUnitOrder();
        return removedUnit;
    }

    protected virtual void SetUnit(Unit u)
    {
        if (u == null)
            return;
        if (u.IsPlayer)
        {
            Unit = u as PlayerUnit;
        }
        else if (!u.IsPlayer)
        {
            Unit = u as Enemy;
        }
        Unit.SetSlot(this);
    }

    public Unit GetUnit()
    {
        return Unit;
    }

    private GameObject DetermineCharacterModel(Unit unit)
    {   
        if (unit.CombatStyle == Unit.Style.Sword)
        {
            return SwordsmanPrefab;
        } 
        else if (unit.CombatStyle == Unit.Style.Polearm)
        {
            return PolearmPrefab;
        }
        else if (unit.CombatStyle == Unit.Style.Mage)
        {
            return MagePrefab;
        }
        else if (unit.CombatStyle == Unit.Style.Range)
        {
            return RangerPrefab;
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
        arrowScript.Fly(ArrowOriginTransform.position, targetPos, attackDmg);

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
        Healthbar.maxValue = GetUnit().GetDynamicMaxHealth();
        Healthbar.value = GetUnit().Health;
        THp.text = BuildHealthString(GetUnit().Health, 0);
        if (Animator != null)
            Animator.SetFloat("Health", GetUnit().Health);
    }

    public string BuildHealthString(float hp, float previewDamage)
    {
        //float hp = GetUnit().health; // This will already include the boost but not the bonus.
        float hp_boost = GetUnit().get_stat_buff(Unit.HEALTH)
            + GetUnit().GetBonusHealth();

        string str = (hp + GetUnit().get_bonus_from_equipment(Unit.HEALTH)).ToString();
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

    public void PlayAnimationEffect(string anim)
    {
        AnimationPlayer.Play(anim);
    }

    private void ShowEquipmentBoosts()
    {
        if (Unit.IsEnemy)
            return;

        EquipmentInventory ei = TurnPhaser.I.GetDisc(GetPunit().OwnerID).equipment_inventory;
        THp.color = ei.get_stat_boost_amount(Unit.GetID(), Unit.HEALTH) > 0 ?
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
            if (Unit.IsPlayer) return true;
            return false;
        }
    }

    public bool HasEnemy
    {
        get
        {
            if (Unit == null) return false;
            if (Unit.IsEnemy) return true;
            return false;
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
