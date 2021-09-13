using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyDeployment : Deployment {
    protected Vector2 player_pos;
    protected float _player_distance;
    protected float player_distance {
        get { return _player_distance; }
        set {
            _player_distance = Mathf.Max(0, value - player_distance_offset);
        }
    }
    protected float player_distance_offset = 250f;
    protected float search_distance = 1000f;
    protected float chase_distance = 700f;
    protected float act_distance = 15f;
    protected bool locked_on = false;
    protected Vector2 target = Vector2.zero;
    
    public virtual void place_unit(Unit unit) { }

    protected void init() {
        PlayerDeployment.I.on_position_change += update_player_pos;
        isEnemy = true;
    }

    private void update_player_pos(Vector2 pos) {
        player_pos = pos;
    }

    public virtual void decision_tree() {
        set_player_distance(player_pos);
        if (player_distance == 0) {
            back_up();
        }
        if (locked_on) {
            rotate_towards_target(player_pos);
        }
        if (player_distance < search_distance) {
            if (player_distance < chase_distance) {
                if (player_distance < act_distance) {
                    act();
                    return;
                }
                chase();
                return;
            }
            locked_on = false;
            search();
            return;
        } else {
            wander();
        }
    }

    protected Quaternion target_rotation;
    protected void rotate_towards_target(Vector3 pos) {

        target_rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.Atan2((pos.y - transform.position.y), 
            (pos.x - transform.position.x)) * Mathf.Rad2Deg - 90f));
        
        Vector3 variance = gameObject.transform.rotation.eulerAngles - get_direction_to_player();
        Debug.Log(variance);
        variance.Normalize();
        if (variance.x < 0.1f && variance.y < 0.1f) {
            return;
        }
        
        // Lerp smooths and thus limits rotation speed.
        float str = Mathf.Min(5 * Time.deltaTime, 1);
        gameObject.transform.rotation = Quaternion.Lerp(gameObject.transform.rotation, target_rotation, str);
    }

    public virtual void search() {

    }

    public void chase() {
        locked_on = true;
        gameObject.transform.position = 
            Vector2.MoveTowards(gameObject.transform.position, player_pos, MAX_VEL/100f);
    }

    public virtual void act() {
        // Attack
        
    }

    public virtual void back_up() {
        move(-get_direction_to_player());
    }

    public void wander() {
        // Move to a random target location at a random distance

    }

    protected void set_player_distance(Vector3 pos) {
        player_distance = Vector2.Distance(transform.position, pos);
    }

    protected Vector3 get_direction_to_player() {
        return StaticOperations.target_unit_vec(gameObject.transform.position, player_pos);
    }
}

public class BehaviorWander {
    private int search_time;
}
