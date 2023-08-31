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

    // Triggers on continuous rigidbodies only trigger with static objects.
    void OnTriggerEnter(Collider collider) 
    {
        if (!Flying || Statics.ColliderIsLayer(collider, "Player") || Statics.ColliderIsLayer(collider, "Slot")) 
            return;

        Stick(collider.transform, false);
        Flying = false;
        SoundManager.I.playerAudioPlayer.ArrowHit(gameObject);
        LineRenderer.enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        Collider collider = collision.collider;
        if (!Flying || Statics.ColliderIsLayer(collider, "Player") || Statics.ColliderIsLayer(collider, "Slot") || Statics.ColliderIsLayer(collider, "Default")) 
            return;

        Stick(collider.transform, Statics.ColliderIsLayer(collider, "Enemy"));
        Flying = false;
        SoundManager.I.playerAudioPlayer.ArrowHit(gameObject);
        LineRenderer.enabled = false;
        if (Statics.ColliderIsLayer(collider, "Enemy"))
        {
            //Slot slot = collider.GetComponent<Slot>();
            Slot slot = collider.GetComponentInParent<Slot>();
            if (slot == null)
                return;

            if (slot.HasUnit)
            {
                slot.Unit.TakeDamage((int)Damage);
            }
        }
    }

    private void Stick(Transform t, bool isEnemy)
    {
        Debug.Log("STICKING! to: " + t.name);
        Rigidbody.velocity = Vector3.zero;
        Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.isKinematic = true;
        transform.SetParent(t);
        if (isEnemy) // Set arrow through center of mass, preserving height. Workaround for speculative collision.
        {
            transform.position = new Vector3(0, t.position.y, 0);
        }

        transform.position = t.position;
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
}
