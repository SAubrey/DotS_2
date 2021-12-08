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
        
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ PD: " + d.PlayerDistance);
        // Move some distance along a line opposite the direction to the player.
        var r = Random.Range(d.ComfortablePlayerDistanceMin + 1f, d.ComfortablePlayerDistanceMax - 1f);
        Vector3 v = Statics.CalcPositionInDirection(d.transform.position, d.GetDirectionToPlayer(), r);
        d.SetAgentDestination(v);
    }

    public void OnExit()
    {
    }
}
