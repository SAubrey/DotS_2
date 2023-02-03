using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MapCell
{
    public const string Plains = "Plains";
    public const string Forest = "Forest";
    public const string Ruins = "Ruins";
    public const string Cliff = "Cliff";
    public const string Cave = "Cave";
    public const string Star = "Star";
    public const string Titrum = "Titrum";
    public const string LushLand = "Lush Land";
    public const string Mire = "Mire";
    public const string Mountain = "Mountain";
    public const string Settlement = "Settlement";
    public const string RuneGate = "Rune Gate";
    public const string City = "City";
    public const string GuardianPass = "Guardian Pass";

    public const int IDCity = -1;
    public const int IDPlains = 0;
    public const int IDForest = 1;
    public const int IDRuins = 2;
    public const int IDCliff = 3;
    public const int IDCave = 4;
    public const int IDStar = 5;
    public const int IDTitrum = 6;
    public const int IDLushLand = 7;

    // Not an actual tile type, just enemy spawn type as determined by travelcards
    public const int IDMeld = 8;
    public const int IDMountain = 9;
    public const int IDSettlement = 10;
    public const int IDRuneGate = 11;
    public const int IDGuardianPass = 12;

    public static MapCell CreateCell(int ID, int tier, Tile tile, Pos pos)
    {
        MapCell mc = null;
        // Don't extract name from tile if provided.
        if (ID == IDPlains)
        {
            mc = new Plains(tier, tile, pos);
        }
        else if (ID == IDForest)
        {
            mc = new Forest(tier, tile, pos);
        }
        else if (ID == IDRuins)
        {
            mc = new Ruins(tier, tile, pos);
        }
        else if (ID == IDCliff)
        {
            mc = new Cliff(tier, tile, pos);
        }
        else if (ID == IDCave)
        {
            mc = new Cave(tier, tile, pos);
        }
        else if (ID == IDStar)
        {
            mc = new Star(tier, tile, pos);
        }
        else if (ID == IDTitrum)
        {
            mc = new Titrum(tier, tile, pos);
        }
        else if (ID == IDLushLand)
        {
            mc = new LushLand(tier, tile, pos);
            //} else if (name == MIRE) {
            //mc = new Mire(tier, tile, pos);
        }
        else if (ID == IDMountain)
        {
            mc = new Mountain(tier, tile, pos);
        }
        else if (ID == IDSettlement)
        {
            mc = new Settlement(tier, tile, pos);
        }
        else if (ID == IDRuneGate)
        {
            mc = new RuneGate(tier, tile, pos);
        }
        else if (ID == IDCity)
        {
            mc = new CityCell(tier, tile, pos);
        }
        else if (ID == IDGuardianPass)
        {
            mc = new GuardianPass(tier, tile, pos);
        }
        else if (ID == IDCity)
        {
            mc = new CityCell(tier, tile, pos);
        }
        else
            mc = new MapCell(0, tier, tile, pos);
        //mc.tile_type_ID = tile_type_ID;
        return mc;
    }

    private static Color EnemyColor = new Color(1, .5f, .5f, 1f);
    public readonly Tile Tile;
    public readonly int Tier;
    public readonly Pos Pos;
    public readonly int ID;
    public bool Entered { get; private set; }
    public bool Discovered { get; private set; }
    public string Name;
    public int Minerals, StarCrystals = 0;
    public bool CreatesTravelcard = true;
    public bool HasRuneGate = false;
    public bool RestoredRuneGate = false;
    public bool HasActiveTeleport 
    {
        get { return RestoredRuneGate || TeleportDestination != null; }
    }
    public MapCell TeleportDestination;
    private bool _travelcardComplete = false;
    public bool TravelcardComplete
    {
        get { return _travelcardComplete; }
        set
        {
            _travelcardComplete = value;
            if (Travelcard != null)
            {
                Travelcard.complete = value;
            }
        }
    }
    public Battle Battle;
    public bool HasSeenCombat = false;
    public bool Locked = false;
    public bool Glows, Flickers = false;
    private List<Enemy> Enemies = new List<Enemy>();
    public GameObject Fog;
    // Travelcards cannot be set to null.  
    private TravelCard _travelcard;
    public TravelCard Travelcard
    {
        get => _travelcard;
        set
        {
            if (value == null)
                return;
            _travelcard = value;
            Locked = RequiresUnlock;
        }
    }

    public MapCell(int ID, int tier, Tile tile, Pos pos)
    {
        this.Tile = tile;
        this.Tier = tier;
        this.Pos = pos;
        this.ID = ID;
        Locked = RequiresUnlock;
        if (CreatesTravelcard)
        {
            Travelcard = TravelDeck.I.DrawCard(Tier, ID);
        }
    }

    public void Enter()
    {
        if (CreatesTravelcard && !TravelcardComplete)
        {
            MapUI.I.DisplayTravelcard(Travelcard);
        }
        if (!Entered)
        {
            Entered = true;
            Discover();
        }

        MapUI.I.TeleportB.enabled = !RestoredRuneGate;
        if (HasActiveTeleport)
        {
            // Show all other rune gates
            // Map.I.ActivateRuneGates();
            if (TeleportDestination != null)
            {
                MapUI.I.TeleportB.enabled = true;
                MapUI.I.TeleportT.text = "Teleport Back";
            }
        }
    }

    public void Discover()
    {
        if (Discovered)
            return;
        Discovered = true;
        Map.I.Tilemap.SetTile(new Vector3Int((int)Pos.x, (int)Pos.y, 0), Tile);
        //MapUI.I.PlaceCellLight(this);
        Tile.color = Color.white;
        GameObject.Destroy(Fog);
        if (ID == IDStar)
        {
            MapUI.I.PlaceSparklePS(this);
        }
    }

    public void PostBattle()
    {
        foreach (Enemy e in Enemies)
            e.GetSlot().UpdateTextUI();
    }

    public void KillEnemy(Enemy enemy)
    {
        Enemies.Remove(enemy);
        if (Enemies.Count == 0)
        {
            SetTileColor();
        }
    }

    public void SetTileColor()
    {
        if (Enemies.Count > 0)
        {
            Vector3Int vec = new Vector3Int(Pos.x, Pos.y, 0);
            Map.I.Tilemap.SetTileFlags(vec, TileFlags.None);
            Map.I.Tilemap.SetColor(vec, EnemyColor); // Dark red
        }
        else
        {
            Debug.Log("setting tile color to white");
            EndColorOscillation();
            Map.I.Tilemap.SetColor(new Vector3Int(Pos.x, Pos.y, 0), Color.white);
            //tile.color = Color.white;
        }
    }

    public void CompleteTravelcard()
    {
        TravelcardComplete = true;
    }

    public void AssignGroupLeader()
    {
        Battle = new Battle(Map.I, this, TurnPhaser.I.ActiveDisc, true);
        BeginColorOscillation();
    }

    public void ClearBattle()
    {
        Battle = null;
        EndColorOscillation();
        SetTileColor();
    }

    private void BeginColorOscillation()
    {
        Map.I.AddOscillatingCell(this);
    }

    private void EndColorOscillation()
    {
        if (Map.I.RemoveOscillatingCell(this))
            Map.I.Tilemap.SetColor(new Vector3Int(Pos.x, Pos.y, 0), Color.white);
    }

    public void OscillateColor()
    {
        float y = 0.75f + (Mathf.Sin(Time.time) / 4f);
        Tile t = (Tile)Map.I.Tilemap.GetTile(new Vector3Int(Pos.x, Pos.y, 0));
        Map.I.Tilemap.SetColor(new Vector3Int(Pos.x, Pos.y, 0), new Color(1, y, y, 1));
    }

    // Can currently only group battle if the player has retreated/scouted the tile
    // and a group has not already been formed on this cell.
    public bool CanSetupGroupBattle()
    {
        return HasEnemies && Map.CheckAdjacentCells(TurnPhaser.I.ActiveDisc.Position, Pos.toVec3);
    }

    public void AddEnemy(Enemy e)
    {
        if (e != null)
            Enemies.Add(e);
        SetTileColor();
    }

    public List<Enemy> GetEnemies()
    {
        return Enemies;
    }

    public bool HasEnemies
    {
        get { return (GetEnemies().Count > 0); }
    }

    public bool RequiresUnlock
    {
        get
        {
            if (HasRuneGate && !RestoredRuneGate)
            {
                return true;
            }
            else if (HasTravelcard)
            {
                if (Travelcard.Unlockable != null)
                    return true;
            }
            return false;
        }
    }

    public TravelCardUnlockable GetUnlockable()
    {
        return Travelcard.Unlockable;
    }

    public int GetUnlockCost()
    {
        if (HasRuneGate)
            return 10;
        else if (Travelcard.Unlockable != null)
            return Travelcard.Unlockable.ResourceCost;
        return 0;
    }

    public string GetUnlockType()
    {
        if (HasRuneGate)
            return Storeable.STAR_CRYSTALS;
        else
            return Travelcard.Unlockable.ResourceType;
    }

    public bool HasTravelcard { get => Travelcard != null; }
    public bool HasBattle { get => Battle != null; }
    public bool HasGroupPending
    {
        get
        {
            if (!HasBattle)
                return false;
            return Battle.GroupPending;
        }
    }

    public Dictionary<string, int> get_travelcard_consequence()
    {
        return Travelcard.Consequence;
    }

    public bool CanMine(Battalion b)
    {
        return b.Disc.CountMiner > 0 && !b.Disc.HasMinedInTurn &&
            b.Disc.Cell == this &&
            (Minerals > 0 || StarCrystals > 0);
    }
}

