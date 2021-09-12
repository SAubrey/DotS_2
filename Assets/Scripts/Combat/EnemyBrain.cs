using System.Collections.Generic;
using UnityEngine;


public class EnemyBrain : MonoBehaviour {
    public static EnemyBrain I { get; private set; }

    private List<Enemy> enemies {
        get { return Map.I.get_enemies_here(); }
    }
    void Awake() {
        if (I == null) {
            I = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void attack(Enemy enemy) {
        Slot target = enemy.target;
        if (!target)
            return;
        // Ignore enemies not in slot 0 of group.
        if (enemy.get_slot().get_group().slots[0] != enemy.get_slot())
            return;

        enemy.get_slot().get_group().rotate_towards_target(target.get_group());


    }

    public void move_units() {
        foreach (Enemy enemy in enemies) {
            // Don't move if within attacking distance.
            if (in_attack_range(enemy.get_slot(), enemy.target))
                continue;

        }
    }

    public void stage_attacks() {
        foreach (Enemy enemy in enemies) {
            if (in_attack_range(enemy.get_slot(), enemy.target))
                attack(enemy);
        }
    }

    public void stage_range_attacks() {
        foreach (Enemy enemy in enemies) {
            if (enemy.is_range && 
                    in_attack_range(enemy.get_slot(), enemy.target)) {
                attack(enemy);
            }
        }
    }

    private bool check_adjacent(Slot start, Slot end) {
        return Statics.calc_distance(start, end) == 1 ? true : false;
    }

    private bool in_attack_range(Slot start, Slot end) {
        int range = start.get_enemy().attack_range;
        //Debug.Log("range: " + range + " distance: " + calc_distance(start, end));
        return Statics.calc_distance(start, end) <= range ? true : false;
    }
}