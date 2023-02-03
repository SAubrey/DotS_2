using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class AIBrain : MonoBehaviour
{
    public enum BehaviorZone
    {
        Follow, Roam, Chase, Comfortable, Act, MoveToAttack
    }

    public Slot Slot;
    public Transform Target;
    public Vector3 TargetPos {
        get { 
            if (Target == null)
                return Vector3.zero;
            return Target.position;
        }
        private set {}
    }
    public float TargetDistance
    {
        get { 
            if (Target == null)
                return 9999f;
            return Vector3.Distance(transform.position, Target.position); 
        }
        private set {}
    }

    protected StateMachine StateMachine = new StateMachine();
    protected float ComfortableDistanceMin = 20f;
    protected float ComfortableDistanceMax = 60f;
    protected float TargetDistanceOffset = 5f; // Should never intend to be closer than this.
    public float ComfortableTargetDistanceMax { get => TargetDistanceOffset + ComfortableDistanceMax; }
    public float ComfortableTargetDistanceMin { get => TargetDistanceOffset + ComfortableDistanceMin; }
    protected float ChaseDistance = 200f;
    public float AttackDistance = 5f;
    protected bool LockedOn = false;
    public bool TookHit = false;
    public bool Attacking = false;
    public bool CanRetarget { get => !Attacking; }
    // Can retarget: Not attacking or movign to attack

    protected virtual void Init() {}

    protected virtual void Awake()
    {
        FillStateMachine();
    }

    protected virtual void Update()
    {
        HandleTargeting();
        StateMachine.Tick();
        TookHit = false;
    }

    protected virtual void FixedUpdate() {}

    protected virtual void FillStateMachine() {}
    
    protected void At(IState from, IState to, Func<bool> condition) =>
            StateMachine.AddTransition(to, from, condition);

    /*
    Units continuously attempt to retarget, and Target is null if none are in range.
    */
    protected void HandleTargeting()
    {
        if (CanRetarget)
        {
            Target = FindNearestEnemyInSight();
        } 
    }

    private Transform FindNearestEnemyInSight()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, ChaseDistance, LayerMask.GetMask(Slot.Unit.TargetMask));
        Transform nearest = null;
        float nearestDistance = Mathf.Infinity;
        float distance = 0;
        foreach (Collider hit in hits)
        {
            //if (s.Unit.MyMask == Slot.Unit.MyMask)
                //continue;
            distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearest = hit.transform;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    public BehaviorZone GetBehaviorZone()
    {
        float td = TargetDistance;
        if (td > ChaseDistance)
            return BehaviorZone.Roam;
        if (td <= ChaseDistance && td > ComfortableTargetDistanceMax)
            return BehaviorZone.Chase;
        if (td <= ComfortableTargetDistanceMax && td >= ComfortableTargetDistanceMin)
            return BehaviorZone.Comfortable;
        if (td < ComfortableTargetDistanceMin)
            return BehaviorZone.Act;
        return BehaviorZone.Roam;
    }

    public void SetDestinationToTarget()
    {
        Slot.SetAgentDestination(TargetPos);
    }

    public Vector3 GetDirectionToTarget()
    {
        return Statics.CalcDirection(transform.position, TargetPos);
    }

    public virtual void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
