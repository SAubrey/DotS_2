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
    protected float _player_distance;
    public float PlayerDistance
    {
        get { return _player_distance; }
        set
        {
            _player_distance = Mathf.Max(0, value - PlayerDistanceOffset);
        }
    }
    public float ComfortablePlayerDistanceMax { get => PlayerDistanceOffset + ComfortableDistanceMax; }
    public float ComfortablePlayerDistanceMin { get => PlayerDistanceOffset + ComfortableDistanceMin; }
    protected float PlayerDistanceOffset = 170f;
    protected float ComfortableDistanceMax = 100f;
    protected float ComfortableDistanceMin = 80f;
    //protected float search_distance = 1000f;
    protected float ChaseDistance = 550f;
    //protected float stand_off_distance = 30f;
    public float AttackDistance = 30f;
    protected bool LockedOn = false;
    protected Vector2 Target = Vector2.zero;

    protected StateMachine state_machine = new StateMachine();

    protected void Awake()
    {
        UpdatePlayerPos(GameObject.Find("PlayerSPDeployment").transform.position);
        var roam = new Roam(this);
        var chase = new Chase(this);
        var comfortable = new Comfortable(this);
        var move_to_attack = new MoveToAttack(this);
        var attack = new Attack(this);
        var backing_up = new BackingUp(this);


        // Add transitions from state, to state, if condition.
        At(roam, chase, in_chase_range());
        At(chase, roam, in_roam_range());

        At(chase, comfortable, in_comfy_range());
        At(comfortable, chase, in_chase_range());

        
        At(comfortable, move_to_attack, moving_to_attack());
        At(move_to_attack, attack, time_to_attack());

        At(attack, backing_up, done_attacking());
        At(backing_up, comfortable, in_comfy_range());


        Func<bool> in_roam_range() => () => GetBehaviorZone() == BehaviorZone.Roam;
        Func<bool> in_chase_range() => () => GetBehaviorZone() == BehaviorZone.Chase;
        Func<bool> in_comfy_range() => () => GetBehaviorZone() == BehaviorZone.Comfortable;
        //Func<bool> in_act_range() => () => get_behavior_zone() == BehaviorZone.act;
        Func<bool> moving_to_attack() => () => comfortable.move_to_attack;
        Func<bool> time_to_attack() => () => move_to_attack.begin_attack;
        Func<bool> done_attacking() => () => attack.done_attacking;
        //Func<bool> time_to_attack() => () => move_to_attack.begin_attack;

        void At(IState from, IState to, Func<bool> condition) =>
            state_machine.AddTransition(to, from, condition);

        state_machine.SetState(roam);
    }

    protected override void Update()
    {
        base.Update();
        SetPlayerDistance(PlayerPos);
        state_machine.Tick();
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
        PlayerDistance = Vector3.Distance((Vector2)transform.position, (Vector2)pos);
    }

    public Vector3 GetDirectionToPlayer()
    {
        return Statics.Direction(gameObject.transform.position, PlayerPos);
    }
}
