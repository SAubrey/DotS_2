using System.Collections.Generic;
using UnityEngine;

public abstract class Unit
{
    // Used to determine post-damage decision making. 
    public const int ALIVE = 0, DEAD = 1;
    // Boost IDs
    public const int HEALTH = 1, DEFENSE = 2, ATTACK = 3, DAMAGE = 4;

    public enum Attributes
    {
        Null, Piercing, Harvest, WeaknessPolearm, Aggressive, Flying, Flanking, 
        CounterCharge, Heal, Stun, ArcingStrike, Stalk, Charge, Inspire
    }

    public bool IsAttributeActive { get { return attribute_active; } }
    public bool IsMelee { get { return CombatStyle == Style.Sword || CombatStyle == Style.Polearm; } }
    public bool IsRange { get { return CombatStyle == Style.Range; } }
    public bool IsMage { get { return CombatStyle == Style.Mage; } }
    public bool IsDead { get { return Dead; } }
    public bool IsPlaced { get { return Slot != null; } }
    // Attribute fields
    public Attributes Attribute1, Attribute2, Attribute3;
    public int AbilityEnabled { get; protected set; }
    protected bool attribute_active = false;
    public bool AttributeRequiresAction = false; // alters button behavior.
    public bool PassiveAttribute = false;

    // Combat fields
    public enum Style 
    {
        Sword, Polearm, Range, Mage, WeakMelee
    }
    public const int Melee = 1, Range = 2, Mage = 3, WeakMelee = 4;
    protected int AttackDmg;
    protected int Defense;
    protected float BlockRatingBase = .25f;
    protected float BlockRatingAdditional = 0f;
    // Damage taken multiplier when actively blocking.
    protected float BlockRating {
        get 
        {
            return Mathf.Max(0f, 1f - (BlockRatingBase + BlockRatingAdditional));
        }
        set { return; }
    }
    public int Health;
    public int HealthMax { get; protected set; }
    public Style CombatStyle { get; protected set; }
    protected float AttackRange = 10f;
    protected bool _blocking = false;
    public bool Blocking {
        get { return _blocking; }
        set 
        {
            _blocking = value;
            if (Slot != null)
            {
                Slot.Animator.SetBool("Block", value);
            }
        }
    }
    protected float BlockTime = 1f;
    protected Timer BlockTimer;
    protected float RangeTime = 1f;
    protected Timer RangeTimer;
    protected bool CanFire = true;
    protected float SmoothSpeed = .125f;

    public bool IsPlayer { get; protected set; }
    public bool IsEnemy { get { return !IsPlayer; } }
    public int ID { get; protected set; }// Code for the particular unit type. (not unique to unit)
    public int OwnerID { get; protected set; }

    protected string Name;
    public Slot Slot { get; protected set; }= null;
    protected bool Dead = false;

    public virtual int CalcHpRemaining(int dmg) { return Mathf.Max(Health - dmg, 0); }
    public virtual int GetAttackDmg() { return AttackDmg; }
    public virtual int GetDefense() { return Defense; }
    public virtual int GetHealth() { return Health; }
    public virtual void RemoveBoost() { }
    public abstract void Die();
    public virtual void AnimateAttackEffect()
    {
        Slot.PlayAnimationEffect(GetAttackAnimationID());
    }
    protected abstract string GetAttackAnimationID();

    // Passed damage should have already accounted for possible defense reduction.
    public virtual int GetPostDmgState(int dmgAfterDef)
    {
        return CalcHpRemaining(dmgAfterDef) > 0 ? ALIVE : DEAD;
    }
    public virtual bool SetAttributeActive(bool state)
    {
        attribute_active = state && can_activate_attribute();
        if (IsPlaced)
        {
            Slot.UpdateTextUI();
        }
        return attribute_active;
    }

    public Unit(string name, int ID, int att, int def, int hp, Style style,
            Attributes atr1, Attributes atr2, Attributes atr3)
    {
        this.Name = name;
        this.ID = ID;
        AttackDmg = att;
        Defense = def;
        CombatStyle = style;
        Health = hp;
        HealthMax = hp;

        Attribute1 = atr1;
        Attribute2 = atr2;
        Attribute3 = atr3;

        BlockTimer = new Timer(BlockTime);
        RangeTimer = new Timer(RangeTime);
        SmoothSpeed = Random.Range(.1f, .15f);
    }

