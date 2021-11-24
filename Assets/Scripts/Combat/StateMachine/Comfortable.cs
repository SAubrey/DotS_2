using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idle, relatively safe distance from player where the enemy decides what and when to do.
public class Comfortable : IState
{
    private EnemyDeployment D;
    private Timer PendingAttackTimer;
    public bool MoveToAttack = false;

    public Comfortable(EnemyDeployment d)
    {
        D = d;
    }

    public void Tick()
    {
        if (PendingAttackTimer.Increase(Time.deltaTime))
        {
            MoveToAttack = true;
        }
    }

    public void OnEnter()
    {
        PendingAttackTimer = new Timer(Random.Range(0.5f, 1.5f));
        Debug.Log("comfortable w/ PD: " + D.PlayerDistance);
    }

    public void OnExit()
    {
        MoveToAttack = false;
    }
}
