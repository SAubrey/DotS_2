using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackingUp : IState
{
    private AIBrain Brain;
    float r;

    public BackingUp(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        if (Brain.Target == null)
            return;
        // Make it active to avoid choosing out of date comfy distance
        Vector3 v = Statics.CalcPositionInDirection(Brain.Slot.transform.position, Brain.GetDirectionToTarget(), r);
        Brain.Slot.SetAgentDestination(v);
    }

    public void OnEnter()
    {
        Debug.Log("Backing up w/ TD: " + Brain.TargetDistance);
        // Move some distance along a line opposite the direction to the player.
        r = Random.Range(Brain.ComfortableTargetDistanceMin + 1f, Brain.ComfortableTargetDistanceMax - 1f);
    }

    public void OnExit()
    {
    }
}
