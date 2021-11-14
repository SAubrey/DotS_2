using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    public static LineDrawer I { get; private set; }
    public GameObject LinePrefab;
    public PreviewLine preview_line;
    public Dictionary<int, Line> lines = new Dictionary<int, Line>();
    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        preview_line = GetComponentInChildren<PreviewLine>();
        preview_line.init();
    }

    public void DrawLine(Unit start_u, Vector3 start_pos, Vector3 end_pos, int id)
    {
        if (start_u == null)
        {
            return;
        }
        GameObject L = GameObject.Instantiate(LinePrefab);
        Line line = L.GetComponent<Line>();
        line.Init(start_u, id, start_pos, end_pos);

        // manage ID
        lines.Add(id, line);
    }

    public Line get_line(int id)
    {
        if (!lines.ContainsKey(id))
            return null;
        return lines[id];
    }

    public void remove(int id)
    {
        if (lines.ContainsKey(id))
        {
            lines[id].Remove();
            lines.Remove(id);
        }
    }

    public void clear()
    {
        foreach (Line l in lines.Values)
        {
            if (l != null && l.gameObject != null)
                l.Remove();
        }
        lines.Clear();
        preview_line.erase();
    }
}