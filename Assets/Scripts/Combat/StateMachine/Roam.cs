using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : IState
{
    private EnemyDeployment D;
    private Vector2 TargetPos;
    Timer T = new Timer(5f, false, 3f, 6f);

    public Roam(EnemyDeployment d)
    {
        this.D = d;
    }

    public void Tick()
    {
        //d.MoveToDestination(target_pos, d.VelWalk, PhysicsBody.MoveForce, 3f);
        D.MoveAgentToLocation(TargetPos);
        if (T.Increase(Time.deltaTime))
        {
            TargetPos = GetRandomTargetPos();
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

    private Vector2 GetRandomTargetPos()
    {
        Vector2 v = D.gameObject.transform.position;
        v.x += UnityEngine.Random.Range(-50f, 50f);
        v.y += UnityEngine.Random.Range(-50f, 50f);
        return v;
    }
}
