using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveToAttack : IState
{
    private EnemyDeployment d;
    public bool begin_attack = false;

    public MoveToAttack(EnemyDeployment d)
    {
        this.d = d;
    }

    public void Tick()
    {
        // Move into attack range
        //d.gameObject.transform.position = 
        // Vector2.MoveTowards(d.gameObject.transform.position, d.player_pos, d.VEL_RUN);

        d.move(StaticOperations.target_unit_vec(d.gameObject.transform.position, d.player_pos),
            d.VEL_SPRINT, d.player_pos.x, d.player_pos.y);

        if (d.player_distance <= d.attack_distance) {
            begin_attack = true;
        }
        //d.rotate_towards_target(d.player_pos);
    }

    public void OnEnter()
    {
        Debug.Log("Move to attack w/ PD: " + d.player_distance);
    }

    public void OnExit()
    {

    }
}
