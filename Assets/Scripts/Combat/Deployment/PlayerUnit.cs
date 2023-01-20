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

    public static readonly List<int> UnitTypes = new List<int> { WARRIOR, SPEARMAN, ARCHER,
        MINER, INSPIRATOR, SEEKER, GUARDIAN, ARBALEST, SKIRMISHER, PALADIN,
        MENDER, CARTER, DRAGOON, SCOUT, DRUMMER, SHIELD_MAIDEN, PIKEMAN };

    public const int EMPTY = 100; // Graphical lookup usage.

    public static PlayerUnit CreatePunit(int ID, int ownerID)
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
        pu.OwnerID = ownerID;
        return pu;
    }

    public PlayerUnit(string name, int ID, int att, int def, int hp, Style style,
            Attributes atr1=Attributes.Null, Attributes atr2=Attributes.Null, Attributes atr3=Attributes.Null) :
            base(name, ID, att, def, hp, style, atr1, atr2, atr3)
    {
        IsPlayer = true;
    }

    public override void Die()
    {
        BatLoader.I.LoadUnitText(TurnPhaser.I.GetDisc(OwnerID).Bat, ID);
        TurnPhaser.I.GetDisc(OwnerID).Bat.RemoveDeadUnit(this);
        Slot.Empty();
    }

    public override int GetAttackDmg()
    {
        return AttackDmg + get_bonus_from_equipment(Unit.ATTACK);
    }

    public override int GetDefense()
    {
        return Defense + get_bonus_from_equipment(Unit.DEFENSE);
    }

    public override int GetDynamicMaxHealth()
    {
        return HealthMax + GetBonusHealth() + get_stat_buff(HEALTH) +
            get_bonus_from_equipment(Unit.HEALTH);
    }

    // ---BOOSTS---
    protected void Boost(int boost_type, int amount)
    {
        if (boosted)
            return;
        affect_boosted_stat(boost_type, amount);
        boosted = true;
    }

    public override void RemoveBoost()
    {
        if (!boosted)
            return;
        affect_boosted_stat(active_boost_type, -active_boost_amount);
        boosted = false;
    }

}

public class Warrior : PlayerUnit
{
    public Warrior() : base("Warrior", WARRIOR, 20, 1, 100, Unit.Style.Sword)
    {
        AttributeRequiresAction = true;
        BlockRating = .9f;
    }
}

public class Spearman : PlayerUnit
{
    public Spearman() : base("Spearman", SPEARMAN, 15, 2, 70, Style.Polearm, Attributes.Piercing, Attributes.CounterCharge)
    {
        PassiveAttribute = true;
    }
}

public class Archer : PlayerUnit
{
    public Archer() : base("Archer", ARCHER, 15, 0, 60, Style.Range)
    {
        PassiveAttribute = true;
    }
}

public class Miner : PlayerUnit
{
    public Miner() : base("Miner", MINER, 10, 0, 50, Style.Mage, Attributes.Harvest)
    {
        PassiveAttribute = true;
    }
}

public class Inspirator : PlayerUnit
{
    public Inspirator() : base("Inspirator", INSPIRATOR, 5, 0, 45, Style.Mage, Attributes.Inspire) { }

    public override bool SetAttributeActive(bool state)
    {
        if (attribute_active == state)
            return false; // prevent double application/depplication
        bool active = base.SetAttributeActive(state);
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
    public Seeker() : base("Seeker", SEEKER, 5, 1, 40, Style.Mage)
    {
        PassiveAttribute = true;
    }
}

public class Guardian : PlayerUnit
{
    public Guardian() : base("Guardian", GUARDIAN, 20, 3, 120, Style.Sword)
    {
        BlockRating = .9f;
    }
}

public class Arbalest : PlayerUnit
{
    public Arbalest() : base("Arbalest", ARBALEST, 30, 0, 75, Style.Range, Attributes.Piercing)
    {
        PassiveAttribute = true;
    }
}

public class Mender : PlayerUnit
{
    public Mender() : base("Mender", MENDER, 5, 3, 40, Style.Mage, Attributes.Heal)
    {
    }

    public override bool SetAttributeActive(bool state)
    {
        bool active = base.SetAttributeActive(state);
        if (active)
        {
            BatLoader.I.SelectingForHeal = active;
            BatLoader.I.HealingUnit = this;
        }
        return active;
    }
}

public class Skirmisher : PlayerUnit
{
    public Skirmisher() : base("Skirmisher", SKIRMISHER, 20, 2, 100, Style.Range, Attributes.Stun)
    {
        AttributeRequiresAction = true;
    }
}

public class Scout : PlayerUnit
{
    public Scout() : base("Scout", SCOUT, 30, 0, 70, Style.Range, Attributes.Piercing)
    {
        PassiveAttribute = true;
    }
}

public class Carter : PlayerUnit
{
    public Carter() : base("Carter", CARTER, 20, 2, 110, Style.Sword)
    {
        PassiveAttribute = true;
        // inv increase by 6
    }
}

public class Dragoon : PlayerUnit
{
    public Dragoon() : base("Dragoon", DRAGOON, 40, 1, 150, Style.Polearm, Attributes.Piercing) { }
}

public class Paladin : PlayerUnit
{
    public Paladin() : base("Paladin", PALADIN, 20, 2, 160, Style.Sword)
    {
        AttributeRequiresAction = true;
    }
}

public class Drummer : PlayerUnit
{
    public Drummer() : base("Drummer", DRUMMER, 10, 1, 50, Style.Mage) { }

    public override bool SetAttributeActive(bool state)
    {
        if (attribute_active == state)
            return false; // prevent double application/depplication
        bool active = base.SetAttributeActive(state);
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
    public ShieldMaiden() : base("Shield Maiden", SHIELD_MAIDEN, 30, 4, 180, Style.Sword)
    {
        AttributeRequiresAction = true;
        BlockRating = 1f;
    }
}

public class Pikeman : PlayerUnit
{
    public Pikeman() : base("Pikeman", PIKEMAN, 30, 1, 120, Style.Polearm, Attributes.Piercing, Attributes.CounterCharge)
    {
        PassiveAttribute = true;
    }
}