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
        d.Move(d.player_pos, d.VEL_SPRINT, PhysicsBody.MoveForce, 3f);

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
