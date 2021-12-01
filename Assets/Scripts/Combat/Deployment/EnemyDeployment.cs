using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public abstract class EnemyDeployment : Deployment
{
    public enum BehaviorZone
    {
        Roam, Chase, Comfortable, Act, MoveToAttack
    }
    // Melee enemy will chase to safe distance then move in for attack.
    // Enemy returns to safe distance after attack, block, or stun
    
    public Vector3 PlayerPos;
    protected float _playerDistance;
    public float PlayerDistance
    {
        get { return _playerDistance; }
        set
        {
            _playerDistance = Mathf.Max(0, value - PlayerDistanceOffset);
        }
    }
    public float ComfortablePlayerDistanceMax { get => PlayerDistanceOffset + ComfortableDistanceMax; }
    public float ComfortablePlayerDistanceMin { get => PlayerDistanceOffset + ComfortableDistanceMin; }
    protected float PlayerDistanceOffset = 20f;
    protected float ComfortableDistanceMax = 70f;
    protected float ComfortableDistanceMin = 50f;
    //protected float search_distance = 1000f;
    protected float ChaseDistance = 300f;
    //protected float stand_off_distance = 30f;
    public float AttackDistance = 30f;
    protected bool LockedOn = false;
    protected Vector2 Target = Vector2.zero;

    protected StateMachine StateMachine = new StateMachine();

    protected override void Awake()
    {
        base.Awake();
        Agent = GetComponent<NavMeshAgent>();

        UpdatePlayerPos(GameObject.Find("PlayerSPDeployment").transform.position);
        var roam = new Roam(this);
        var chase = new Chase(this);
        var comfortable = new Comfortable(this);
        var moveToAttack = new MoveToAttack(this);
        var attack = new Attack(this);
        var backingUp = new BackingUp(this);


        // Add transitions from state, to state, if condition.
        At(roam, chase, in_chase_range());
        At(chase, roam, in_roam_range());

        At(chase, comfortable, in_comfy_range());
        At(comfortable, chase, in_chase_range());

        
        At(comfortable, moveToAttack, moving_to_attack());
        At(moveToAttack, attack, time_to_attack());

        At(attack, backingUp, done_attacking());
        At(backingUp, comfortable, in_comfy_range());


        Func<bool> in_roam_range() => () => GetBehaviorZone() == BehaviorZone.Roam;
        Func<bool> in_chase_range() => () => GetBehaviorZone() == BehaviorZone.Chase;
        Func<bool> in_comfy_range() => () => GetBehaviorZone() == BehaviorZone.Comfortable;
        //Func<bool> in_act_range() => () => get_behavior_zone() == BehaviorZone.act;
        Func<bool> moving_to_attack() => () => comfortable.MoveToAttack;
        Func<bool> time_to_attack() => () => moveToAttack.begin_attack;
        Func<bool> done_attacking() => () => attack.DoneAttacking;
        //Func<bool> time_to_attack() => () => move_to_attack.begin_attack;

        void At(IState from, IState to, Func<bool> condition) =>
            StateMachine.AddTransition(to, from, condition);

        StateMachine.SetState(roam);
    }

    protected override void Update()
    {
        base.Update();
        SetPlayerDistance(PlayerPos);
        StateMachine.Tick();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected void Init()
    {
        PlayerDeployment.I.OnPositionChange += UpdatePlayerPos;
        IsEnemy = true;
    }

    private void UpdatePlayerPos(Vector3 pos)
    {
        PlayerPos = pos;
    }

    public BehaviorZone GetBehaviorZone()
    {
        float pd = PlayerDistance;
        if (pd > ChaseDistance)
            return BehaviorZone.Roam;
        if (pd <= ChaseDistance && pd > ComfortablePlayerDistanceMax)
            return BehaviorZone.Chase;
        if (pd <= ComfortablePlayerDistanceMax && pd >= ComfortablePlayerDistanceMin)
            return BehaviorZone.Comfortable;
        if (pd < ComfortablePlayerDistanceMin)
            return BehaviorZone.Act;
        return BehaviorZone.Roam;
    }

    public virtual void BackUp()
    {
        MoveInDirection(-GetDirectionToPlayer(), VelRun, PhysicsBody.MoveForce);
    }


    protected void SetPlayerDistance(Vector3 pos)
    {
        PlayerDistance = Vector3.Distance(transform.position, pos);
    }

    public Vector3 GetDirectionToPlayer()
    {
        return Statics.CalcDirection(gameObject.transform.position, PlayerPos);
    }
}
