using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PhysicsBody : MonoBehaviour
{
    public event Action<Vector2> on_position_change;
    private Vector2 _position;
    public Vector2 position { 
        get 
        {
            return transform.position;
        } 
        protected set 
        {
            if (value == _position)
                return;
            _position = value;
            if (on_position_change != null)
                on_position_change(position);
        } 
    }

    public static float MoveForce = 250f;
    public Rigidbody2D rb;

    public virtual bool Move(Vector2 dest, float maxVel, float force, float acceptableDistance)
    {
        // Check if close enough to avoid jittering. 
        // Not ideal for movement without a distant destination.
        if (maxVel == 0 || arrivedAtPos(dest, acceptableDistance))
            return false;
        
        Vector2 direction = Statics.Direction(transform.position, dest);
        Move(direction, maxVel, force);
        return true;
    }

    protected virtual void Move(Vector2 direction, float maxVel, float force) {
        if (maxVel == 0)
            return;

        direction.Normalize();
        force *= (1f - (rb.velocity.magnitude / maxVel));
        rb.AddForce(direction * force);
        position =  rb.position;
    }

    protected bool arrivedAtPos(Vector2 dest, float acceptable_distance)
    {
        return Vector2.Distance(transform.position, dest) < acceptable_distance;
    }

    protected void RotateToMouse()
    {
        Vector3 mouse_pos = Input.mousePosition;
        //mouse_pos.z =  Input.mousePosition.z - CamSwitcher.I.battle_cam.transform.position.z;
        mouse_pos.z = 2000f;
        mouse_pos = CamSwitcher.I.battle_cam.ScreenToWorldPoint(mouse_pos);

        float angle = Mathf.Atan2((mouse_pos.y - transform.position.y),
            (mouse_pos.x - transform.position.x)) * Mathf.Rad2Deg - 90f;

        Quaternion rotation = Quaternion.Euler(new Vector3(0f, 0f, angle));
        transform.rotation = rotation;
        //float str = Mathf.Min(10f * Time.fixedDeltaTime, 1f);
        //transform.rotation = Quaternion.Lerp(transform.rotation, rotation, str);
    }
}
