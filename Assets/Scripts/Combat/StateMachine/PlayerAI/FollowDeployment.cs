using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowDeployment : IState
{
    private AIBrain Brain;
    public bool Returning = false;

    public FollowDeployment(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        // Return to slot point position.
        if (Vector3.Distance(Brain.Slot.SlotPointTransform.position, Brain.Slot.transform.position) > 1f)
        {
            Brain.Slot.SetAgentDestination(Brain.Slot.SlotPointTransform.position);
            
        } else
        {
            if (Returning)
                Returning = false;
        }
    }

    public void OnEnter()
    {
        Debug.Log("Follow");
        Returning = true;
    }

    public void OnExit()
    {
    }
}

