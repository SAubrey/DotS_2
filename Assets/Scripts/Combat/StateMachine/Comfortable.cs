using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Idle, relatively safe distance from player where the enemy decides what and when to do.
public class Comfortable : IState
{
    private EnemyDeployment d;
    private Timer pending_attack_timer;
    public bool move_to_attack = false;

    public Comfortable(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        if (pending_attack_timer.Increase(Time.deltaTime))
        {
            move_to_attack = true;
        }
    }

    public void OnEnter()
    {
        pending_attack_timer = new Timer(Random.Range(0.5f, 1.5f));
        Debug.Log("comfortable w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {
        move_to_attack = false;
    }
}
