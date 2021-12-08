using UnityEngine;

public class MoveToAttack : IState
{
    private EnemyDeployment D;
    public bool BeginAttack = false;

    public MoveToAttack(EnemyDeployment d)
    {
        this.D = d;
    }

    public void Tick()
    {
        // Move into attack range
        D.SetAgentDestination(D.PlayerPos);

        if (D.PlayerDistance <= D.AttackDistance) {
            BeginAttack = true;
        }
    }

    public void OnEnter()
    {
        Debug.Log("Move to attack w/ PD: " + D.PlayerDistance);
    }

    public void OnExit()
    {
        BeginAttack = false;
    }
}
