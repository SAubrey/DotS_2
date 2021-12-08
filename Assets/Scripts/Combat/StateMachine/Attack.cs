using UnityEngine;

public class Attack : IState
{
    private EnemyDeployment D;
    private Timer Timer, AttackRecoveryTimer;
    public bool Attacking = true;
    private bool RecoveringFromAttack = false;

    public Attack(EnemyDeployment d)
    {
        D = d;
    }

    public void Tick()
    {
        // Check for hit some way through animation
        if (Timer.Increase(Time.deltaTime))
        {
            D.MeleeAttack(D.GetAttackingZone(true));
            RecoveringFromAttack = true;
        }

        if (RecoveringFromAttack)
        {
            if (AttackRecoveryTimer.Increase(Time.deltaTime)) 
            {
                Attacking = false;
            }
        }

    }

    public void OnEnter()
    {
        Timer = new Timer(0.5f, true, .4f, .6f);
        AttackRecoveryTimer = new Timer(1f, true);
        Debug.Log("attack w/ PD: " + D.PlayerDistance);
        Attacking = true;
    }

    public void OnExit()
    {
        RecoveringFromAttack = false;
        Attacking = true;
    }
}
