using UnityEngine;
using System.Collections.Generic;

public class PlayerUnit : Unit
{
    public const int WARRIOR = 0;
    public const int SPEARMAN = WARRIOR + 1;
    public const int ARCHER = SPEARMAN + 1;
    public const int MINER = ARCHER + 1;
    public const int INSPIRATOR = MINER + 1;
    public const int SEEKER = INSPIRATOR + 1;
    public const int GUARDIAN = SEEKER + 1;
    public const int ARBALEST = GUARDIAN + 1;
    public const int SKIRMISHER = ARBALEST + 1;
    public const int PALADIN = SKIRMISHER + 1;
    public const int MENDER = PALADIN + 1;
    public const int CARTER = MENDER + 1;
    public const int DRAGOON = CARTER + 1;
    public const int SCOUT = DRAGOON + 1;
    public const int DRUMMER = SCOUT + 1;
    public const int SHIELD_MAIDEN = DRUMMER + 1;
    public const int PIKEMAN = SHIELD_MAIDEN + 1;

    public static readonly List<int> unit_types = new List<int> { WARRIOR, SPEARMAN, ARCHER,
        MINER, INSPIRATOR, SEEKER, GUARDIAN, ARBALEST, SKIRMISHER, PALADIN,
        MENDER, CARTER, DRAGOON, SCOUT, DRUMMER, SHIELD_MAIDEN, PIKEMAN };

    public const int EMPTY = 100; // Graphical lookup usage.

    public static PlayerUnit create_punit(int ID, int owner_ID)
    {
        PlayerUnit pu = null;
        if (ID == WARRIOR) pu = new Warrior();
        else if (ID == SPEARMAN) pu = new Spearman();
        else if (ID == ARCHER) pu = new Archer();
        else if (ID == MINER) pu = new Miner();
        else if (ID == INSPIRATOR) pu = new Inspirator();
        else if (ID == SEEKER) pu = new Seeker();
        else if (ID == GUARDIAN) pu = new Guardian();
        else if (ID == ARBALEST) pu = new Arbalest();
        else if (ID == SKIRMISHER) pu = new Skirmisher();
        else if (ID == PALADIN) pu = new Paladin();
        else if (ID == MENDER) pu = new Mender();
        else if (ID == DRUMMER) pu = new Drummer();
        else if (ID == PIKEMAN) pu = new Pikeman();
        else if (ID == CARTER) pu = new Carter();
        else if (ID == DRAGOON) pu = new Dragoon();
        else if (ID == SCOUT) pu = new Scout();
        else if (ID == SHIELD_MAIDEN) pu = new ShieldMaiden();
        pu.owner_ID = owner_ID;
        return pu;
    }

    public PlayerUnit(string name, int ID, int att, int def, int hp, int style,
            int atr1 = 0, int atr2 = 0, int atr3 = 0) :
            base(name, ID, att, def, hp, style, atr1, atr2, atr3)
    {
        type = PLAYER;
    }

    public override void die()
    {
        BatLoader.I.load_unit_text(TurnPhaser.I.getDisc(owner_ID).bat, ID);
        TurnPhaser.I.getDisc(owner_ID).bat.remove_dead_unit(this);
        slot.empty();
    }

    protected override string get_attack_animation_ID()
    {
        if (combat_style == RANGE)
        {
            return AnimationPlayer.ARROW_FIRE;
        }
        else if (ID == SPEARMAN || ID == PIKEMAN || ID == CARTER
          || ID == GUARDIAN || ID == SKIRMISHER)
        {
            return AnimationPlayer.SPEAR_THRUST;
        }
        else
        {
            return AnimationPlayer.SWORD_SLASH;
        }
    }

    public override int get_attack_dmg()
    {
        return attack_dmg + get_bonus_att_dmg() + get_bonus_from_equipment(Unit.ATTACK);
    }

    public override int get_defense()
    {
        return defense + get_bonus_def() + get_bonus_from_equipment(Unit.DEFENSE);
    }

    public override int get_dynamic_max_health()
    {
        return max_health + get_bonus_health() + get_stat_buff(HEALTH) +
            get_bonus_from_equipment(Unit.HEALTH);
    }

    // ---ATTRIBUTES---
    /*
        protected void apply_surrounding_effect(int boost_type, int amount, List<Pos> coords) {
            Group g;
            Pos low = coords[0];
            Pos high = coords[1];
            for (int col = low.x; col <= high.x; col++) {
                for (int row = low.y; row <= high.y; row++) {
                    g = f.get_group(col, row);
                    if (!g) 
                        continue;

                    foreach (Slot s in g.slots) {
                        if (!s.has_punit) // Skip empty or enemies.
                            continue;

                        if (s.get_punit().boosted) {
                            s.get_punit().remove_boost();
                        } else {
                            s.get_punit().boost(boost_type, amount);
                        }
                    }
                }
            }
        }*/

    // ---BOOSTS---
    protected void boost(int boost_type, int amount)
    {
        if (boosted)
            return;
        affect_boosted_stat(boost_type, amount);
        boosted = true;
    }

