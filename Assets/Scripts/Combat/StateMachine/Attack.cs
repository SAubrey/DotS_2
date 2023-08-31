using UnityEngine;

public class Attack : IState
{
    private AIBrain Brain;
    private Timer Timer, AttackRecoveryTimer;
    private bool RecoveringFromAttack = false;

    public Attack(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        if (Timer.Increase(Time.deltaTime))
        {
            Brain.Slot.Unit.MeleeAttack();
            RecoveringFromAttack = true;
        }

        // Allow some delay before being able to take other action.
        if (RecoveringFromAttack)
        {
            if (AttackRecoveryTimer.Increase(Time.deltaTime)) 
            {
                Brain.Attacking = false;
            }
        }

    }

    public void OnEnter()
    {
        Timer = new Timer(0.5f, true, .4f, .6f);
        AttackRecoveryTimer = new Timer(1.5f, true);
        #if DEBUG_AI 
        Debug.Log("attack w/ target distance: " + Brain.TargetDistance);
        #endif
        Brain.Attacking = true;
    }

    public void OnExit()
    {
        RecoveringFromAttack = false;
        Brain.Attacking = true;
    }
}
