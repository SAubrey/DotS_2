using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : IState
{
    private EnemyDeployment D;
    private Timer Timer;
    public bool DoneAttacking = false;

    public Attack(EnemyDeployment d)
    {
        D = d;
    }

    public void Tick()
    {
        // Check for hit some way through animation
        if (Timer.Increase(Time.deltaTime))
        {
            D.MeleeAttack(D.GetAttackingZone(true));
            DoneAttacking = true;
        }
    }

    public void OnEnter()
    {
        Timer = new Timer(0.5f, true, .4f, .6f);
        Debug.Log("attack w/ PD: " + D.PlayerDistance);
    }

    public void OnExit()
    {
        Timer.Reset();
        DoneAttacking = false;
    }
}