    public override void remove_boost()
    {
        if (!boosted)
            return;
        affect_boosted_stat(active_boost_type, -active_boost_amount);
        boosted = false;
    }

}

public class Warrior : PlayerUnit
{
    public Warrior() : base("Warrior", WARRIOR, 1, 1, 100, MELEE, GROUPING_1)
    {
        attribute_requires_action = true;
        block_rating = .9f;
    }
}

public class Spearman : PlayerUnit
{
    public Spearman() : base("Spearman", SPEARMAN, 1, 2, 70, MELEE, PIERCING, COUNTER_CHARGE)
    {
        passive_attribute = true;
    }
}

public class Archer : PlayerUnit
{
    public Archer() : base("Archer", ARCHER, 2, 0, 60, RANGE)
    {
        passive_attribute = true;
    }
}

public class Miner : PlayerUnit
{
    public Miner() : base("Miner", MINER, 1, 0, 50, WEAK_MELEE, HARVEST)
    {
        passive_attribute = true;
    }
}

public class Inspirator : PlayerUnit
{
    public Inspirator() : base("Inspirator", INSPIRATOR, 0, 0, 45, WEAK_MELEE, INSPIRE) { }

    public override bool set_attribute_active(bool state)
    {
        if (attribute_active == state)
            return false; // prevent double application/depplication
        bool active = base.set_attribute_active(state);
        if (active)
        {
            //apply_surrounding_effect(HEALTH, 1, get_forward3x1_coords());
            // affecting num actions directly bypasses one-way street to allow
            // the user to undo and redo the buff. This means has_acted_in_stage
            // does not get enabled for inspirator, which only matters if units
            // are cycling after having acted.
        }
        else
        {
            //apply_surrounding_effect(HEALTH, -1, get_forward3x1_coords());
        }
        return active;
    }
}

public class Seeker : PlayerUnit
{
    public Seeker() : base("Seeker", SEEKER, 1, 1, 40, WEAK_MELEE, TRUE_SIGHT)
    {
        passive_attribute = true;
    }
}

public class Guardian : PlayerUnit
{
    public Guardian() : base("Guardian", GUARDIAN, 2, 3, 120, MELEE, PARRY)
    {
        block_rating = .9f;
    }
}

public class Arbalest : PlayerUnit
{
    public Arbalest() : base("Arbalest", ARBALEST, 3, 0, 75, RANGE, PIERCING)
    {
        passive_attribute = true;
    }
}

public class Mender : PlayerUnit
{
    public Mender() : base("Mender", MENDER, 0, 3, 40, WEAK_MELEE, HEAL_1)
    {
    }

    public override bool set_attribute_active(bool state)
    {
        bool active = base.set_attribute_active(state);
        if (active)
        {
            BatLoader.I.selecting_for_heal = active;
            BatLoader.I.healing_unit = this;
        }
        return active;
    }
}

public class Skirmisher : PlayerUnit
{
    public Skirmisher() : base("Skirmisher", SKIRMISHER, 2, 2, 100, RANGE, STUN, GROUPING_2)
    {
        attribute_requires_action = true;
    }
}

public class Scout : PlayerUnit
{
    public Scout() : base("Scout", SCOUT, 3, 0, 70, RANGE, PIERCING)
    {
        passive_attribute = true;
    }
}

public class Carter : PlayerUnit
{
    public Carter() : base("Carter", CARTER, 2, 2, 110, MELEE)
    {
        passive_attribute = true;
        // inv increase by 6
    }
}

public class Dragoon : PlayerUnit
{
    public Dragoon() : base("Dragoon", DRAGOON, 4, 1, 150, MELEE, GROUPING_2, PIERCING) { }
}

public class Paladin : PlayerUnit
{
    public Paladin() : base("Paladin", PALADIN, 2, 2, 160, MELEE, GROUPING_2)
    {
        attribute_requires_action = true;
    }
}

public class Drummer : PlayerUnit
{
    public Drummer() : base("Drummer", DRUMMER, 1, 1, 50, WEAK_MELEE, BOLSTER) { }

    public override bool set_attribute_active(bool state)
    {
        if (attribute_active == state)
            return false; // prevent double application/depplication
        bool active = base.set_attribute_active(state);
        if (active)
        {
            //apply_surrounding_effect(DEFENSE, 1, get_forward3x1_coords());
        }
        else
        {
            //apply_surrounding_effect(DEFENSE, -1, get_forward3x1_coords());
        }
        return active;
    }
}

public class ShieldMaiden : PlayerUnit
{
    public ShieldMaiden() : base("Shield Maiden", SHIELD_MAIDEN, 3, 4, 180, MELEE, GROUPING_1, COMBINED_EFFORT)
    {
        attribute_requires_action = true;
        block_rating = 1f;
    }
}

public class Pikeman : PlayerUnit
{
    public Pikeman() : base("Pikeman", PIKEMAN, 3, 1, 120, MELEE, REACH, PIERCING, COUNTER_CHARGE)
    {
        passive_attribute = true;
    }
}