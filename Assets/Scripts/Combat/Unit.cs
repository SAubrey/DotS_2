using System.Collections.Generic;
using UnityEngine;

public abstract class Unit
{
    public const int PLAYER = 0, ENEMY = 1;

    // Used to determine post-damage decision making. 
    public const int ALIVE = 0, DEAD = 1, INJURED = 2;

    // Boost IDs
    public const int HEALTH = 1, DEFENSE = 2, ATTACK = 3, DAMAGE = 4;


    // Attributes (0 is null atr)
    public const int FLANKING = 1;
    public const int FLYING = 2;
    public const int GROUPING_1 = 3;
    public const int GROUPING_2 = 4;
    public const int STALK = 5;
    public const int PIERCING = 6;
    public const int ARCING_STRIKE = 7;
    public const int AGGRESSIVE = 8;
    public const int TARGET_RANGE = 9;
    public const int TARGET_HEAVY = 10;
    public const int STUN = 11;
    public const int CHARGE = 12;
    public const int PARRY = 13;
    public const int PENETRATING_BLOW = 14;
    public const int CRUSHING_BLOW = 15;

    // Enemy only attributes
    public const int TERROR_1 = 16;
    public const int TERROR_2 = 17;
    public const int TERROR_3 = 18;
    public const int WEAKNESS_POLEARM = 19;

    // Allied only attributes
    public const int REACH = 21;
    public const int INSPIRE = 22;
    public const int HARVEST = 23;
    public const int COUNTER_CHARGE = 24;
    public const int BOLSTER = 25;
    public const int TRUE_SIGHT = 26;
    public const int HEAL_1 = 27;
    public const int COMBINED_EFFORT = 28;

    // Attribute fields
    public int attribute1, attribute2, attribute3 = 0;
    protected bool attribute_active = false;
    public bool attribute_requires_action = false; // alters button behavior.
    public bool passive_attribute = false;

    // Combat fields
    public const int MELEE = 1, RANGE = 2, WEAK_MELEE = 3;
    protected int attack_dmg;
    protected int defense;
    protected float block_rating = 0.5f;
    public int health, max_health;
    public int combat_style;
    public float attack_range = 10f;
    public bool blocking = false;
    protected float block_time = 1f;
    protected Timer block_timer;
    protected float range_time = 1f;
    protected Timer range_timer;
    protected bool can_fire = true;
    public float smooth_speed = .125f;

    protected int type; // Player or Enemy
    protected int ID; // Code for the particular unit type. (not unique to unit)
    public int owner_ID { get; protected set; }

    protected string name;
    // Unique identifier for looking up drawn attack lines and aggregating attacks.

    protected Slot slot = null;
    protected bool dead = false; // Used to determine what to remove in the Battalion.

    private static int static_unique_ID = 0;
    private int unique_ID = static_unique_ID;

    public virtual int calc_hp_remaining(int dmg) { return Mathf.Max(health - dmg, 0); }
    public virtual int get_attack_dmg() { return attack_dmg; }
    public virtual int get_defense() { return defense; }
    public virtual int get_health() { return health; }
    public virtual void remove_boost() { }
    public abstract void die();
    public virtual void animate_attack()
    {
        slot.play_animation(get_attack_animation_ID());
    }
    protected abstract string get_attack_animation_ID();

    // Passed damage should have already accounted for possible defense reduction.
    public virtual int get_post_dmg_state(int dmg_after_def)
    {
        return calc_hp_remaining(dmg_after_def) > 0 ? ALIVE : DEAD;
    }
    public virtual bool set_attribute_active(bool state)
    {
        attribute_active = state && can_activate_attribute();
        if (is_placed)
        {
            slot.update_text_UI();
        }
        return attribute_active;
    }

    public Unit(string name, int ID, int att, int def, int hp, int style,
            int atr1 = 0, int atr2 = 0, int atr3 = 0)
    {
        this.name = name;
        this.ID = ID;
        attack_dmg = att;
        defense = def;
        combat_style = style;
        attack_range = style == MELEE ? 1 : 8;
        health = hp;
        max_health = hp;

        unique_ID = static_unique_ID;
        static_unique_ID++;

        attribute1 = atr1;
        attribute2 = atr2;
        attribute3 = atr3;

        block_timer = new Timer(block_time);
        range_timer = new Timer(range_time);
        smooth_speed = Random.Range(.1f, .15f);
    }

    public bool has_attribute(int atr_ID)
    {
        return (attribute1 == atr_ID ||
                attribute2 == atr_ID ||
                attribute3 == atr_ID);
    }


    public void update_timers(float dt)
    {
        if (get_slot() == null)
            return;
        if (block_timer.Increase(dt))
        {
            blocking = false;
        }
        if (range_timer.Increase(dt))
        {
            can_fire = true;
        }
    }

    public void melee_attack(LayerMask layer_mask)
    {
        if (get_slot() == null)
            return;
        if (slot.melee_att_zone == null)
            return;

        animate_attack();
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(slot.melee_att_zone.transform.position, attack_range, layer_mask);
        //
        foreach (Collider2D h in hits)
        {
            Slot s = h.GetComponent<Slot>();
            if (s == null)
                continue;
            Unit u = s.get_unit();
            if (u == null)
                continue;
            u.take_damage(get_attack_dmg() * 10);
        }
    }

