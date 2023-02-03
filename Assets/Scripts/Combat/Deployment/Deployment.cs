using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Deployment : AgentBody
{
    public bool IsPlayer { get; protected set; } = false;
    public bool IsEnemy { get; protected set; } = false;
    protected Slot LockedOnTarget;

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

    protected List<Group[]> Zones = new List<Group[]>();
    public LayerMask target_layer_mask;
    
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

    public virtual void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
