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
        d.MoveToDestination(d.PlayerPos, d.VelSprint, PhysicsBody.MoveForce, 3f);

        if (d.PlayerDistance <= d.AttackDistance) {
            begin_attack = true;
        }
        //d.rotate_towards_target(d.player_pos);
    }

    public void OnEnter()
    {
        Debug.Log("Move to attack w/ PD: " + d.PlayerDistance);
    }

    public void OnExit()
    {

    }
}
