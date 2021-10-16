using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attack : IState
{
    private EnemyDeployment d;
    private Timer timer;
    public bool done_attacking = false;

    public Attack(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        // Check for hit some way through animation
        if (timer.Increase(Time.deltaTime))
        {
            d.melee_attack(d.get_attacking_zone(true));
            done_attacking = true;
        }
    }

    public void OnEnter()
    {
        timer = new Timer(0.5f, .4f, .6f);
        Debug.Log("attack w/ PD: " + d.player_distance);
    }

    public void OnExit()
    {
        timer.Reset();
        done_attacking = false;
    }
}
