using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Deployment : AgentBody
{
    public bool IsPlayer { get; protected set; } = false;
    public bool IsEnemy { get; protected set; } = false;
    protected Slot LockedOnTarget;

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
            //Agent.ve = Blocking ? MaxSpeed * .5f : MaxSpeed;
        }
    }

    public bool CanMove { get { return !Attacking && !Stunned && !Blocking; } }

    protected float StamRegenAmount = .1f;
    protected float StamAttackCost = 20f;
    protected float StamRangeCost = 10f;
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
        Agent.updateRotation = false;
    }

    protected override void Update()
    {
        base.Update();
        UpdateSlotTimers(Time.deltaTime);
        if (LockedOnTarget != null)
        {
            Statics.RotateToPoint(transform, LockedOnTarget.transform.position);
        }
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

    public void UpdateSlotTimers(float dt)
    {
        foreach (Group[] gs in Zones)
        {
            foreach (Group g in gs)
            {
                foreach (Slot s in g.Slots)
                {
                    if (s.HasUnit)
                        s.Unit.UpdateTimers(dt);
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
                    g.Slots[i].Unit.MeleeAttack(target_layer_mask);
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
            for (int i = 0; i < g.Slots.Count; i++)
            {
                if (g.Slots[i].HasUnit)
                {
                    Unit u = g.Slots[i].Unit;
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
    }

    public virtual void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
