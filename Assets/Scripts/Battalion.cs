using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Battalion
{
    public IDictionary<int, List<PlayerUnit>> Units =
        new Dictionary<int, List<PlayerUnit>>() { };
    public bool InBattle = false;
    private Discipline _disc;
    public Discipline Disc { get => _disc; private set => _disc = value; }
    public MapCell PendingGroupBattleCell;

    public Battalion(Discipline disc)
    {
        this.Disc = disc;

        // Initialize units dictionary.
        foreach (int unitType in PlayerUnit.UnitTypes)
            Units.Add(unitType, new List<PlayerUnit>());

        AddDefaultTroops();
        if (disc.ID == 0)
        {
            AddUnits(PlayerUnit.SEEKER, 1, true);
        }
        // Units for testing
        //add_units(PlayerUnit.MENDER, 1);
        //add_units(PlayerUnit.SCOUT, 1);
    }

    public void AddDefaultTroops()
    {
        AddUnits(PlayerUnit.ARCHER, 1);
        AddUnits(PlayerUnit.WARRIOR, 2);
        AddUnits(PlayerUnit.SPEARMAN, 2);
        AddUnits(PlayerUnit.INSPIRATOR, 1);
        AddUnits(PlayerUnit.MINER, 1);
    }

    public void AddUnits(int type, int count, bool show = false)
    {
        for (int i = 0; i < count; i++)
        {
            if (Units.ContainsKey(type))
            {
                Units[type].Add(PlayerUnit.CreatePunit(type, Disc.ID));
            }
        }
        if (Disc.Active)
        {
            Disc.TriggerUnitCountChange();
        }
        if (show)
        {
            Disc.ShowAdjustment(PlayerUnit.CreatePunit(type, -1).GetName(), count);
        }
    }

    public void LoseRandomUnit(string message = "")
    {
        // Get indices with available units.
        ArrayList spawned_unit_types = new ArrayList();
        for (int i = 0; i < Units.Count; i++)
        {
            if (Units[i].Count > 0)
            {
                spawned_unit_types.Add(i);
            }
        }
        if (spawned_unit_types.Count <= 0)
            return; // No more units to remove.

        int roll = UnityEngine.Random.Range(0, spawned_unit_types.Count - 1);
        int removed_unit_type_ID = (int)spawned_unit_types[roll];

        string s = Units[removed_unit_type_ID][0].GetName() + " " + message;
        Units[removed_unit_type_ID].RemoveAt(0);
        Disc.TriggerUnitCountChange();
        Disc.ShowAdjustment(s, -1);
    }

    public PlayerUnit GetUnit(int ID)
    {
        List<PlayerUnit> punits;
        Units.TryGetValue(ID, out punits);
        if (punits == null)
            return null;

        for (int i = 0; i < punits.Count; i++)
        {
            if (punits[i] != null)
                return punits[i];
        }
        return null;
    }

    public PlayerUnit GetPlaceableUnit(int ID)
    {
        List<PlayerUnit> punits;
        Units.TryGetValue(ID, out punits);
        if (punits == null)
            return null;

        for (int i = 0; i < punits.Count; i++)
        {
            PlayerUnit pu = punits[i];
            if (pu != null && !pu.IsPlaced)
                return pu;
        }
        return null;
    }

    public int CountUnits(int type = -1)
    {
        int i = 0;
        if (type >= 0)
        {
            i += Units[type].Count;
        }
        else
        { // Count all units.
            for (int t = 0; t < Units.Count; t++)
            {
                i += Units[t].Count;
            }
        }
        return i;
    }

    public void RemoveDeadUnit(PlayerUnit du)
    {
        Units[du.GetID()].Remove(du);
    }

    public void PostBattle()
    {
        if (CountUnits() <= 0)
        {
            Disc.Die();
            return;
        }

        foreach (PlayerUnit pu in GetAllPlacedUnits())
        {
            pu.Health = pu.GetDynamicMaxHealth();
            pu.GetSlot().UpdateTextUI();
        }
    }

    public bool HasMiner { get => GetUnit(PlayerUnit.MINER) != null; }

    public bool HasSeeker { get => GetUnit(PlayerUnit.SEEKER) != null; }

    public bool HasScout { get => GetUnit(PlayerUnit.SCOUT) != null; }


    public List<PlayerUnit> GetAllPlacedUnits()
    {
        List<PlayerUnit> punits = new List<PlayerUnit>();
        foreach (List<PlayerUnit> type_list in Units.Values)
        {
            foreach (PlayerUnit pu in type_list)
            {
                if (pu.IsPlaced)
                    punits.Add(pu);
            }
        }
        return punits;
    }
}
