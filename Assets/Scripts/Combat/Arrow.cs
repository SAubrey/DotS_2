using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Rigidbody Rigidbody;
    [SerializeField] public LineRenderer LineRenderer;
    public string TargetTag = "Enemy";
    private float Damage = 10f;
    private bool Flying, Fired = false;
    Timer TimerNewLine = new Timer(0.02f);
    Timer TimerDespawn = new Timer(60f);

    void Awake()
    {
        LineRenderer.positionCount = 10;
    }

    void FixedUpdate()
    {
        if (Flying)
        {
            Rigidbody.rotation = Statics.CalcRotationWithVelocity(Rigidbody.velocity);
        }
        if (TimerNewLine.Increase(Time.fixedDeltaTime) && Fired)
        {
            UpdateTrail();
        }
        if (TimerDespawn.Increase(Time.fixedDeltaTime))
        {
            Despawn();
        }
    }

    public void SetTargetMask(string tag) 
    {   
        TargetTag = tag;
    }

    public void Fly(Vector3 startPoint, Vector3 targetPoint, float damage, float speed) 
    {   
        transform.SetParent(null);
        
        for (int i = 0; i < LineRenderer.positionCount; i++)
        {
            LineRenderer.SetPosition(i, startPoint);
        }
        /*
        // Determine angle to hit directly on target. 
        float angle0, angle1 = 0;
        bool inRange = Statics.CalcLaunchAngle(speed, startPoint, targetPoint, Physics.gravity.magnitude, out angle0, out angle1);
        if (!inRange)
            return;
        var rot = Quaternion.AngleAxis(angle0, Vector3.right);
        var dir = Statics.CalcLaunchVelocity(startPoint, targetPoint, Physics.gravity.magnitude, angle0).normalized;
        //Rigidbody.velocity = speed * dir;
        */
        // Fire directly at the target but miss due to gravity. Faster = closer to target.
        Vector3 direction = Statics.CalcDirection(startPoint, targetPoint);
        Rigidbody.velocity = speed * direction;

        Damage = damage;
        Flying = true;
        Fired = true;
        LineRenderer.enabled = true;
    }

    void OnTriggerEnter(Collider collider) 
    {
        if (!Flying || ColliderIsLayer(collider, "Player") || ColliderIsLayer(collider, "Slot")) 
            return;

        SoundManager.I.playerAudioPlayer.ArrowHit(gameObject);
        Flying = false;
        if (ColliderIsLayer(collider, "Enemy"))
        {
            Slot slot = collider.GetComponent<Slot>();
            if (slot == null)
                return;

            if (slot.HasUnit)
            {
                slot.Unit.TakeDamage((int)Damage);
            }
        }
        Stick(collider.transform);
    }

    private void Stick(Transform t)
    {
        Debug.Log("STICKING!");
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.isKinematic = true;
        transform.SetParent(t);
    }

    private void Despawn()
    {
        GameObject.Destroy(gameObject);
    }

    private void AddLineToTrail()
    {
        LineRenderer.positionCount = LineRenderer.positionCount + 1;
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, transform.position);
    }

    Vector3[] linePoints = new [] { new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), 
            new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() };
    private void UpdateTrail() 
    {
        // Move all positions up to the next position, then update the last position to the current position.
        LineRenderer.GetPositions(linePoints);
        for (int i = 0; i < LineRenderer.positionCount - 1; i++) 
        {
            LineRenderer.SetPosition(i, linePoints[i + 1]);
        }
        LineRenderer.SetPosition(LineRenderer.positionCount - 1, transform.position);
    }

    private bool ColliderIsLayer(Collider collider, string layer)
    {
        return collider.gameObject.layer == LayerMask.NameToLayer(layer);
    }
}
