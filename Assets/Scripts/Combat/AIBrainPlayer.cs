using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIBrainPlayer : AIBrain
{
    float TooFarFromPlayerDistance = 35f;
    
    protected override void Awake()
    {
        base.Awake();

        ChaseDistance = 30f;
    }
    
    protected override void Update()
    {
        base.Update();
    }

    protected override void FillStateMachine()
    {

        /*
        If an enemy is close enough, break follow and approach enemy with shield up.
        Attack after a block or hit, then return to blocking
        At any time, Return to player if player moves too far away.
        When returning, return completely to start position before being able to reengage.
        While returning, can face enemy and block if an attack is imminent.
        */
        var follow = new FollowDeployment(this);
        var blockAndApproach = new BlockAndApproach(this);
        var attack = new Retaliate(this);
        

        At(follow, blockAndApproach, CanApproach());
        At(blockAndApproach, attack, TookHitTrue());

        At(attack, blockAndApproach, DoneAttacking());


        StateMachine.AddAnyTransition(follow, TooFarFromPlayer());

        Func<bool> CanApproach() => () => TargetDistance <= ChaseDistance && Target != null && !follow.Returning;
        Func<bool> TookHitTrue() => () => TookHit == true;
        Func<bool> DoneAttacking() => () => !attack.Attacking;

        Func<bool> TooFarFromPlayer() => () => GetPlayerDistance() > TooFarFromPlayerDistance;// && !attack.Attacking;
        /*var chase = new Chase(this);
        var comfortable = new Comfortable(this);
        var moveToAttack = new MoveToAttack(this);
        var attack = new Attack(this);
        var backingUp = new BackingUp(this);

        // Add transitions from state, to state, if condition.
        StateMachine.AddAnyTransition(follow, );

        At(follow, chase, InChaseRange());
        
        At(chase, follow, InFollowRange());
        //At(chase, backingUp, InActRange());
        
        At(comfortable, chase, InChaseRange());
        At(comfortable, moveToAttack, MoveToAttack());

        StateMachine.AddAnyTransition(attack, TimeToAttack());
        At(moveToAttack, attack, TimeToAttack());

        //At(attack, backingUp, DoneAttacking());

        StateMachine.AddAnyTransition(backingUp, BackUp());
        At(backingUp, follow, AlwaysFollow());

        Func<bool> InFollowRange() => () => GetBehaviorZone() == BehaviorZone.Follow;
        Func<bool> AlwaysFollow() => () => true;
        Func<bool> InChaseRange() => () => GetBehaviorZone() == BehaviorZone.Chase;
        //Func<bool> InActRange() => () => GetBehaviorZone() == BehaviorZone.Act;
        Func<bool> BackUp() => () => GetBehaviorZone() == BehaviorZone.Act && !moveToAttack.BeginAttack && !comfortable.MoveToAttack && !attack.Attacking;
        Func<bool> MoveToAttack() => () => comfortable.MoveToAttack;
        Func<bool> TimeToAttack() => () => moveToAttack.BeginAttack;
        //Func<bool> DoneAttacking() => () => !attack.Attacking;
        //Func<bool> time_to_attack() => () => move_to_attack.begin_attack;
        */

        StateMachine.SetState(follow);
    }

    public float GetPlayerDistance() 
    {
        return Vector3.Distance(Player.I.transform.position, Slot.transform.position);
    }
}
