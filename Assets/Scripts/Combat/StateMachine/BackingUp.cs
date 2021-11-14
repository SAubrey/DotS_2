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
        d.BackUp();
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {
    }
}
