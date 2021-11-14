using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public string name;
}

interface ISaveLoad
{
    GameData Save();
    void Load(GameData generic);
}

[System.Serializable]
public class TurnPhaserData : GameData
{
    public int turn;
    public int activeDiscID;
}

[System.Serializable]
public class MapData : GameData
{
    public List<int> t1_bag = new List<int>();
    public List<int> t2_bag = new List<int>();
    public List<int> t3_bag = new List<int>();
    public List<SMapCell> cells = new List<SMapCell>();
    public MapData(Map map, string name)
    {
        this.name = name;

        foreach (int num in map.Bags[1])
            t1_bag.Add(num);
        foreach (int num in map.Bags[2])
            t2_bag.Add(num);
        foreach (int num in map.Bags[3])
            t3_bag.Add(num);

        foreach (MapCell mc in map.MapCells.Values)
        {
            SMapCell mcs =
                new SMapCell(mc.ID,
                        mc.Pos.x, mc.Pos.y,
                        mc.Tier, mc.Discovered,
                        mc.Minerals, mc.StarCrystals);
            cells.Add(mcs);
        }
    }
}

[System.Serializable]
public struct SMapCell
{
    public int ID;
    public int x, y;
    public int tier;
    public bool discovered;
    public int minerals, star_crystals;
    public SMapCell(int ID, int x, int y,
            int tier, bool discovered,
            int minerals, int star_crystals)
    {
        this.ID = ID;
        this.x = x;
        this.y = y;
        this.tier = tier;
        this.discovered = discovered;
        this.minerals = minerals;
        this.star_crystals = star_crystals;
    }
}

[System.Serializable]
public class DisciplineData : GameData
{
    public SBattalion sbat;
    public SStoreableResources sresources;
    public float col, row;
    public int redrawn_travel_card_ID;

    public DisciplineData(Discipline disc, string name)
    {
        this.name = name;
        col = disc.Position.x;
        row = disc.Position.y;
        if (disc.Bat.InBattle)
            redrawn_travel_card_ID = disc.GetTravelcard().ID;
        sbat = new SBattalion(disc.Bat);
        sresources = new SStoreableResources(disc);
    }
}

[System.Serializable]
public struct SStoreableResources
{
    public int light, unity, star_crystals, minerals, arelics, erelics, mrelics;
    public SStoreableResources(Storeable s)
    {
        light = s.GetResource(Storeable.LIGHT);
        unity = s.GetResource(Storeable.UNITY);
        star_crystals = s.GetResource(Storeable.STAR_CRYSTALS);
        minerals = s.GetResource(Storeable.MINERALS);
        arelics = s.GetResource(Storeable.ARELICS);
        erelics = s.GetResource(Storeable.ERELICS);
        mrelics = s.GetResource(Storeable.MRELICS);
    }
}

[System.Serializable]
public struct SBattalion
{
    // Indices refer to the unit type, values refer to the amount.
    public List<int> unit_types;
    public SBattalion(Battalion bat)
    {
        unit_types = PlayerUnit.UnitTypes;
    }
}

[System.Serializable]
public class CityData : GameData
{
    public SStoreableResources sresources;
    public bool[] purchases;

    public CityData(City city, string name)
    {
        this.name = name;
        sresources = new SStoreableResources(city);

        CityUI cui = CityUI.I;
        purchases = new bool[cui.upgrades.Count];
        for (int i = 0; i < cui.upgrades.Count; i++)
        {
            purchases[i] = cui.upgrades[i].purchased;
        }
    }
}