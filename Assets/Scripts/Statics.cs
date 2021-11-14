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

    public static float AngleBetweenTwoPoints(Vector3 a, Vector3 b) {
         return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
     }

    public static float GetAdjustedIncrease(float current, float amount, float max)
    {
        // Increase as much as possible without exceeding.
        return current + amount <= max ? amount :
            amount - (max - (current + amount)); // subtract by overflow
    }

    public static Vector3 GetMouseWorldPos(Camera cam, LayerMask mask, float maxDistance=2048f) {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, maxDistance, mask)) {
            return raycastHit.point;
        }
        return Vector3.zero;
    }

    public static Collider2D DetermineClosestCollider(Collider2D[] colliders, Vector3 sourcePoint)
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

    public static Vector3 Direction(Vector3 v, Vector3 target)
    {
        return (target - v).normalized;
    }

    public static Vector3 GetSmoothedNextPosition(Vector3 pos, Vector3 end_pos, float smooth_speed)
    {
        return Vector2.Lerp(pos, end_pos, smooth_speed);
    }
    
    public static int calc_map_distance(Pos pos1, Pos pos2)
    {
        int dx = Mathf.Abs(pos1.x - pos2.x);
        int dy = Mathf.Abs(pos1.y - pos2.y);
        return dx + dy;
    }

    public static int valid_nonnegative_change(int starting_amount, int change)
    {
        if (starting_amount + change < 0)
        {
            return starting_amount;
        }
        return change;
    }
}