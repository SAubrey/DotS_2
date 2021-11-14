using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    protected Vector3 Low, High;

    void Start()
    {
        RectTransform rt = (RectTransform)gameObject.transform;
        Low.x = rt.rect.xMin;
        High.x = rt.rect.xMax;
        Low.z = rt.rect.yMin;
        High.z = rt.rect.yMax;
        Debug.Log("low: " + Low.x + ", " + Low.z + " high: " + High.x +", " + High.z);
    }

    private Vector3 GetSpawnPos()
    {
        float x = UnityEngine.Random.Range(Low.x, High.x);
        float z = UnityEngine.Random.Range(Low.z, High.z);
        return new Vector3(x, 0f, z);
    }

    public void PlaceDeployment(GameObject d, GameObject parentObj)
    {
        d.transform.localPosition = GetSpawnPos();
    }

    public void Reset()
    {
        Low = Vector3.zero;
        High = Vector3.zero;
    }
}
