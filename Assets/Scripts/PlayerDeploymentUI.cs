using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDeploymentUI : MonoBehaviour {
    public Group[] zone_front_sword = new Group[3];
    public Group[] zone_front_polearm = new Group[3];
    public Group[] zone_center = new Group[1];
    public Group[] zone_rear = new Group[3];
    protected List<Group[]> groups = new List<Group[]>();

    void Start() {   
        groups.Add(zone_front_sword);
        groups.Add(zone_front_polearm);
        groups.Add(zone_center);
        groups.Add(zone_rear);
    }
    
    public void place_units(Battalion b) {
        foreach (List<PlayerUnit> pu in b.units.Values) {
            foreach (Unit u in pu) {
                place_unit(u);
            }
        }
    }

    public void place_unit(Unit unit) {
        Group[] zone = null;
        if (unit.is_melee) {
            if (unit.has_attribute(Unit.PIERCING)) {
                zone = zone_front_polearm;
            } else {
                zone = zone_front_sword;
            }
        } else if (unit.is_range) {
            zone = zone_rear;
        } else {
            zone = zone_center;
        }

        Group g = get_highest_empty_group(zone);
        if (g != null) {
            g.place_unit(unit);
        }
    }
    
    public Group get_highest_empty_group(Group[] groups) {
        foreach (Group g in groups) {
            if (!g.is_full) {
                return g;
            }
        }
        return null;
    }
}
