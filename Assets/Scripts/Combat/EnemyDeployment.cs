using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;

public abstract class EnemyDeployment : Deployment
{
    public enum BehaviorZone
    {
        roam, chase, comfortable, act, move_to_attack
    }
    // Melee enemy will chase to safe distance then move in for attack.
    // Enemy returns to safe distance after attack, block, or stun
    public Vector2 player_pos;
    protected float _player_distance;
    public float player_distance
    {
        get { return _player_distance; }
        set
        {
            _player_distance = Mathf.Max(0, value - player_distance_offset);
        }
    }
    public float comfortable_player_distance_max { get => player_distance_offset + comfortable_distance_max; }
    public float comfortable_player_distance_min { get => player_distance_offset + comfortable_distance_min; }
    protected float player_distance_offset = 170f;
    protected float comfortable_distance_max = 100f;
    protected float comfortable_distance_min = 80f;
    //protected float search_distance = 1000f;
    protected float chase_distance = 550f;
    //protected float stand_off_distance = 30f;
    public float attack_distance = 30f;
    protected bool locked_on = false;
    protected Vector2 target = Vector2.zero;

    protected StateMachine state_machine = new StateMachine();

    protected void Awake()
    {
        update_player_pos(GameObject.Find("PlayerSPDeployment").transform.position);
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


        Func<bool> in_roam_range() => () => get_behavior_zone() == BehaviorZone.roam;
        Func<bool> in_chase_range() => () => get_behavior_zone() == BehaviorZone.chase;
        Func<bool> in_comfy_range() => () => get_behavior_zone() == BehaviorZone.comfortable;
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
        set_player_distance(player_pos);
        state_machine.Tick();
    }

    
    protected override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    protected void init()
    {
        PlayerDeployment.I.on_position_change += update_player_pos;
        isEnemy = true;
    }

    private void update_player_pos(Vector2 pos)
    {
        player_pos = pos;
    }

    public BehaviorZone get_behavior_zone()
    {
        float pd = player_distance;
        if (pd > chase_distance)
            return BehaviorZone.roam;
        if (pd <= chase_distance && pd > comfortable_player_distance_max)
            return BehaviorZone.chase;
        if (pd <= comfortable_player_distance_max && pd >= comfortable_player_distance_min)
            return BehaviorZone.comfortable;
        if (pd < comfortable_player_distance_min)
            return BehaviorZone.act;
        return BehaviorZone.roam;
    }

    public virtual void back_up()
    {
        Move(-get_direction_to_player(), VEL_RUN, PhysicsBody.MoveForce);
    }

    public void wander()
    {
        // Move to a random target location at a random distance

    }

    protected Quaternion target_rotation;
    public void rotate_towards_target(Vector3 pos)
    {
        target_rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2((pos.y - transform.position.y),
            (pos.x - transform.position.x)) * Mathf.Rad2Deg - 90f));

        // Lerp smooths and thus limits rotation speed.
        float str = Mathf.Min(5f * Time.deltaTime, 1f);
        transform.rotation = Quaternion.Lerp(transform.rotation, target_rotation, str);
        face_slots_to_camera();
    }

    protected void set_player_distance(Vector3 pos)
    {
        player_distance = Vector2.Distance((Vector2)transform.position, (Vector2)pos);
    }

    public Vector3 get_direction_to_player()
    {
        return Statics.Direction(gameObject.transform.position, player_pos);
    }
}
