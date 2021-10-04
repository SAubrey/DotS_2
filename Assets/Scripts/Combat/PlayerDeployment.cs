using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PlayerDeployment : Deployment
{
    public static PlayerDeployment I { get; private set; }

    public UnityEngine.Experimental.Rendering.Universal.Light2D light2d;
    private float sprintMaxVel;
    public bool attacking = false;

    // Movement

    public Group[] zone_front_sword = new Group[3];
    public Group[] zone_front_polearm = new Group[3];
    public Group[] zone_center = new Group[1];
    public Group[] zone_rear = new Group[3];

    public bool swordsmen_forward = true;
    public Group[] forward_zone
    {
        get
        {
            if (swordsmen_forward) return zone_front_sword;
            else return zone_front_polearm;
        }
    }

    public bool canMove
    {
        get { return !attacking; }
    }


    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        zones.Add(zone_front_sword);
        zones.Add(zone_front_polearm);
        zones.Add(zone_center);
        zones.Add(zone_rear);
        stamina = MAX_STAMINA;
        isPlayer = true;
        VEL_RUN = VEL_RUN + 100f;
    }

    protected void init()
    {
        update_healthbar();
        update_staminabar();
        stamina = MAX_STAMINA;
    }

    protected override void Update()
    {
        base.Update();

        if (Input.GetMouseButtonDown(0))
        { // Left click
                melee_attack(forward_zone);
        }
        else if (Input.GetMouseButtonDown(1))
        { // Right click
                block();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            // show range_attack landing zone
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Vector3 p = Input.mousePosition;
            p.z = 2000f;
            p = CamSwitcher.I.battle_cam.ScreenToWorldPoint(p);
            range_attack(zone_rear, p);
        }

        // Swap melee and polearm troops.
        if (Input.GetKeyDown(KeyCode.F))
        {
            swordsmen_forward = !swordsmen_forward;
        }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.E))
        {
            toggle_slot_dust_ps(1);
            trigger_begin_rotation_event();
        }
        if (Input.GetKeyUp(KeyCode.Q) || Input.GetKeyUp(KeyCode.E))
        {
            toggle_slot_dust_ps(-1);
            trigger_end_rotation_event();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            /*if (lockedOnEnemy == null) {
                lockedOnEnemy = FindNearestEnemyInSight();
            } else {
                lockedOnEnemy = null;
            }*/
        }
    }

    public void FixedUpdate() {
        Vector2 movement = get_movement_input();
        if (movement != Vector2.zero) 
        {
            move(movement, VEL_RUN);
        }
        else {
            simulate_friction();
        }
    
        if (Input.GetKey(KeyCode.Q))
        {
            rotate(-1);
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotate(1);
        }
    }

    public void simulate_friction() {
        rb.AddForce(-rb.velocity);
    }

    private Vector2 get_movement_input()
    {
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

    public override void place_unit(Unit unit)
    {
        Group[] zone = null;
        if (unit.is_melee)
        {
            zone = unit.has_attribute(Unit.PIERCING) ? zone_front_polearm : zone_front_sword;
        }
        else
        {
            zone = unit.is_range ? zone_rear : zone_center;
        }

        Group g = get_highest_empty_group(zone);
        if (g != null)
        {
            g.place_unit(unit);
        }
    }

    public void place_units(Battalion b)
    {
        foreach (List<PlayerUnit> pu in b.units.Values)
        {
            foreach (Unit u in pu)
            {
                place_unit(u);
            }
        }
        MapUI.I.update_deployment(b);
    }



    protected void block()
    {
        if (stamina < stam_block_cost)
            return;

        foreach (Group g in forward_zone)
        {
            for (int i = 0; i < 3; i++)
            {
                if (g.slots[i].has_unit)
                {
                    Unit u = g.slots[i].get_unit();
                    u.blocking = true;
                }
            }
        }
    }

    public override Group[] get_attacking_zone(bool melee)
    {
        return melee ? forward_zone : zone_rear;
    }


    private GameObject find_nearest_enemy_in_sight()
    {
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

    private void validate_all_punits()
    {
        foreach (Group g in zone_front_sword)
        {
            g.validate_unit_order();
        }
    }

    public float get_stam_regen_amount()
    {
        float vel = 0;//Mathf.Abs(body.velocity.x) + Mathf.Abs(body.velocity.y);
        // Double regeneration when not moving.
        return vel < VEL_RUN / 10 ? stam_regen_amount * 2 : stam_regen_amount;
    }
}
