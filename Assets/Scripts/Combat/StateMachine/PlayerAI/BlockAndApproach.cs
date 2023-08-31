using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockAndApproach : IState
{
    private AIBrain Brain;

    public BlockAndApproach(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        if (Brain.TargetDistance > 1f)
        {
            Brain.SetDestinationToTarget();
        }
    }

    public void OnEnter()
    {
        Brain.Slot.Unit.Blocking = true;
        #if DEBUG_AI 
        Debug.Log("BlockAndApproach");
        #endif
    }

    public void OnExit()
    {
        Brain.Slot.Unit.Blocking = false;
    }
}