public class CityCell : MapCell
{
    public CityCell(int tier, Tile tile, Pos pos) : base(IDCity, tier, tile, pos)
    {
        Name = "City";
        CreatesTravelcard = false;
        //pos = new
    }
}

public class Plains : MapCell
{
    public Plains(int tier, Tile tile, Pos pos) : base(IDPlains, tier, tile, pos)
    {
        Name = Plains;
    }
}

public class Forest : MapCell
{
    public Forest(int tier, Tile tile, Pos pos) : base(IDForest, tier, tile, pos)
    {
        Name = Forest;
    }
}

public class Ruins : MapCell
{
    public Ruins(int tier, Tile tile, Pos pos) : base(IDRuins, tier, tile, pos)
    {
        Name = Ruins;
    }
}

public class Cliff : MapCell
{
    public Cliff(int tier, Tile tile, Pos pos) : base(IDCliff, tier, tile, pos)
    {
        Name = Cliff;
    }
}

public class Cave : MapCell
{
    public Cave(int tier, Tile tile, Pos pos) : base(IDCave, tier, tile, pos)
    {
        Name = Cave;
    }
}

public class Star : MapCell
{
    public Star(int tier, Tile tile, Pos pos) : base(IDStar, tier, tile, pos)
    {
        Name = Star;
        StarCrystals = 18;
        CreatesTravelcard = false;
        TravelcardComplete = true;
        Glows = true;
    }
}