    void OnDrawGizmos() {
        //UnityEditor.Handles.DrawWireDisc(slot.melee_att_zone.transform.position, Vector2.up, attack_range);
        Gizmos.DrawWireSphere(slot.melee_att_zone.transform.position, attack_range);
    }

    public void range_attack(LayerMask mask, Vector3 target_pos)
    {
        if (get_slot() == null)
            return;


        get_slot().range_attack(mask, target_pos);
    }

    public virtual int take_damage(int dmg)
    {
        int final_dmg = calc_dmg_taken(dmg, has_attribute(Unit.PIERCING));
        int state = get_post_dmg_state(final_dmg);
        health = (int)calc_hp_remaining(final_dmg);
        slot.update_healthbar();

        if (state == DEAD)
        {
            die();
        }
        return state;
    }

    protected void change_slotpoint(Slot end)
    {
        slot.empty();
        end.fill(this);
        end.get_group().validate_unit_order();
    }


    protected virtual int calc_dmg_taken(int dmg, bool piercing = false)
    {
        dmg -= get_defense();
        if (blocking && !piercing)
        {
            dmg = (int)(dmg * block_rating);
        }
        return dmg > 0 ? dmg : 0;
    }

    public bool can_attack()
    {
        return attack_dmg > 0;
    }

    public bool can_defend()
    {
        return defense > 0;
    }

    public virtual int get_dynamic_max_health()
    {
        return max_health + get_bonus_health() + get_stat_buff(HEALTH);
    }

    // "Bonus" refers to any stat increases not from boost-type attributes.
    public int get_bonus_health()
    {
        int sum_hp = 0;
        // hp from non-boost attr?
        return sum_hp;
    }

    public int get_bonus_att_dmg()
    {
        int sum_dmg = 0;
        if (is_actively_grouping)
        {
            sum_dmg += ((1 + attack_dmg) * (count_grouped_units() - 1));
        }

        return sum_dmg;
    }

    public int get_bonus_def()
    {
        int sum_def = 0;
        if (is_actively_grouping)
        {
            sum_def += ((1 + defense) * (count_grouped_units() - 1));
        }
        return sum_def;
    }

    public int get_stat_buff(int type)
    {
        if (type != active_boost_type)
            return 0;
        return active_boost_amount;
    }

    public int get_bonus_from_equipment(int stat_ID)
    {
        if (is_enemy)
            return 0;
        Discipline d = TurnPhaser.I.getDisc(owner_ID).bat.disc;
        return d.equipment_inventory.get_stat_boost_amount(ID, stat_ID);
    }

    // Returns number of same units in group with Grouping that have actions remaining.
    public int count_grouped_units()
    {
        int grouped_units = slot.get_group().get_num_of_same_active_units_in_group(ID);
        // Limit grouped units to attribute capacity.
        if (has_attribute(Unit.GROUPING_1) && grouped_units > 2)
        {
            grouped_units = 2;
        }
        return grouped_units;
    }

    public int active_boost_type = -1;
    public int active_boost_amount = 0;
    protected void affect_boosted_stat(int boost_type, int amount)
    {
        active_boost_type = boost_type;
        active_boost_amount = amount;

        if (boost_type == HEALTH)
        {
            health += amount;
        }
        else if (boost_type == ATTACK)
        {
            attack_dmg += amount;
        }
        else if (boost_type == DEFENSE)
        {
            defense += amount;
        }
    }

    private bool _boosted = false;
    public bool boosted
    {
        get { return _boosted; }
        set
        {
            _boosted = value;
            if (!value)
            {
                active_boost_type = -1;
                active_boost_amount = 0;
            }
            if (slot != null)
                slot.update_text_UI();
        }
    }

    /*
    This parent class version does boolean checks for aspects
    that apply to all player units.
    */
    public virtual bool can_activate_attribute()
    {
        if (passive_attribute)
            return false;
        return true;
    }

    public int get_raw_attack_dmg()
    {
        return attack_dmg;
    }

    public int get_raw_defense()
    {
        return defense;
    }

    public int get_type()
    {
        return type;
    }

    public string get_name()
    {
        return name;
    }

    public int get_ID()
    {
        return ID;
    }

    public Slot get_slot()
    {
        return slot;
    }

    public void set_slot(Slot s)
    {
        slot = s;
    }

    public bool is_actively_grouping
    {
        get { return attribute_active && has_grouping; }
    }
    public bool has_grouping
    {
        get { return has_attribute(GROUPING_1) || has_attribute(GROUPING_2); }
    }

    public bool is_attribute_active { get { return attribute_active; } }
    public bool is_melee { get { return combat_style == MELEE; } }
    public bool is_range { get { return combat_style == RANGE; } }
    public bool is_enemy { get { return type == ENEMY; } }
    public bool is_playerunit { get { return type == PLAYER; } }
    public bool is_dead { get { return dead; } }
    public bool is_placed { get { return slot != null; } }
}
