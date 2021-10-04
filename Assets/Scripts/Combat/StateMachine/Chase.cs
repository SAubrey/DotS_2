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
        // Move and rotate towards target
        /*d.gameObject.transform.position = 
            Vector2.MoveTowards(d.gameObject.transform.position, 
                d.player_pos, d.VEL_RUN); */
        d.move(StaticOperations.target_unit_vec(d.gameObject.transform.position, d.player_pos),
            d.VEL_RUN, d.player_pos.x, d.player_pos.y);

        d.rotate_towards_target(d.player_pos);
    }

    public void OnEnter()
    {
        Debug.Log("chase w/ PD: " + d.player_distance);
    }

    public void OnExit()
    {

    }
}
