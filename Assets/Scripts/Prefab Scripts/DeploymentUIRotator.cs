using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class DeploymentUIRotator : EventTrigger
{
    public Image hoverable;
    public float expandedX, retractedX;
    public float expandedScale, retractedScale;
    public bool expanding = false;

    void Update()
    {
        float x = expanding ? expandedX : retractedX;
        SmoothRotate(x);
        float scale = expanding ? expandedScale : retractedScale;
        SmoothScale(scale);
    }

    private void SmoothRotate(float targetX)
    {
        if (targetX == transform.rotation.x)
            return;
        //float dx = Mathf.Lerp(transform.rotation.x, targetX, .125f);
        //transform.rotation = Quaternion.Euler(new Vector3(dx, 0f, 0f));
        Quaternion rotation = Quaternion.Euler(new Vector3(targetX, 0f, 0f));
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, .125f);
    }

    private void SmoothScale(float scale) {
        float sx = Mathf.Lerp(transform.localScale.x, scale, .125f);
        transform.localScale = new Vector3(sx, sx, sx);
    }

    public void OnEnter(BaseEventData b)
    {
        expanding = true;
    }

    public void OnExit(BaseEventData b)
    {
        expanding = false;
    }
}
