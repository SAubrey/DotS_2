using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : Slot
{
    public Image image;
    protected override void Awake()
    {
        Cam = GameObject.Find("MapCamera").GetComponent<Camera>();
        FaceUIToCam();
    }

    protected override void Start() {
        //healthbar_fill.color = healthbar_fill_color;
        //healthbar_bg.color = statbar_bg_color;
    }

    protected override void Update() {
        Move();
    }

    protected override void FixedUpdate() {}

    public override bool Fill(Unit u)
    {
        if (u == null)
            return false;
        SetUnit(u);
        InitUI(u);
        image.color = Color.white;
        return true;
    }

    protected void Move()
    {
        // Slots hold units and smooth lerp towards the static slot points. 
        // Slots points are fixed in a grid in group. Thus, slots are not under the hierarchy of groups anymore.
        // Rotation, movement, and formation change will incur smooth movement. 
        // This also allows slots to move forward towards a destination.

        Vector3 desired_pos = SlotPointTransform.position;
        transform.position = Vector2.Lerp(transform.position, desired_pos, .025f);
    }

    public override void FaceUIToCam()
    { 
        //image.transform.LookAt(cam.transform);
        Frame.transform.LookAt(Cam.transform);
        Frame.transform.forward *= -1;
    }

    protected override void SetUnit(Unit u)
    {
        if (u == null)
            return;
        if (u.IsPlayer)
        {
            Unit = u as PlayerUnit;
        }
        else if (u.IsEnemy)
        {
            Unit = u as Enemy;
        }

    }
}