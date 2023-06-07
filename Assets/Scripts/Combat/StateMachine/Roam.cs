using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : IState
{
    private AIBrain Brain;
    private Vector3 TargetPos;
    Timer T = new Timer(5f, false, 3f, 6f);

    public Roam(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        if (T.Increase(Time.deltaTime))
        {
            TargetPos = GetRandomTargetPos();
            Brain.Slot.SetAgentDestination(TargetPos);
        }
    }

    public void OnEnter()
    {
        // Set random target, update when target entered
        TargetPos = GetRandomTargetPos();
        Debug.Log("Roam: " + TargetPos + " Time: " + Time.time);
        Brain.Roam = true;
    }

    public void OnExit()
    {
        TargetPos = Vector3.zero;
        T.Reset();
        Brain.Roam = false;
    }

    private Vector3 GetRandomTargetPos()
    {
        Vector3 v = Brain.transform.position;
        v.x += UnityEngine.Random.Range(-40f, 40f);
        v.z += UnityEngine.Random.Range(-40f, 40f);
        return v;
    }
}
