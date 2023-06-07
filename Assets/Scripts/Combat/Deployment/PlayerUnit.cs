using UnityEngine;
using System.Collections.Generic;

public class PlayerUnit : Unit
{
    public const int WARRIOR = 0;
    public const int SPEARMAN = WARRIOR + 1;
    public const int ARCHER = SPEARMAN + 1;
    public const int GUARDIAN = ARCHER + 1;
    public const int ARBALEST = GUARDIAN + 1;
    public const int PALADIN = ARBALEST + 1;
    public const int MENDER = PALADIN + 1;

    public static readonly List<int> UnitTypes = new List<int> { WARRIOR, SPEARMAN, ARCHER,
        GUARDIAN, ARBALEST, PALADIN,
        MENDER };

    public const int EMPTY = 100; // Graphical lookup usage.

    public static PlayerUnit CreatePunit(int ID, int ownerID)
    {
        PlayerUnit pu = null;
        if (ID == WARRIOR) pu = new Warrior();
        else if (ID == SPEARMAN) pu = new Spearman();
        else if (ID == ARCHER) pu = new Archer();
        else if (ID == GUARDIAN) pu = new Guardian();
        else if (ID == ARBALEST) pu = new Arbalest();
        else if (ID == PALADIN) pu = new Paladin();
        else if (ID == MENDER) pu = new Mender();
        pu.OwnerID = ownerID;
        return pu;
    }

    public PlayerUnit(string name, int ID, int speed, int att, int def, int hp, Style style,
            Attributes atr1=Attributes.Null, Attributes atr2=Attributes.Null, Attributes atr3=Attributes.Null) :
            base(name, ID, speed, att, def, hp, style, atr1, atr2, atr3)
    {
        IsPlayer = true;
        TargetMask = "Enemy";
        MyMask = "Player";
    }

    public override void Die()
    {
        BatLoader.I.LoadUnitText(TurnPhaser.I.GetDisc(OwnerID).Bat, ID);
        TurnPhaser.I.GetDisc(OwnerID).Bat.RemoveDeadUnit(this);
        base.Die();
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
    public Warrior() : base("Warrior", WARRIOR, 7, 20, 1, 100, Unit.Style.Sword)
    {
        AttributeRequiresAction = true;
        BlockRating = .9f;
    }
}

public class Spearman : PlayerUnit
{
    public Spearman() : base("Spearman", SPEARMAN, 7, 15, 2, 70, Style.Polearm, Attributes.Piercing, Attributes.CounterCharge)
    {
        PassiveAttribute = true;
    }
}

public class Archer : PlayerUnit
{
    public Archer() : base("Archer", ARCHER, 7, 15, 0, 60, Style.Range)
    {
        PassiveAttribute = true;
    }
}

public class Guardian : PlayerUnit
{
    public Guardian() : base("Guardian", GUARDIAN, 7, 20, 3, 120, Style.Sword)
    {
        BlockRating = .9f;
    }
}

public class Arbalest : PlayerUnit
{
    public Arbalest() : base("Arbalest", ARBALEST, 7, 30, 0, 75, Style.Range, Attributes.Piercing)
    {
        PassiveAttribute = true;
    }
}

public class Mender : PlayerUnit
{
    public Mender() : base("Mender", MENDER, 7, 5, 3, 40, Style.Mage, Attributes.Heal)
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

public class Paladin : PlayerUnit
{
    public Paladin() : base("Paladin", PALADIN, 7, 20, 2, 160, Style.Sword)
    {
        AttributeRequiresAction = true;
    }
}
