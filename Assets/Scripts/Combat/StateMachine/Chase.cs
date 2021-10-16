using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : IState
{
    private EnemyDeployment d;

    public Chase(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        // Move and rotate towards target
        d.Move(d.player_pos, d.VEL_RUN, PhysicsBody.MoveForce, 3f);
        d.rotate_towards_target(d.player_pos);
    }

    public void OnEnter()
    {
        Debug.Log("chase w/ PD: " + d.player_distance);
    }

    public void OnExit()
    {

    }
}