public class Titrum : MapCell
{
    public Titrum(int tier, Tile tile, Pos pos) : base(IDTitrum, tier, tile, pos)
    {
        Name = Titrum;
        Minerals = 24;
        Glows = true;
    }
}
public class LushLand : MapCell
{
    public LushLand(int tier, Tile tile, Pos pos) : base(IDLushLand, tier, tile, pos)
    {
        Name = LushLand;
        CreatesTravelcard = false;
        TravelcardComplete = true;
    }
}
/*
public class Mire : MapCell {
    public Mire(int tier, Tile tile, Pos pos) : base(tier, tile, pos, MIRE_ID) {
        name = MIRE;
    }
}*/
public class Mountain : MapCell
{
    public Mountain(int tier, Tile tile, Pos pos) : base(IDMountain, tier, tile, pos)
    {
        Name = Mountain;
        Minerals = 21;
    }
}
public class Settlement : MapCell
{
    public Settlement(int tier, Tile tile, Pos pos) : base(IDSettlement, tier, tile, pos)
    {
        Name = Settlement;
        CreatesTravelcard = false;
    }
}
public class RuneGate : MapCell
{
    public RuneGate(int tier, Tile tile, Pos pos) : base(IDRuneGate, tier, tile, pos)
    {
        Name = RuneGate;
        HasRuneGate = true;
        Glows = true;
    }
}

public class GuardianPass : MapCell
{
    public GuardianPass(int tier, Tile tile, Pos pos) : base(IDGuardianPass, tier, tile, pos)
    {
        Name = GuardianPass;
    }
}
