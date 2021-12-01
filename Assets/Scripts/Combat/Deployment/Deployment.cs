using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.AI;

public abstract class Deployment : PhysicsBody
{
    public bool IsPlayer { get; protected set; } = false;
    public bool IsEnemy { get; protected set; } = false;

    protected NavMeshAgent Agent;
    public float VelRun = 30f;
    [HideInInspector] public float VelWalk = 10f;
    [HideInInspector] public float VelSprint = 50f;
    public float VelMax = 25f;
    public float VelMaxDynamic = 25f;

    protected bool Invulnerable;

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
                UpdateSlider(staminabar, StamMax, Stamina);
            }
        }
    }

    public bool Stunned = false;
    public bool Attacking = false;
    private bool _Blocking = false;
    public bool Blocking {
        get { return _Blocking; }
        set {
            _Blocking = value;
            VelMaxDynamic = Blocking ? VelMax * .5f : VelMax;
        }
    }

    public bool CanMove { get { return !Attacking && !Stunned && !Blocking; } }

    public float UnitImgDir = Group.Up;
    protected float StamRegenAmount = .1f;
    protected float StamAttackCost = 20f;
    protected float StamRangeCost = 25f;
    protected float StamBlockCost = 20f;

    protected List<Group[]> Zones = new List<Group[]>();
    public LayerMask target_layer_mask;

    private Timer StamRegenTimer = new Timer(.0005f);
    public abstract Group[] GetAttackingZone(bool melee);

    public abstract void PlaceUnit(Unit unit);

    protected virtual void AnimateSlotAttack(bool melee) { }

    protected override void Awake() 
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();
    }

    protected virtual void Update()
    {
        UpdateSlotTimers(Time.deltaTime);
    }

    protected virtual void FixedUpdate()
    {
        if (CanMove)
        {
            if (StamRegenTimer.Increase(Time.fixedDeltaTime) &&
                Stamina <= StamMax - StamRegenAmount)
            {
                RegenStamina(StamRegenAmount);
            }
        }
    }

    public void MoveAgentToLocation(Vector3 pos) 
    {
        Agent.SetDestination(pos);
    }

    protected Quaternion TargetRotation;
    public void RotateTowardsTarget(Vector3 Target)
    {
        //TargetRotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2((pos.z - transform.position.z),
         //   (pos.x - transform.position.x)) * Mathf.Rad2Deg - 90f));

        // Lerp smooths and thus limits rotation speed.
        //float str = Mathf.Min(5f * Time.deltaTime, 1f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, str);
        //Rigidbody.rotation = Quaternion.Lerp(transform.rotation, TargetRotation, str);
        transform.LookAt(Target);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        //RotateRigidbodyToTarget(Rigidbody, Target);
        FaceSlotsToCamera();
    }

    public void UpdateSlotTimers(float dt)
    {
        foreach (Group[] gs in Zones)
        {
            foreach (Group g in gs)
            {
                foreach (Slot s in g.Slots)
                {
                    if (s.HasUnit)
                        s.GetUnit().UpdateTimers(dt);
                }
            }
        }
    }

    public void MeleeAttack(Group[] zone)
    {
        if (Stamina < StamAttackCost)
            return;

        Stamina -= StamAttackCost;
        foreach (Group g in zone)
        {
            for (int i = 0; i < 3; i++)
            {
                if (g.Slots[i].HasUnit)
                {
                    g.Slots[i].GetUnit().MeleeAttack(target_layer_mask);
                }
            }
        }
    }


    public void RangeAttack(Group[] zone, Vector3 targetPos)
    {
        if (Stamina < StamRangeCost)
            return;

        Stamina -= StamRangeCost;
        foreach (Group g in zone)
        {
            for (int i = 0; i < 3; i++)
            {
                if (g.Slots[i].HasUnit)
                {
                    Unit u = g.Slots[i].GetUnit();
                    u.RangeAttack(target_layer_mask, targetPos);
                }
            }
        }
    }

    public Group GetHighestEmptyGroup(Group[] groups)
    {
        foreach (Group g in groups)
        {
            if (!g.IsFull)
            {
                return g;
            }
        }
        return null;
    }

    public void RegenStamina(float amount)
    {
        if (Stamina >= StamMax || Blocking)
            return;
        //stamina += StaticOps.GetAdjustedIncrease(stamina, amount, MAX_STAMINA);
        Stamina += amount;
    }

    public void UpdateSlider(Slider slider, float maxValue, float value)
    {
        slider.maxValue = maxValue;
        slider.value = value;

        //float green = ((float)stamina / (float)MAX_STAMINA);
        //staminabar.fillRect.GetComponent<Image>().color = new Color(.1f, .65f, .1f, green);
    }

    public int CollectiveHealth
    {
        get
        {
            int sum = 0;
            foreach (Group[] zone in Zones)
            {
                foreach (Group g in zone)
                {
                    sum += g.SumUnitHealth();
                }
            }
            return sum;
        }
    }

    protected void FaceSlotsToCamera()
    {
        foreach (Group[] zone in Zones)
        {
            foreach (Group g in zone)
            {
                foreach (Slot s in g.Slots)
                {
                    s.FaceUIToCam();
                }
            }
        }
    }

    public virtual void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
