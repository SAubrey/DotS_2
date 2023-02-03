using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Retaliate : IState
{
    private AIBrain Brain;
    private Timer Timer, AttackRecoveryTimer;
    public bool Attacking = true;
    private bool RecoveringFromAttack = false;

    public Retaliate(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        // Check for hit some way through animation
        if (Timer.Increase(Time.deltaTime))
        {
            Brain.Slot.Unit.MeleeAttack();
            RecoveringFromAttack = true;
        }

        if (RecoveringFromAttack)
        {
            if (AttackRecoveryTimer.Increase(Time.deltaTime)) 
            {
                Attacking = false;
            }
        }

    }

    public void OnEnter()
    {
        Timer = new Timer(0.5f, true, .4f, .6f);
        AttackRecoveryTimer = new Timer(1f, true);
        Attacking = true;
    }

    public void OnExit()
    {
        RecoveringFromAttack = false;
        Attacking = true;
    }
}
