using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemyDeployment : EnemyDeployment
{
    public Group[] Zone = new Group[1];
    public int CombatStyle = Unit.Melee;

    void Start()
    {
        base.Init();
        Zones.Add(Zone);
    }

    protected override void Update()
    {
        base.Update();
    }

    public override Group[] GetAttackingZone(bool melee)
    {
        return Zone;
    }

    public void place_units(List<Unit> units)
    {
        foreach (Unit u in units)
        {
            PlaceUnit(u);
        }
        CombatStyle = units[0].CombatStyle;
    }

    public override void PlaceUnit(Unit unit)
    {
        Group g = Zone[0];
        if (g != null)
        {
            g.PlaceUnit(unit);
        }
    }
}
