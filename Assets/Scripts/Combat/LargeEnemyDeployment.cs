using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeEnemyDeployment : EnemyDeployment
{

    public Group[] zone_front = new Group[3];
    public Group[] zone_rear = new Group[3];

    void Start()
    {
        zones.Add(zone_front);
        zones.Add(zone_rear);
        base.init();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override Group[] get_attacking_zone(bool melee)
    {
        return melee ? zone_front : zone_rear;
    }


    public override void place_unit(Unit unit)
    {
        Group[] zone = null;
        if (unit.is_melee)
        {
            zone = zone_front;
        }
        else
        {
            zone = zone_rear;
        }

        Group g = get_highest_empty_group(zone);
        if (g != null)
        {
            g.place_unit(unit);
        }
    }

}
