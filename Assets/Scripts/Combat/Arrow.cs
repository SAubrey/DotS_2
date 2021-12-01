using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Arrow : MonoBehaviour
{
    [SerializeField] private Rigidbody Rigidbody;
    [SerializeField] public LineRenderer LineRenderer;
    public string TargetTag = "Enemy";
    private float Damage = 10f;
    private bool Flying = false;
    Timer TimerNewLine = new Timer(0.08f);
    Timer TimerDespawn = new Timer(15f);

/*
    public void init(LayerMask mask, Vector3 launch_pos, Vector3 target_pos)
    {
        target_mask = mask;

        direction = (target_pos - launch_pos).normalized;
        float distance = Vector3.Distance(launch_pos, target_pos);
        duration = distance / velocity;
        //Debug.Log("distance: " + distance + "DURATION " + duration);
        vertical_velocity = duration * (gravity / 2f) - (1f / duration);
        //Debug.Log("vertical v" + vertical_velocity);
        shadow_pos = launch_pos;
        in_flight = true;
        arrow.transform.up = direction;
        GetComponent<SpriteRenderer>().transform.LookAt(CamSwitcher.I.BattleCam.transform);

        t.Increase(0.09f);
    }*/
    void FixedUpdate()
    {
        Rigidbody.rotation = Statics.CalcRotationWithVelocity(Rigidbody.velocity);
        if (TimerNewLine.Increase(Time.fixedDeltaTime))
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

    public void Fly(Vector3 startPoint, Vector3 targetPoint, float damage) 
    {   
        transform.SetParent(null);
        LineRenderer.positionCount = 10;

        Vector3 direction = Statics.CalcDirection(startPoint, targetPoint);
        //Rigidbody.velocity = Statics.CalcLaunchVelocity(startPoint, targetPoint, Physics.gravity.magnitude, 15f);
        float angle0, angle1 = 0;
        bool inRange = Statics.CalcLaunchAngle(50f, startPoint, targetPoint, Physics.gravity.magnitude, out angle0, out angle1);
        if (!inRange)
            return;
        var rot = Quaternion.AngleAxis(angle0, Vector3.right);
        //var dir = rot * direction;
        var dir = Statics.CalcLaunchVelocity(startPoint, targetPoint, Physics.gravity.magnitude, angle0).normalized;
        Rigidbody.velocity = 50f * dir;
        Damage = damage;
        Flying = true;
    }

    void OnTriggerEnter(Collider collider) 
    {
        if (!Flying || ColliderIsLayer(collider, "Player") || ColliderIsLayer(collider, "Slot")) 
            return;

        Flying = false;
        if (ColliderIsLayer(collider, "Enemy"))
        {
            Debug.Log("Hit: " + collider.gameObject.layer);
            Slot slot = collider.GetComponent<Slot>();
            if (slot == null)
                return;

            if (slot.HasUnit)
            {
                slot.GetUnit().TakeDamage((int)Damage);
            }
        }
        Stick(collider.transform);
    }

    private void Stick(Transform transform)
    {
        Debug.Log("stuck: " + transform.gameObject.name);
        Rigidbody.velocity = Vector3.zero;
        //Rigidbody.angularVelocity = Vector3.zero;
        Rigidbody.isKinematic = true;
        ///LineRenderer.positionCount = 0;
        transform.SetParent(transform);
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

    private void UpdateTrail() 
    {
        // Move all positions up to the next position, then update the last position to the current position.
        Vector3[] linePoints = new [] { new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3(), 
            new Vector3(), new Vector3(), new Vector3(), new Vector3(), new Vector3() };
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
