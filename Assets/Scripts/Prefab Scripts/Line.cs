using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line : MonoBehaviour
{
    private const float TIMEOUT = 3f;
    private float time_alive = 0;
    public bool fading = false;
    public LineRenderer lr;
    public int id;
    public virtual void Init(Unit u, int id,
                Vector3 start_pos, Vector3 end_pos)
    {
        this.id = id;
        Draw(u, start_pos, end_pos);
    }

    void Update()
    {
        if (!fading)
            return;

        Fade();
        time_alive += Time.deltaTime;
        if (time_alive >= TIMEOUT)
            Remove();
    }

    private void Fade()
    {
        Vector4 sc = lr.startColor;
        Vector4 ec = lr.endColor;
        lr.startColor = new Color(sc[0], sc[1], sc[2], (1 - (time_alive / TIMEOUT)) * 0.1f);
        lr.endColor = new Color(ec[0], ec[1], ec[2], 1 - (time_alive / TIMEOUT));
    }

    public void Draw(Unit u, Vector3 start_pos, Vector3 end_pos)
    {
        if (u == null)
        {
            Debug.Log("not drawing null line");
            return;
        }

        lr.sortingLayerName = "Top";
        lr.positionCount = 2;
        lr.startWidth = 20f;
        lr.endWidth = 1f;
        if (u.IsPlayer)
        {
            lr.startColor = new Color(.9f, .1f, .1f, 0);
            lr.endColor = new Color(.9f, .1f, .1f, .4f);
        }
        else
        {
            lr.startColor = new Color(1, 1, 1, 0);
            lr.endColor = new Color(1, 1, 1, .4f);
        }
        lr.SetPosition(0, start_pos);
        lr.SetPosition(1, end_pos);
        lr.useWorldSpace = true;
    }

    public void BeginFade()
    {
        fading = true;
    }

    public void Remove()
    {
        GameObject.Destroy(this.gameObject);
    }
}