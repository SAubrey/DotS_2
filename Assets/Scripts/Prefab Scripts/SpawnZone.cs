using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    protected Vector2 low;
    protected Vector2 high;
    //public Pos current_pos;

    void Start()
    {
        RectTransform rt = (RectTransform)gameObject.transform;
        low.x = rt.rect.xMin;
        high.x = rt.rect.xMax;
        low.y = rt.rect.yMin;
        high.y = rt.rect.yMax;
        Debug.Log("low: " + low.x + ", " + low.y + " high: " + high.x +", " + high.y);
    }

    public Vector2 get_spawn_pos()
    {
        float x = UnityEngine.Random.Range(low.x, high.x);
        float y = UnityEngine.Random.Range(low.y, high.y);
        return new Vector2(x, y);
    }

    public void place_deployment(GameObject d, GameObject parent_obj)
    {
        //d.transform.SetParent(parent_obj.transform);
        d.transform.localPosition = get_spawn_pos();
    }

    public void reset()
    {
        low = Vector2.zero;
        high = Vector2.zero;
    }
}
