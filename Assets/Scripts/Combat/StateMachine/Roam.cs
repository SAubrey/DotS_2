using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roam : IState
{
    private EnemyDeployment d;
    private Vector2 target_pos;
    Timer t = new Timer(2f);

    public Roam(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        d.move(StaticOperations.target_unit_vec(d.gameObject.transform.position, target_pos),
            d.VEL_WALK, target_pos.x, target_pos.y);

        if (t.increase(Time.deltaTime))
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
        // 
    }

    private Vector2 get_random_target_pos()
    {
        Vector2 v = d.gameObject.transform.position;
        v.x += UnityEngine.Random.Range(-100f, 100f);
        v.y += UnityEngine.Random.Range(-100f, 100f);
        return v;
    }
}
