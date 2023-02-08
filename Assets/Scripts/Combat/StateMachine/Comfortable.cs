using UnityEngine;

// Idle, relatively safe distance from player where the enemy decides what and when to do.
public class Comfortable : IState
{
    private AIBrain Brain;
    private Timer PendingAttackTimer;
    public bool MoveToAttack = false;

    public Comfortable(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        if (PendingAttackTimer.Increase(Time.deltaTime))
        {
            MoveToAttack = true;
        }
    }

    public void OnEnter()
    {
        PendingAttackTimer = new Timer(Random.Range(0.6f, 2f));
        Debug.Log("comfortable w/ TD: " + Brain.TargetDistance);
    }

    public void OnExit()
    {
        MoveToAttack = false;
    }
}
