using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LargeEnemyDeployment : EnemyDeployment
{

    public Group[] zone_front = new Group[3];
    public Group[] zone_rear = new Group[3];

    void Start()
    {
        Zones.Add(zone_front);
        Zones.Add(zone_rear);
        base.Init();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override Group[] GetAttackingZone(bool melee)
    {
        return melee ? zone_front : zone_rear;
    }


    public override void PlaceUnit(Unit unit)
    {
        Group[] zone = null;
        if (unit.IsMelee)
        {
            zone = zone_front;
        }
        else
        {
            zone = zone_rear;
        }

        Group g = GetHighestEmptyGroup(zone);
        if (g != null)
        {
            g.PlaceUnit(unit);
        }
    }

}
