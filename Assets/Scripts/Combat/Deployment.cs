using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public abstract class Deployment : PhysicsBody
{
    public bool IsPlayer { get; protected set; } = false;
    public bool IsEnemy { get; protected set; } = false;

    public Vector3 Destination;
    public float VelRun = 100f;
    public float VelWalk = 50f;
    public float VelSprint = 350f;
    public float VelMax = 51f;
    public float VelMaxBase { get { return 51f; } set { return; } }
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
            VelMax = Blocking ? VelMaxBase * .5f : VelMaxBase;
        }
    }

    public bool CanMove
    {
        get { return !Attacking && !Stunned && !Blocking; }
    }

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
    }

    protected virtual void Update()
    {
        UpdateSlotTimers(Time.deltaTime);

        if (Rigidbody.velocity.magnitude > VelMax)
            VelMax = Rigidbody.velocity.magnitude;
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


    public void RangeAttack(Group[] zone, Vector3 target_pos)
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
                    u.RangeAttack(target_layer_mask, target_pos);
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
        slider.maxValue = StamMax;
        slider.value = Stamina;

        //float green = ((float)stamina / (float)MAX_STAMINA);
        //staminabar.fillRect.GetComponent<Image>().color = new Color(.1f, .65f, .1f, green);
    }

    private void determine_unit_img_direction(Vector3 dir)
    {
        if ((dir.z > 90 && dir.z < 270) && UnitImgDir == Group.Up)
        {
            FlipSlotImgs(Group.Down);
            UnitImgDir = Group.Down;
        }
        else if ((dir.z <= 90 || dir.z >= 270) && UnitImgDir == Group.Down)
        {
            FlipSlotImgs(Group.Up);
            UnitImgDir = Group.Up;
        }
    }

    public int hp
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

    protected void FlipSlotImgs(int dir)
    {
        foreach (Group[] zone in Zones)
        {
            foreach (Group g in zone)
            {
                g.RotateSprites(dir);
            }
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
                    s.FaceCam();
                }
            }
        }
    }

    public virtual void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
