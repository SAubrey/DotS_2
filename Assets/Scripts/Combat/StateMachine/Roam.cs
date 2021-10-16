using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : IState
{
    private EnemyDeployment d;
    private Vector2 target_pos;
    Timer t = new Timer(5f, 3f, 6f);

    public Roam(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        d.Move(target_pos, d.VEL_WALK, PhysicsBody.MoveForce, 3f);

        if (t.Increase(Time.deltaTime))
        {
            target_pos = get_random_target_pos();
        }
    }

    public void OnEnter()
    {
        // Set random target, update when target entered
        target_pos = get_random_target_pos();
        Debug.Log("Roam: " + target_pos);
    }

    public void OnExit()
    {
        target_pos = Vector2.zero;
        t.Reset();
    }

    private Vector2 get_random_target_pos()
    {
        Vector2 v = d.gameObject.transform.position;
        v.x += UnityEngine.Random.Range(-50f, 50f);
        v.y += UnityEngine.Random.Range(-50f, 50f);
        return v;
    }
}
