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
        StateMachine.AddAnyTransition(follow, DeadTarget());

        Func<bool> CanApproach() => () => TargetDistance <= ChaseDistance && Target != null && !follow.Returning;
        Func<bool> TookHitTrue() => () => TookHit == true;
        Func<bool> DoneAttacking() => () => !attack.Attacking;

        Func<bool> TooFarFromPlayer() => () => GetPlayerDistance() > TooFarFromPlayerDistance;// && !attack.Attacking;
        Func<bool> DeadTarget() => () => Target == null;

        StateMachine.SetState(follow);
    }

    protected override Slot FindNearestEnemyInSight()
    {
        var enemies = TurnPhaser.I.ActiveDisc.Cell.GetEnemies();
        Slot nearest = null;
        float nearestDistance = Mathf.Infinity;
        float distance = 0;

        foreach (Enemy e in enemies)
        {
            distance = Vector3.Distance(transform.position, e.Slot.transform.position);
            if (distance < nearestDistance)
            {
                nearest = e.Slot;
                nearestDistance = distance;
            }
        }
        return nearest;
    }

    public float GetPlayerDistance() 
    {
        return Vector3.Distance(Player.I.transform.position, Slot.transform.position);
    }
}
