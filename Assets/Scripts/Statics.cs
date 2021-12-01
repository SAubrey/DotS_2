using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Fields and methods not particular to a class and do not need to be instantiated more than once.
*/
public class Statics
{
    // UI CONSTANTS
    public static readonly Color DISABLED_C = new Color(.78125f, .78125f, .78125f, 1);
    public static readonly Color ASTRA_COLOR = new Color(.6f, .6f, 1, 1);
    public static readonly Color ENDURA_COLOR = new Color(1, 1, .6f, 1);
    public static readonly Color MARTIAL_COLOR = new Color(1, .6f, .6f, 1);
    public static readonly Color[] disc_colors = { ASTRA_COLOR, ENDURA_COLOR, MARTIAL_COLOR, Color.white };
    public static Color BLUE = new Color(.1f, .1f, 1, 1);
    public static Color RED = new Color(1, .1f, .1f, 1);
    public static Color ORANGE = new Color(1, .6f, 0, 1);

    public static readonly Vector3 CITY_POS = new Vector3(10.5f, 10.5f, 0);

    public static float CalcAngleBetweenTwoPoints(Vector3 a, Vector3 b) {
         return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
    }

    public static Vector3 CalcLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float gravity, float initialAngle) 
    {
        float angle = initialAngle * Mathf.Deg2Rad;
 
        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(targetPoint.x, 0, targetPoint.z);
        Vector3 planarPostion = new Vector3(startPoint.x, 0, startPoint.z);
 
        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        // Distance along the y axis between objects
        float yOffset = startPoint.y - targetPoint.y;
 
        float initialVelocity = (1f / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(distance, 2f)) / (distance * Mathf.Tan(angle) + yOffset));
 
        Vector3 velocity = new Vector3(0f, initialVelocity * Mathf.Sin(angle), initialVelocity * Mathf.Cos(angle));
 
        // Rotate our velocity to match the direction between the two objects
        float angleBetweenObjects = Vector3.Angle(Vector3.forward, planarTarget - planarPostion) * (targetPoint.x > startPoint.x ? 1 : -1);
        Vector3 finalVelocity = Quaternion.AngleAxis(angleBetweenObjects, Vector3.up) * velocity;
 
        return finalVelocity;
    }

    public static bool CalcLaunchAngle(float speed, float distance, float yOffset, float gravity, out float angle0, out float angle1)
    {
        angle0 = angle1 = 0;
 
        float speedSquared = speed * speed;
 
        float operandA = Mathf.Pow(speed, 4);
        float operandB = gravity * (gravity * (distance * distance) + (2 * yOffset * speedSquared));
 
        // Target is not in range
        if (operandB > operandA)
            return false;
 
        float root = Mathf.Sqrt(operandA - operandB);
 
        angle0 = Mathf.Atan((speedSquared + root) / (gravity * distance));
        angle1 = Mathf.Atan((speedSquared - root) / (gravity * distance));
 
        return true;
    }

    public static Quaternion CalcRotationWithVelocity(Vector3 velocity) 
    {
        return Quaternion.LookRotation(velocity.normalized);
    }

    
    public static bool CalcLaunchAngle(float speed, Vector3 startPoint, Vector3 targetPoint, float gravity, out float angle0, out float angle1)
    {
        angle0 = angle1 = 0;

        // Positions of this object and the target on the same plane
        Vector3 planarTarget = new Vector3(targetPoint.x, 0, targetPoint.z);
        Vector3 planarPostion = new Vector3(startPoint.x, 0, startPoint.z);
 
        // Planar distance between objects
        float distance = Vector3.Distance(planarTarget, planarPostion);
        float yOffset = startPoint.y - targetPoint.y;
 
        float speedSquared = speed * speed;
 
        float operandA = Mathf.Pow(speed, 4);
        float operandB = gravity * (gravity * (distance * distance) + (2 * yOffset * speedSquared));
 
        // Target is not in range
        if (operandB > operandA)
            return false;
 
        float root = Mathf.Sqrt(operandA - operandB);
 
        angle0 = Mathf.Atan((speedSquared + root) / (gravity * distance));
        angle1 = Mathf.Atan((speedSquared - root) / (gravity * distance));
 
        return true;
    }

    public static float CalcAdjustedIncrease(float current, float amount, float max)
    {
        // Increase as much as possible without exceeding.
        return current + amount <= max ? amount :
            amount - (max - (current + amount)); // subtract by overflow
    }

    public static Vector3 GetMouseWorldPos(Camera cam, LayerMask mask, float maxDistance=2048f) {
        Ray ray = cam.ScreenPointToRay(Controller.I.MousePosition.ReadValue<Vector2>());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, mask)) {
            return hit.point;
        //if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit, maxDistance, mask)) {
        }
        return Vector3.zero;
    }

    public static Collider2D FindClosestCollider(Collider2D[] colliders, Vector3 sourcePoint)
    {
        if (colliders.Length == 0)
            return null;
        Collider2D closest = colliders[0];
        float closestDistance = Vector3.Distance(sourcePoint, closest.transform.position);
        foreach (Collider2D col in colliders)
        {
            float distance = Vector3.Distance(sourcePoint, col.transform.position);
            if (Vector2.Distance(sourcePoint, col.transform.position) < closestDistance)
            {
                closest = col;
                closestDistance = distance;
            }
        }
        return closest;
    }

    public static Vector3 CalcDirection(Vector3 v, Vector3 target)
    {
        return (target - v).normalized;
    }

    public static Quaternion CalcRotationToPoint(Transform t, Vector3 point)
    {   
        return Quaternion.LookRotation(Statics.CalcDirection(t.position, point));
    }

    public static Vector3 CalcSmoothedNextPosition(Vector3 pos, Vector3 end_pos, float smooth_speed)
    {
        return Vector2.Lerp(pos, end_pos, smooth_speed);
    }
    
    public static int CalcMapDistance(Pos pos1, Pos pos2)
    {
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);
        return dx + dy;
    }

    public static int CalcValidNonnegativeChange(int startingAmount, int change)
    {
        if (startingAmount + change < 0)
        {
            return startingAmount;
        }
        return change;
    }

    public static Vector3 CalcPositionInDirection(Vector3 startPoint, Vector3 direction, float length)
    {
        return startPoint + (direction.normalized * length);
    }
}