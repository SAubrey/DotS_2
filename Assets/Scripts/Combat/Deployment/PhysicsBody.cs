using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PhysicsBody : MonoBehaviour
{
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

    public static float MoveForce = 1000f;
    public Rigidbody Rigidbody;

    protected virtual void Awake() 
    {
        Rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public virtual void MoveToDestination(Vector3 dest, float maxVel, float force, float stopDistance=2f)
    {
        // Check if close enough to avoid jittering. 
        // Not ideal for movement without a distant destination.
        //Debug.Log(ArrivedAtPos(dest, acceptableDistance));
        if (maxVel == 0 || WithinDistance(dest, stopDistance))
            return;
        
        if (WithinDistance(dest, 5f))
        {
            Brake();
            return;
        }

        Vector3 direction = Statics.CalcDirection(Rigidbody.position, dest);
        MoveInDirection(direction, maxVel, force);
        RotateWithDirection(Rigidbody.velocity);
        return;
    }

    public void RotateWithDirection(Vector3 velocity) 
    {
        if (velocity == Vector3.zero)
            return;
        transform.rotation = Quaternion.LookRotation(velocity.normalized);
    }

    protected virtual void MoveInDirection(Vector3 direction, float maxVel, float force) {
        if (maxVel == 0)
            return;

        direction.Normalize();
        //force *= (1f - (Rigidbody.velocity.magnitude / maxVel));
        Rigidbody.AddForce(direction * force);
        Position =  Rigidbody.position;
    }

    protected bool WithinDistance(Vector3 dest, float distance)
    {
        return Vector3.Distance(transform.position, dest) < distance;
    }

    protected void Brake()
    {
        if (Rigidbody.velocity.magnitude < .3f)
            return;
        Vector3 brakeForce = -Rigidbody.velocity;
        Rigidbody.AddForce(brakeForce * 8f);
    }

    protected void RotateToMouse(LayerMask raycastMask)
    {
        Vector3 mousePos;
        mousePos = Statics.GetMouseWorldPos(CamSwitcher.I.battle_cam, raycastMask);
        if (mousePos == Vector3.zero)
            return;
        //float angle = Mathf.Atan2((mouse_pos.z - transform.position.z),
        //    (mouse_pos.x - transform.position.x)) * Mathf.Rad2Deg;
        //Vector3 direction = (transform.position - mousePos).normalized;
        //Quaternion rotation = Quaternion.LookRotation(direction);
        transform.LookAt(mousePos);
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        
        //float str = Mathf.Min(10f * Time.fixedDeltaTime, 1f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, rotation, str);
    }

    public static void RotateRigidbodyToTarget(Rigidbody rb, Vector3 target) 
    {
        var angle = Mathf.Atan2(rb.position.x, target.z) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, angle, 0f);
        rb.MoveRotation(rotation);// * deltaRotation);
    }

    public static void RotateTransformToTarget(Transform t, Vector3 target)
    {
        t.LookAt(target);
        t.rotation = Quaternion.Euler(0f, t.rotation.eulerAngles.y, 0f);
    }
}
