using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmallEnemyDeployment : EnemyDeployment {
    public Group[] zone = new Group[1];
    public int combat_style = Unit.MELEE;

    void Start() {
        base.init();
        groups.Add(zone);
    }

    void Update() {
        decision_tree();
    }

    public void place_units(List<Unit> units) {
        foreach (Unit u in units) {
            place_unit(u);
        }
        combat_style = units[0].combat_style;
    }
    
    public override void place_unit(Unit unit) {
        Group g = get_highest_empty_group(zone);
        if (g != null) {
            g.place_unit(unit);
        }
    }
}
