using UnityEngine;

public class Enemy : Unit
{
    public const int COMMON = 0, UNCOMMON = 1, RARE = 2;

    // Enemy names
    public const int GALTSA = 0, GREM = 1, ENDU = 2,
        KOROTE = 3, MOLNER = 4, ETUENA = 5,
        CLYPTE = 6, GOLIATH = 7, KVERM = 8,
        LATU = 9, EKE_TU = 10, OETEM = 11,
        EKE_FU = 12, EKE_SHI_AMI = 13, EKE_LORD = 14,
        KETEMCOL = 15, MAHUKIN = 16, DRONGO = 17,
        MAHEKET = 18, CALUTE = 19, ETALKET = 20,
        MUATEM = 21, DRAK = 22, ZERRKU = 23,
        GOKIN = 24, TAJAQAR = 25, TAJAERO = 26,
        TERRA_QUAL = 27, DUALE = 28,
        MELD_WARRIOR = 29, MELD_SPEARMAN = 30,
        T1_GUARDIAN = 31, T2_GUARDIAN = 32;

    public int xp;
    public bool xp_taken = false;

    // player unit to be moved towards and attacked
    private Slot _target;
    public Slot target
    {
        get
        {
            if (_target != null)
            {
                if (_target.HasPunit)
                    return _target;
            }
            else
                _target = null;
            return null;
        }
        set { _target = value; }
    }


    public Enemy(string name, int ID, int att, int def, int hp, int xp, Style style,
            Attributes atr1=Attributes.Null, Attributes atr2=Attributes.Null, Attributes atr3=Attributes.Null) :
            base(name, ID, att, def, hp, style, atr1, atr2, atr3)
    {
        IsPlayer = false;
        OwnerID = -1;
        this.xp = xp;
    }

    public static Enemy CreateEnemy(int ID)
    {
        Enemy e = null;
        if (ID == GALTSA) e = new Galtsa();
        else if (ID == GREM) e = new Grem();
        else if (ID == ENDU) e = new Endu();
        else if (ID == KOROTE) e = new Korote();
        else if (ID == MOLNER) e = new Molner();
        else if (ID == ETUENA) e = new Etuena();
        else if (ID == CLYPTE) e = new Clypte();
        else if (ID == GOLIATH) e = new Goliath();
        else if (ID == KVERM) e = new Kverm();
        else if (ID == LATU) e = new Latu();
        else if (ID == EKE_TU) e = new Eke_tu();
        else if (ID == OETEM) e = new Oetem();
        else if (ID == EKE_FU) e = new Eke_fu();
        else if (ID == EKE_SHI_AMI) e = new Eke_shi_ami();
        else if (ID == EKE_LORD) e = new Eke_Lord();
        else if (ID == KETEMCOL) e = new Ketemcol();
        else if (ID == MAHUKIN) e = new Mahukin();
        else if (ID == DRONGO) e = new Drongo();
        else if (ID == MAHEKET) e = new Maheket();
        else if (ID == CALUTE) e = new Calute();
        else if (ID == ETALKET) e = new Etalket();
        else if (ID == MUATEM) e = new Muatem();
        else if (ID == DRAK) e = new Drak();
        else if (ID == ZERRKU) e = new Zerrku();
        else if (ID == GOKIN) e = new Gokin();
        else if (ID == TAJAQAR) e = new Tajaqar();
        else if (ID == TAJAERO) e = new Tajaero();
        else if (ID == TERRA_QUAL) e = new Terra_Qual();
        else if (ID == DUALE) e = new Duale();
        else if (ID == MELD_SPEARMAN) e = new Meld_Spearman();
        else if (ID == MELD_WARRIOR) e = new Meld_Warrior();
        return e;
    }

    public bool CanTarget(Slot slot)
    {
        if (!slot.HasPunit)
            return false;

        bool meleeVsFlying = IsMelee &&
                    slot.Unit.HasAttribute(Unit.Attributes.Flying);
        return !meleeVsFlying;
    }

    protected override string GetAttackAnimationID()
    {
        if (CombatStyle == Style.Range)
        {
            return AnimationPlayer.ARROW_FIRE;
        }
        else
        {
            return AnimationPlayer.SLASH;
        }
    }

    public override void Die()
    {
        Map.I.GetCurrentCell().KillEnemy(this);
        Slot.Empty();
        EnemyLoader.I.Enemies.Remove(this);
    }

    public int take_xp_from_death()
    {
        if (!xp_taken)
        {
            xp_taken = true;
            return xp;
        }
        return 0;
    }
}

// Plains
public class Galtsa : Enemy
{
    public Galtsa() : base("Galtsa", GALTSA, 20, 0, 100, 2, Style.Sword, Attributes.Charge)
    {
    }
}
public class Grem : Enemy
{
    public Grem() : base("Grem", GREM, 10, 0, 100, 1, Style.Sword)
    {
    }
}
public class Endu : Enemy
{
    public Endu() : base("Endu", ENDU, 40, 0, 100, 3, Style.Sword, Attributes.Charge)
    {
    }
}
public class Korote : Enemy
{
    public Korote() : base("Korote", KOROTE, 10, 0, 100, 2, Style.Sword, Attributes.Flanking)
    {
    }
}
public class Molner : Enemy
{
    public Molner() : base("Molner", MOLNER, 20, 0, 100, 2, Style.Sword, Attributes.Flanking, Attributes.Charge)
    {
    }
}
public class Etuena : Enemy
{
    public Etuena() : base("Etuena", ETUENA, 20, 0, 100, 3, Style.Sword, Attributes.Flying, Attributes.Charge)
    {
    }
}
public class Clypte : Enemy
{
    public Clypte() : base("Clypte", CLYPTE, 30, 0, 100, 3, Style.Range)
    {
    }
}
public class Goliath : Enemy
{
    public Goliath() : base("Goliath", GOLIATH, 100, 0, 250, 12, Style.Sword, Attributes.Charge)
    {
    }
}

