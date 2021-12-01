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
        // Move some distance along a line opposite the direction to the player.
        Vector3 v = Statics.CalcPositionInDirection(d.transform.position, d.GetDirectionToPlayer(), d.ComfortablePlayerDistanceMin);
        d.MoveAgentToLocation(v);
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {
    }
}
