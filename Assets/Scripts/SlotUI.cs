using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SlotUI : Slot
{

    void Awake()
    {
        cam = GameObject.Find("MapCamera").GetComponent<Camera>();
        face_cam();
    }
    void Start()
    {
    }

    public override bool fill(Unit u)
    {
        if (u == null)
            return false;
        set_unit(u);
        init_UI(u);
        sprite_unit.color = Color.white;
        sprite_unit.sprite = BatLoader.I.get_unit_img(u, Group.DOWN);
        //if (u.is_playerunit)
        //toggle_light(true);
        return true;
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