// Forest
public class Kverm : Enemy
{
    public Kverm() : base("Kverm", KVERM, 20, 0, 100, 1, Style.Sword, Attributes.Stalk)
    {
    }
}
public class Latu : Enemy
{
    public Latu() : base("Latu", LATU, 30, 0, 100, 3, Style.Sword, Attributes.Stalk, Attributes.Aggressive)
    {
    }
}
public class Eke_tu : Enemy
{
    public Eke_tu() : base("Eke Tu", EKE_TU, 10, 0, 100, 2, Style.Sword, Attributes.Aggressive)
    {
    }
}
public class Oetem : Enemy
{
    public Oetem() : base("Oetem", OETEM, 40, 0, 100, 3, Style.Sword)
    {
    }
}
public class Eke_fu : Enemy
{
    public Eke_fu() : base("Eke Fu", EKE_FU, 30, 0, 100, 2, Style.Sword, Attributes.Flanking)
    {
    }
}
public class Eke_shi_ami : Enemy
{
    public Eke_shi_ami() : base("Eke Shi Ami", EKE_SHI_AMI, 30, 0, 100, 4, Style.Range, Attributes.Piercing, Attributes.Stun)
    {
    }
}
public class Eke_Lord : Enemy
{
    public Eke_Lord() : base("Eke Lord", EKE_LORD, 6, 0, 1100, 12, Style.Sword, Attributes.ArcingStrike, Attributes.Stun)
    {
    }
}
public class Ketemcol : Enemy
{
    public Ketemcol() : base("Ketemcol", KETEMCOL, 2, 1, 100, 6, Style.Sword, Attributes.ArcingStrike, Attributes.Stun)
    {
    }
}

// Titrum
public class Mahukin : Enemy
{
    public Mahukin() : base("Mahukin", MAHUKIN, 2, 2, 100, 4, Style.Sword)
    {
    }
}
public class Drongo : Enemy
{
    public Drongo() : base("Drongo", DRONGO, 3, 3, 100, 6, Style.Sword)
    {
    }
}
public class Maheket : Enemy
{
    public Maheket() : base("Maheket", MAHEKET, 3, 2, 100, 5, Style.Sword)
    {
    }
}
public class Calute : Enemy
{
    public Calute() : base("Calute", CALUTE, 6, 0, 100, 5, Style.Sword, Attributes.Stalk, Attributes.Aggressive)
    {
    }
}
public class Etalket : Enemy
{
    public Etalket() : base("Etalket", ETALKET, 2, 0, 100, 4, Style.Sword, Attributes.Stalk)
    {
    }
}
public class Muatem : Enemy
{
    public Muatem() : base("Muatem", MUATEM, 7, 5, 100, 12, Style.Sword)
    {
    }
}

// Mountain/Cliff
public class Drak : Enemy
{
    public Drak() : base("Drak", DRAK, 3, 0, 100, 3, Style.Sword, Attributes.Flying)
    {
    }
}
public class Zerrku : Enemy
{
    public Zerrku() : base("Zerrku", ZERRKU, 3, 0, 100, 4, Style.Range)
    {
    }
}
public class Gokin : Enemy
{
    public Gokin() : base("Gokin", GOKIN, 2, 0, 100, 2, Style.Sword, Attributes.Flanking)
    {
    }
}

// Cave
public class Tajaqar : Enemy
{
    public Tajaqar() : base("Tajaqar", TAJAQAR, 3, 1, 100, 5, Style.Sword, Attributes.Flanking)
    {
    }
}
public class Tajaero : Enemy
{
    public Tajaero() : base("Tajaero", TAJAERO, 3, 0, 100, 4, Style.Range, Attributes.Flying)
    {
    }
}
public class Terra_Qual : Enemy
{
    public Terra_Qual() : base("Terra Qual", TERRA_QUAL, 5, 2, 1100, 12, Style.Sword, Attributes.ArcingStrike)
    {
    }
}
public class Duale : Enemy
{
    public Duale() : base("Duale", DUALE, 2, 0, 100, 5, Style.Range, Attributes.Aggressive, Attributes.Flanking)
    {
    }
}

public class Meld_Warrior : Enemy
{
    public Meld_Warrior() : base("Meld Warrior", MELD_WARRIOR, 1, 1, 100, 3, Style.Sword, Attributes.Charge)
    {
    }
}

public class Meld_Spearman : Enemy
{
    public Meld_Spearman() : base("Meld Spearman", MELD_SPEARMAN, 1, 2, 100, 3, Style.Sword, Attributes.Charge)
    {
    }
}

public class t1_Guardian : Enemy
{
    public t1_Guardian() : base("Guardian", T1_GUARDIAN, 100, 0, 1000, 30, Style.Sword)
    {
        // 5 terror
        // swipes front and left tile, hits front and behind
    }
}

public class t2_Guardian : Enemy
{
    public t2_Guardian() : base("Deep Guardian", T2_GUARDIAN, 130, 5, 4000, 60, Style.Range)
    {

    }
}