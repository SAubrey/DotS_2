using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class StaticOperations {
    public static float pixelsPerUnit = 100;
    public static float GetAdjustedIncrease(float current, float amount, float max) {
        // Increase as much as possible without exceeding.
        return current + amount <= max ? amount : 
            amount - (max - (current + amount)); // subtract by overflow
    }
    
    public static Collider2D DetermineClosestCollider(Collider2D[] colliders, Vector2 sourcePoint) {
        if (colliders.Length == 0)
            return null;
        Collider2D closest = colliders[0];
        float closestDistance = Vector2.Distance(sourcePoint, closest.transform.position);
        foreach (Collider2D col in colliders) {
            float distance = Vector2.Distance(sourcePoint, col.transform.position);
            if (Vector2.Distance(sourcePoint, col.transform.position) < closestDistance) {
                closest = col;
                closestDistance = distance;
            }
        }
        return closest;
    }

    public static Vector2 TargetUnitVec(Vector2 v, Vector2 target) {
        return (target - v).normalized;
    }

    public static Vector3 GetSmoothedNextPosition(Vector3 pos, Vector3 end_pos, float smooth_speed) {
        return Vector2.Lerp(pos, end_pos, smooth_speed);
    }
}
