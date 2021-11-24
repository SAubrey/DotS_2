using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class Storeable : MonoBehaviour, ISaveLoad
{
    public const string LIGHT = "light";
    public const string UNITY = "unity";
    public const string STAR_CRYSTALS = "star crystals";
    public const string MINERALS = "minerals";
    public const string ARELICS = "Astra relics";
    public const string MRELICS = "Martial relics";
    public const string ERELICS = "Endura relics";
    public const string EQUIMARES = "equimares";

    public static string[] Fields = { LIGHT, UNITY, STAR_CRYSTALS,
                            MINERALS, ARELICS, MRELICS, ERELICS, EQUIMARES };
    public Dictionary<string, int> Resources = new Dictionary<string, int>() {
        {LIGHT, 4},
        {UNITY, 10},
        {STAR_CRYSTALS, 0},
        {MINERALS, 0},
        {ARELICS, 0},
        {MRELICS, 0},
        {ERELICS, 0},
        {EQUIMARES, 0},
    };
    public GameObject rising_info_prefab;
    public GameObject origin_of_rise_obj;
    private int _ID;
    public int ID {
        get { return _ID; }
        set { 
            _ID = value;
            if (ID == Discipline.Astra)
                Name = "Astra";
            else if (ID == Discipline.Endura)
                Name = "Endura";
            else if (ID == Discipline.Martial)
                Name = "Martial";
            else
                Name = "City";
        }
    }
    public string Name;
    public bool Initialized { get; protected set; } = false;
    public const int InitialCapacity = 72;
    private int _capacity = InitialCapacity;
    public int Capacity
    {
        get { return _capacity; }
        set
        {
            _capacity = value;
            if (OnCapacityChange != null)
                OnCapacityChange(ID, GetSumStoreableResources(), value);
        }
    }
    public const int InitialLightRefreshAmount = 4;
    public int LightRefreshAmount = InitialLightRefreshAmount;

    public event Action<int, string, int, int, int> OnResourceChange; // Disc ID, res type, new amount
    public event Action<int, int, int> OnCapacityChange;
    protected List<Adjustment> Adjustments = new List<Adjustment>();
    Timer ShowAdjTimer = new Timer(.5f);

    protected virtual void Start()
    {
        TurnPhaser.I.OnTurnChange += RegisterTurn;
    }

    void Update()
    {
        /* Display adjusted resources in a timely sequence. */
        if (Adjustments.Count == 0 || !Initialized)
        {
            return;
        }

        if (ShowAdjTimer.Increase(Time.deltaTime))
        {
            Adjustment a = Adjustments[0];
            Adjustments.Remove(a);
            if (Resources.ContainsKey(a.resource))
            {
                AdjustResource(a.resource, a.amount, true);
            }
            else
            {
                RisingInfo.create_rising_info_map(
                    RisingInfo.build_resource_text(a.resource, a.amount),
                    Statics.disc_colors[ID],
                    origin_of_rise_obj.transform,
                    rising_info_prefab);
            }
            UIPlayer.I.play(UIPlayer.INV_IN);
            // Allow next adjustment to happen instantly.
            if (Adjustments.Count == 0)
            {
                ShowAdjTimer.Reset();
            }
        }
    }

    public virtual GameData Save() { return null; }
    public virtual void Load(GameData generic) { }

    public virtual void Init(bool fromSave)
    {
        Capacity = InitialCapacity;
        LightRefreshAmount = InitialLightRefreshAmount;
    }

    public virtual void RegisterTurn()
    {
        LightDecayCascade();
    }

    public virtual void LightDecayCascade()
    {
        Dictionary<string, int> d = new Dictionary<string, int>();
        d.Add(LIGHT, -1);
        if (Resources[LIGHT] <= 0)
        {
            if (Resources[STAR_CRYSTALS] > 0)
            {
                d.Add(STAR_CRYSTALS, -1);
                d[LIGHT] = LightRefreshAmount;
            }
            else
            {
                if (Resources[UNITY] >= 2)
                    d.Add(UNITY, -2);
                else if (Resources[UNITY] == 1) // Can't have negative Unity.
                    d.Add(UNITY, -1);
            }
        }
        ShowAdjustments(d);
    }

    public void ShowAdjustments(Dictionary<string, int> adjs)
    {
        foreach (KeyValuePair<string, int> a in adjs)
        {
            if (a.Value != 0)
            {
                Adjustments.Add(new Adjustment(a.Key, a.Value));
            }
        }
    }

    public void ShowAdjustment(string var, int val)
    {
        Dictionary<string, int> d = new Dictionary<string, int>();
        d.Add(var, val);
        ShowAdjustments(d);
    }

    protected void RemoveResourcesLostOnDeath()
    {
        Dictionary<string, int> adjs = new Dictionary<string, int>();
        adjs.Add(STAR_CRYSTALS, -Resources[STAR_CRYSTALS]);
        adjs.Add(MINERALS, -Resources[MINERALS]);
        adjs.Add(ARELICS, -Resources[ARELICS]);
        adjs.Add(MRELICS, -Resources[MRELICS]);
        adjs.Add(ERELICS, -Resources[ERELICS]);
        adjs.Add(EQUIMARES, -Resources[EQUIMARES]);

        Resources[LIGHT] = 4;
        Resources[UNITY] = 10;
        ShowAdjustments(adjs);
    }

    // Use != 0 with result to use as boolean.
    public int GetValidChangeAmount(string type, int change)
    {
        // Return change without going lower than 0.
        //Debug.Log(type + " " + get_res(type) + " + " + change);
        int amount = Statics.CalcValidNonnegativeChange(GetResource(type), change);

        //return change - (get_var(type) - change);
        // Return change without going higher than cap.
        if (GetSumStoreableResources() + change > Capacity)
            return Capacity - GetSumStoreableResources();
        return amount;
    }

    public int GetSumStoreableResources()
    {
        int sum = 0;
        foreach (string res in Resources.Keys)
        {
            if (res == Discipline.Experience ||
                res == Storeable.UNITY ||
                res == Storeable.LIGHT)
                continue;
            sum += Resources[res];
        }
        return sum;
    }

    public int AdjustResource(string var, int val, bool show = false)
    {
        val = GetValidChangeAmount(var, val);
        Resources[var] += val;

        Debug.Log(ID + " " + var + " " + GetResource(var) + " " +
            GetSumStoreableResources() + " " + Capacity);
        if (OnResourceChange != null)
        {
            OnResourceChange(ID, var, GetResource(var),
                GetSumStoreableResources(), Capacity);
        }
        if (show && val != 0)
        {
            RisingInfo.create_rising_info_map(
                RisingInfo.build_resource_text(var, val),
                Statics.disc_colors[ID],
                origin_of_rise_obj.transform,
                rising_info_prefab);
        }
        return val;
    }

    public void SetResource(string res, int amount)
    {
        if (!Resources.ContainsKey(res))
            return;
        Resources[res] = amount;
    }

    public int GetResource(string var)
    {
        int r;
        if (Resources.TryGetValue(var, out r))
        {
            return r;
        }
        Debug.Log("RESOURCE NOT FOUND IN DICTIONARY");
        return 0;
    }
}

public class Adjustment
{
    public string resource;
    public int amount;
    public Adjustment(string resource, int amount)
    {
        this.resource = resource;
        this.amount = amount;
    }
}