    public bool HasAttribute(Attributes atr)
    {
        return (Attribute1 == atr ||
                Attribute2 == atr ||
                Attribute3 == atr);
    }

    public void UpdateTimers(float dt)
    {
        if (GetSlot() == null)
            return;
        if (BlockTimer.Increase(dt))
        {
            Blocking = false;
        }
        if (RangeTimer.Increase(dt))
        {
            CanFire = true;
        }
    }

    public void MeleeAttack(LayerMask layerMask)
    {
        if (GetSlot() == null)
            return;
        if (Slot.MeleeAttZone == null)
            return;

        //AnimateAttackEffect();
        Slot.Animator.SetTrigger("Attack");
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(Slot.MeleeAttZone.transform.position, AttackRange, layerMask);
        //
        foreach (Collider2D h in hits)
        {
            Slot s = h.GetComponent<Slot>();
            if (s == null)
                continue;
            Unit u = s.Unit;
            if (u == null)
                continue;
            u.TakeDamage(GetAttackDmg() * 10);
        }
    }

    void OnDrawGizmos() {
        //UnityEditor.Handles.DrawWireDisc(slot.melee_att_zone.transform.position, Vector2.up, attack_range);
        Gizmos.DrawWireSphere(Slot.MeleeAttZone.transform.position, AttackRange);
    }

    public void RangeAttack(LayerMask mask, Vector3 targetPos)
    {
        if (GetSlot() == null)
            return;

        GetSlot().ShootArrow(mask, targetPos, GetAttackDmg());
    }

    public virtual int TakeDamage(int dmg)
    {
        int finalDmg = CalcDmgTaken(dmg, HasAttribute(Unit.Attributes.Piercing));
        int state = GetPostDmgState(finalDmg);
        Health = (int)CalcHpRemaining(finalDmg);
        Slot.UpdateHealthbar();

        // Don't trigger an out of synch animation.
        if (!Slot.Animator.GetCurrentAnimatorStateInfo(0).IsName("TakeDamage"))
        {
            Slot.Animator.SetTrigger("TakeDamage");
        }
        Debug.Log(Name + " took " + finalDmg + " from " + dmg + " with " + Health + " hp remaining.");

        if (state == DEAD)
        {
            Die();
        }
        return state;
    }

    protected virtual int CalcDmgTaken(int dmg, bool piercing = false)
    {
        if (Blocking && !piercing)
        {
            dmg = (int)(dmg * BlockRating);
        }
        return dmg > 0 ? dmg : 0;
    }

    public bool CanDefend()
    {
        return Defense > 0;
    }

    public virtual int GetDynamicMaxHealth()
    {
        return HealthMax + GetBonusHealth() + get_stat_buff(HEALTH);
    }

    // "Bonus" refers to any stat increases not from boost-type attributes.
    public int GetBonusHealth()
    {
        int sum_hp = 0;
        // hp from non-boost attr?
        return sum_hp;
    }

    public int get_stat_buff(int type)
    {
        if (type != active_boost_type)
            return 0;
        return active_boost_amount;
    }

    public int get_bonus_from_equipment(int stat_ID)
    {
        if (!IsPlayer)
            return 0;
        Discipline d = TurnPhaser.I.GetDisc(OwnerID).Bat.Disc;
        return d.EquipmentInventory.get_stat_boost_amount(ID, stat_ID);
    }

    public int active_boost_type = -1;
    public int active_boost_amount = 0;
    protected void affect_boosted_stat(int boost_type, int amount)
    {
        active_boost_type = boost_type;
        active_boost_amount = amount;

        if (boost_type == HEALTH)
        {
            Health += amount;
        }
        else if (boost_type == ATTACK)
        {
            AttackDmg += amount;
        }
        else if (boost_type == DEFENSE)
        {
            Defense += amount;
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
            if (Slot != null)
                Slot.UpdateTextUI();
        }
    }

    /*
    This parent class version does boolean checks for aspects
    that apply to all player units.
    */
    public virtual bool can_activate_attribute()
    {
        if (PassiveAttribute)
            return false;
        return true;
    }

    public int get_raw_attack_dmg()
    {
        return AttackDmg;
    }

    public int get_raw_defense()
    {
        return Defense;
    }

    public string GetName()
    {
        return Name;
    }

    public Slot GetSlot()
    {
        return Slot;
    }

    public void SetSlot(Slot s)
    {
        Slot = s;
    }
}
