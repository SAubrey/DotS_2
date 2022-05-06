using UnityEngine;
using System;
using UnityEngine.AI;

public class AgentBody : MonoBehaviour
{
    protected NavMeshAgent Agent { get; private set; }
    public float MaxAcceleration { get; private set; } = 20f;
    public float MaxSpeed 
    { 
        get { return Agent.speed; } 
        protected set { Agent.speed = value; }
    }

    public event Action<Vector3> OnPositionChange;
    private Vector3 _position;
    public Vector3 Position { 
        get 
        {
            return transform.position;
        } 
        protected set 
        {
            if (value == _position)
                return;
            _position = value;
            if (OnPositionChange != null)
                OnPositionChange(Position);
        } 
    }

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        MaxAcceleration = Agent.acceleration;
        MaxSpeed = Agent.speed;
    }

    protected virtual void Update()
    {
        Position = transform.position;
        if (Agent.hasPath) 
            Agent.acceleration = (Agent.remainingDistance < 4f) ? 4 * MaxAcceleration : MaxAcceleration;
    }

    public virtual void SetAgentDestination(Vector3 pos)
    {
        Agent.SetDestination(pos);
        if (Agent.updateRotation == false)
            Statics.RotateToPoint(transform, pos);
    }

    public virtual void SetAgentDestinationAndRotation(Vector3 movePos, Vector3 lookPos)
    {
        Agent.SetDestination(movePos);
        Statics.RotateToPoint(transform, lookPos);
    }
}
