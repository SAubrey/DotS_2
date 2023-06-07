using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chase : IState
{
    private AIBrain Brain;

    public Chase(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        Brain.SetDestinationToTarget();
    }

    public void OnEnter()
    {
        Debug.Log("chase w/ PD: " + Brain.TargetDistance);
        Brain.Chase = true;
        
    }

    public void OnExit()
    {
        Brain.Chase = false;
    }
}
