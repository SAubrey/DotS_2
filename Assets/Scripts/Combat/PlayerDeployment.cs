using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerDeployment : Deployment {
    public static PlayerDeployment I { get; private set; }
    
    public UnityEngine.Experimental.Rendering.Universal.Light2D light2d;
    private float sprintMaxVel;
    public bool attacking = false;

    // Movement
    public bool stunned = false;

    public Group[] zone_front_sword = new Group[3];
    public Group[] zone_front_polearm = new Group[3];
    public Group[] zone_center = new Group[1];
    public Group[] zone_rear = new Group[3];

    public bool swordsmen_forward = true;

    public bool canMove {
        get { return !attacking; }
    }


    void Awake() {
        if (I == null) {
            I = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {   
        groups.Add(zone_front_sword);
        groups.Add(zone_front_polearm);
        groups.Add(zone_center);
        groups.Add(zone_rear);
        stamina = MAX_STAMINA;
        isPlayer = true;
    }

    protected void init() {
        update_healthbar();
        update_stamina();
    }

    void Update() {
        if (Input.GetMouseButtonDown(0)) { // Left click
            if (stamina > 0)
                melee_attack();
        } else if (Input.GetKey(KeyCode.Q)) {
            rotate(-1);    
        } else if (Input.GetKey(KeyCode.E)) {
            rotate(1);
        }

        // Swap melee and polearm troops.
        if (Input.GetKeyDown(KeyCode.F)) {
            swordsmen_forward = !swordsmen_forward;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E)) {
            toggle_slot_dust_ps(1);
            trigger_begin_rotation_event();
        } 
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E)) {
            toggle_slot_dust_ps(-1);
            trigger_end_rotation_event();
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            /*if (lockedOnEnemy == null) {
                lockedOnEnemy = FindNearestEnemyInSight();
            } else {
                lockedOnEnemy = null;
            }*/
        } else {
            move(get_movement_input());
        }
    }

    private Vector2 get_movement_input() {
        Vector2 vec = Vector2.zero;
        if (Input.GetKey(KeyCode.W))
            vec += VEC_UP;
         else if (Input.GetKey(KeyCode.S))
            vec += VEC_DOWN;
        if (Input.GetKey(KeyCode.A)) 
            vec += VEC_LEFT;
        else if (Input.GetKey(KeyCode.D)) 
            vec += VEC_RIGHT;
        return vec;
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

    public void take_damage(int dmg) {
        if (invulnerable)
            return;
        
   
   //     stun(timers.normalStunTime);
    }

    public void melee_attack() {
        //animation_player.play(AnimationPlayer.SWORD_SLASH);
        animate_slot_attack(true);
    }

    public void range_attack() {
        animate_slot_attack(false);
    }

    public void animate_slot_attack(bool melee) {
        Group[] gs;
        string anim;
        if (melee) {
            if (swordsmen_forward) {
                gs = zone_front_sword;
                anim = AnimationPlayer.SWORD_SLASH;
            } else {
                gs = zone_front_polearm;
                anim = AnimationPlayer.SPEAR_THRUST;
            }
        } else {
            gs = zone_rear;
            anim = AnimationPlayer.ARROW_FIRE;
        }

        foreach (Group g in gs) {
            for (int i = 0; i < 3; i++) {
                if (g.slots[i].has_unit) {
                    g.slots[i].play_animation(anim);
                }
            }
        }
    }

    public Group[] determine_attacking_zone(bool melee) {
        Group[] gs;
        if (melee) {
            if (swordsmen_forward) {
                gs = zone_front_sword;
            } else {
                gs = zone_front_polearm;
            }
        } else {
            gs = zone_rear;
        }
        return gs;
    }


    private GameObject FindNearestEnemyInSight() {
        int mask = 1 << 9;
        // Scan for enemies with enemy layer mask.
        Collider2D[] enemiesInRange = Physics2D.OverlapCircleAll(
            new Vector2(transform.position.x, transform.position.y), light2d.pointLightOuterRadius, mask);

        Collider2D closestEnemy = StaticOperations.DetermineClosestCollider(enemiesInRange, 
            new Vector2(transform.position.x, transform.position.y));
        if (closestEnemy == null)
            return null;

        return closestEnemy.gameObject;
    }

    private void validate_all_punits() {
        foreach (Group g in zone_front_sword) {
            g.validate_unit_order();
        }
    }


    public float get_stam_regen_amount() {
        float vel = 0;//Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y);
        // Double regeneration when not moving.
        return vel < MAX_VEL / 10 ? stamRegenAmount * 2 : stamRegenAmount;
    }
}
