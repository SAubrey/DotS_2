using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : IState
{
    private EnemyDeployment D;
    private Vector3 TargetPos;
    Timer T = new Timer(5f, false, 3f, 6f);

    public Roam(EnemyDeployment d)
    {
        this.D = d;
    }

    public void Tick()
    {
        if (T.Increase(Time.deltaTime))
        {
            TargetPos = GetRandomTargetPos();
            D.SetAgentDestination(TargetPos);
        }
    }

    public void OnEnter()
    {
        // Set random target, update when target entered
        TargetPos = GetRandomTargetPos();
        Debug.Log("Roam: " + TargetPos + " Time: " + Time.time);
    }

    public void OnExit()
    {
        TargetPos = Vector2.zero;
        T.Reset();
    }

    private Vector3 GetRandomTargetPos()
    {
        Vector3 v = D.gameObject.transform.position;
        v.x += UnityEngine.Random.Range(-50f, 50f);
        v.z += UnityEngine.Random.Range(-50f, 50f);
        return v;
    }
}
