using UnityEngine;

public class MoveToAttack : IState
{
    private AIBrain Brain;
    public bool BeginAttack = false;

    public MoveToAttack(AIBrain brain)
    {
        Brain = brain;
    }

    public void Tick()
    {
        // Move into attack range
        Brain.SetDestinationToTarget();

        if (Brain.TargetDistance <= Brain.AttackDistance) {
            BeginAttack = true;
        }
    }

    public void OnEnter()
    {
        #if DEBUG_AI 
        Debug.Log("Move to attack w/ TD: " + Brain.TargetDistance);
        #endif
    }

    public void OnExit()
    {
        BeginAttack = false;
    }
}
