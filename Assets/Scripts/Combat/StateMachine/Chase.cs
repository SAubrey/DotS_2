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
        d.SetAgentDestination(d.PlayerPos); 
    }

    public void OnEnter()
    {
        Debug.Log("chase w/ PD: " + d.PlayerDistance);
        
    }

    public void OnExit()
    {

    }
}
