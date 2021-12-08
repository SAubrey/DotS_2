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
    protected float PlayerDistanceOffset = 5f; // Enemy should never intend to be closer than this.
    protected float ComfortableDistanceMax = 40f;
    protected float ComfortableDistanceMin = 20f;
    protected float ChaseDistance = 200f;
    public float AttackDistance = 7f;
    protected bool LockedOn = false;
    protected StateMachine StateMachine = new StateMachine();

    protected override void Awake()
    {
        base.Awake();
        IsEnemy = true;

        UpdatePlayerPos(GameObject.Find("PlayerSPDeployment").transform.position);
        FillStateMachine();
    }

    protected void Init()
    {
        PlayerDeployment.I.OnPositionChange += UpdatePlayerPos;
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

    private void FillStateMachine()
    {
        var roam = new Roam(this);
        var chase = new Chase(this);
        var comfortable = new Comfortable(this);
        var moveToAttack = new MoveToAttack(this);
        var attack = new Attack(this);
        var backingUp = new BackingUp(this);

        // Add transitions from state, to state, if condition.
        At(roam, chase, InChaseRange());
        
        At(chase, roam, InRoamRange());
        //At(chase, backingUp, InActRange());
        At(chase, comfortable, InComfyRange());
        
        At(comfortable, chase, InChaseRange());
        At(comfortable, moveToAttack, MoveToAttack());

        StateMachine.AddAnyTransition(attack, TimeToAttack());
        At(moveToAttack, attack, TimeToAttack());

        //At(attack, backingUp, DoneAttacking());

        StateMachine.AddAnyTransition(backingUp, BackUp());
        At(backingUp, comfortable, InComfyRange());

        Func<bool> InRoamRange() => () => GetBehaviorZone() == BehaviorZone.Roam;
        Func<bool> InChaseRange() => () => GetBehaviorZone() == BehaviorZone.Chase;
        Func<bool> InComfyRange() => () => GetBehaviorZone() == BehaviorZone.Comfortable;
        //Func<bool> InActRange() => () => GetBehaviorZone() == BehaviorZone.Act;
        Func<bool> BackUp() => () => GetBehaviorZone() == BehaviorZone.Act && !moveToAttack.BeginAttack && !comfortable.MoveToAttack && !attack.Attacking;
        Func<bool> MoveToAttack() => () => comfortable.MoveToAttack;
        Func<bool> TimeToAttack() => () => moveToAttack.BeginAttack;
        //Func<bool> DoneAttacking() => () => !attack.Attacking;
        //Func<bool> time_to_attack() => () => move_to_attack.begin_attack;

        void At(IState from, IState to, Func<bool> condition) =>
            StateMachine.AddTransition(to, from, condition);

        StateMachine.SetState(roam);
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

    private void UpdatePlayerPos(Vector3 pos)
    {
        PlayerPos = pos;
    }

    protected void SetPlayerDistance(Vector3 pos)
    {
        PlayerDistance = Vector3.Distance(transform.position, pos);
    }

    public Vector3 GetDirectionToPlayer()
    {
        return Statics.CalcDirection(transform.position, PlayerPos);
    }

    public override void Delete()
    {
        PlayerDeployment.I.OnPositionChange -= UpdatePlayerPos;
        GameObject.Destroy(gameObject);
    }
}
