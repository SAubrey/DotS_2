using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackingUp : IState
{
    private EnemyDeployment d;

    public BackingUp(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        //d.move(-d.get_direction_to_player(), d.VEL_SPRINT);
        d.back_up();
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ PD: " + d.player_distance);
    }

    public void OnExit()
    {
    }
}
