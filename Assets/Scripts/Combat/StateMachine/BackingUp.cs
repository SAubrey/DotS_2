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
        d.MoveAgentToLocation(Statics.CalcPositionInDirection(d.transform.position, d.GetDirectionToPlayer(), d.ComfortablePlayerDistanceMin));
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {
    }
}
