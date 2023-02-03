using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AIBrainEnemy : AIBrain
{
    protected override void Awake()
    {
        base.Awake();

    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FillStateMachine()
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
        Func<bool> BackUp() => () => GetBehaviorZone() == BehaviorZone.Act && !moveToAttack.BeginAttack && !comfortable.MoveToAttack && !Attacking;
        Func<bool> MoveToAttack() => () => comfortable.MoveToAttack;
        Func<bool> TimeToAttack() => () => moveToAttack.BeginAttack;
        //Func<bool> DoneAttacking() => () => !attack.Attacking;
        //Func<bool> time_to_attack() => () => move_to_attack.begin_attack;

        StateMachine.SetState(roam);
    }

    public override void Delete()
    {
        GameObject.Destroy(gameObject);
    }
}
