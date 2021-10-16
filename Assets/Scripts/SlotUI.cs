using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : Slot
{
    public Image image;
    void Awake()
    {
        cam = GameObject.Find("MapCamera").GetComponent<Camera>();
        face_cam();
    }

    protected override void Start() {
        //healthbar_fill.color = healthbar_fill_color;
        //healthbar_bg.color = statbar_bg_color;
    }

    void Update() {
        move();
    }

    protected override void FixedUpdate() {}

    public override bool fill(Unit u)
    {
        if (u == null)
            return false;
        set_unit(u);
        init_UI(u);
        image.color = Color.white;
        image.sprite = BatLoader.I.get_unit_img(u, Group.DOWN);
        return true;
    }

    protected void move()
    {
        // Slots hold units and smooth lerp towards the static slot points. 
        // Slots points are fixed in a grid in group. Thus, slots are not under the hierarchy of groups anymore.
        // Rotation, movement, and formation change will incur smooth movement. 
        // This also allows slots to move forward towards a destination.

        Vector3 desired_pos = slot_point_transform.position;
        transform.position = Vector2.Lerp(transform.position, desired_pos, .025f);
    }

    public override void face_cam()
    { 
        //image.transform.LookAt(cam.transform);
        frame.transform.LookAt(cam.transform);
        frame.transform.forward *= -1;
    }

    protected override void set_unit(Unit u)
    {
        if (u == null)
            return;
        if (u.is_playerunit)
        {
            unit = u as PlayerUnit;
        }
        else if (u.is_enemy)
        {
            unit = u as Enemy;
        }

    }
}