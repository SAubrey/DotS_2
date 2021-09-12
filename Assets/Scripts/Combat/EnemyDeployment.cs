using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyDeployment : Deployment {
    public Vector2 player_pos;
    public float player_distance;
    public float search_distance = 30f;
    public float act_distance = 10f;

    public Group[] zone_front = new Group[3];
    public Group[] zone_rear = new Group[3];

    void Start() {
        PlayerDeployment.I.on_position_change += update_player_pos;
        isEnemy = true;
        groups.Add(zone_front);
        groups.Add(zone_rear);
    }

    void Update() {
        decision_tree();
    }

    private void update_player_pos(Vector2 pos) {
        player_pos = pos;
    }

    public void decision_tree() {
        if (player_distance < search_distance) {
            if (player_distance < act_distance) {
                act();
                return;
            }
            search();
            return;
        }
    }

    public void place_unit(Unit unit) {
        Group[] zone = null;
        if (unit.is_melee) {
            zone = zone_front;
        } else {
            zone = zone_rear;
        } 

        Group g = get_highest_empty_group(zone);
        if (g != null) {
            g.place_unit(unit);
        }
    }

    public void search() {

        // Move to a random target location at a random distance
    }

    public void act() {
        //agent.SetDestination(player_pos);
    }

    protected void set_player_distance(Vector3 pos) {
        player_distance = Vector2.Distance(transform.position, pos);
    }
}

public class BehaviorWander {
    private int search_time;
}
