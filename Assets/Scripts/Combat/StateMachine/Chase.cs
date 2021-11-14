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
        d.MoveToDestination(d.PlayerPos, d.VelRun, PhysicsBody.MoveForce, 3f);
        d.RotateTowardsTarget(d.PlayerPos);
    }

    public void OnEnter()
    {
        Debug.Log("chase w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {

    }
}
