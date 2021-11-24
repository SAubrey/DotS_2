using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    protected Vector3 Low, High;
    RectTransform rt;

    void Start()
    {
        rt = (RectTransform)gameObject.transform;
        // Spawn pos is lower left corner world pos to height/width
        Low.x = rt.rect.xMin;
        High.x = rt.rect.xMax;
        Low.z = rt.rect.yMin;
        High.z = rt.rect.yMax;
        Debug.Log("low: " + (rt.position.x + Low.x) + ", " + (rt.position.z + Low.z) + " high: " + (rt.position.x + High.x) +", " + (rt.position.z + High.z));
    }

    public Vector3 GetSpawnPos()
    {
        float x = rt.position.x + UnityEngine.Random.Range(Low.x, High.x);
        float z = rt.position.z + UnityEngine.Random.Range(Low.z, High.z);
        Debug.Log(x + " , " + z);
        return new Vector3(x, 1f, z);
    }

    public void PlaceDeployment(GameObject d, GameObject parentObj)
    {
        d.transform.position = GetSpawnPos();
    }

    public void Reset()
    {
        Low = Vector3.zero;
        High = Vector3.zero;
    }
}
