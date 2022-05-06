using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour, ISaveLoad
{
    public static Map I { get; private set; }

    public Tilemap Tilemap;
    public Tile Plains1, Plains2;
    public Tile Forest1, Forest2;
    public Tile Ruins1, Ruins2;
    public Tile Cliff1, Cliff2;
    public Tile Cave1, Cave2;
    public Tile Star1, Star2;
    public Tile Titrum1, Titrum2;
    public Tile LushLand2;
    public Tile Mountain2;
    public Tile Settlement;
    public Tile RuneGate;
    public Tile City, Shadow;
    public CityCell CityCell;

    // <MapCell Cell ID, List of Tile image per tier>
    public Dictionary<int, List<Tile>> Tiles = new Dictionary<int, List<Tile>>();

    // <Tier, List<Cell ID>> Filled and then removed from as cells are drawn.
    public Dictionary<int, List<int>> Bags = new Dictionary<int, List<int>>();
    // <Tier, Dictionary<Cell ID, Count>
    private readonly Dictionary<int, Dictionary<int, int>> BagCounters
     = new Dictionary<int, Dictionary<int, int>>();
    private readonly Dictionary<int, int> T1BagCount = new Dictionary<int, int>() {
        {MapCell.IDPlains, 6},
        {MapCell.IDForest, 6},
        {MapCell.IDRuins, 4},
        {MapCell.IDCliff, 1},
        {MapCell.IDCave, 1},
        {MapCell.IDStar, 4},
        {MapCell.IDTitrum, 2},
    };
    private readonly Dictionary<int, int> T2BagCount = new Dictionary<int, int>() { // 96 + 4 * 4
        {MapCell.IDPlains, 42}, //32
        {MapCell.IDForest, 22},
        {MapCell.IDRuins, 12},
        {MapCell.IDCliff, 2},
        {MapCell.IDCave, 6},
        {MapCell.IDStar, 7},
        {MapCell.IDTitrum, 8},
        {MapCell.IDSettlement, 2},
        {MapCell.IDLushLand, 5},
        {MapCell.IDMountain, 14},
        {MapCell.IDRuneGate, 2},
    };
    private readonly Dictionary<int, int> T3BagCount = new Dictionary<int, int>() { // 212 + 8 x 4
        {MapCell.IDPlains, 82},
        {MapCell.IDForest, 54},
        {MapCell.IDRuins, 24},
        {MapCell.IDCliff, 4},
        {MapCell.IDCave, 12},
        {MapCell.IDStar, 14},
        {MapCell.IDTitrum, 16},
        {MapCell.IDSettlement, 4},
        {MapCell.IDLushLand, 10},
        {MapCell.IDMountain, 28},
        {MapCell.IDRuneGate, 4},
    };

    public Dictionary<Pos, MapCell> MapCells = new Dictionary<Pos, MapCell>();
    public bool Scouting { get; set; } = false;
    public List<MapCell> OscillatingCells = new List<MapCell>();
    private System.Random Rand;
    private Color HalfOpacityWhite = new Color(1f, 1f, 1f, .85f);

    void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Tilemap = GameObject.Find("MapTilemap").GetComponent<Tilemap>();
        Rand = new System.Random();

        Bags.Add(1, new List<int>());
        Bags.Add(2, new List<int>());
        Bags.Add(3, new List<int>());

        BagCounters.Add(1, T1BagCount);
        BagCounters.Add(2, T2BagCount);
        BagCounters.Add(3, T3BagCount);

        Tiles.Add(MapCell.IDCity, new List<Tile> { City, City, City });
        Tiles.Add(MapCell.IDPlains, new List<Tile> { Plains1, Plains2, Plains2 });
        Tiles.Add(MapCell.IDForest, new List<Tile> { Forest1, Forest2, Forest2 });
        Tiles.Add(MapCell.IDRuins, new List<Tile> { Ruins1, Ruins2, Ruins2 });
        Tiles.Add(MapCell.IDCliff, new List<Tile> { Cliff1, Cliff2, Cliff2 });
        Tiles.Add(MapCell.IDCave, new List<Tile> { Cave1, Cave2, Cave2 });
        Tiles.Add(MapCell.IDStar, new List<Tile> { Star1, Star2, Star2 });
        Tiles.Add(MapCell.IDTitrum, new List<Tile> { Titrum1, Titrum2, Titrum2 });
        Tiles.Add(MapCell.IDSettlement, new List<Tile> { Settlement, Settlement, Settlement });
        Tiles.Add(MapCell.IDRuneGate, new List<Tile> { RuneGate, RuneGate, RuneGate });
        Tiles.Add(MapCell.IDMountain, new List<Tile> { Mountain2, Mountain2, Mountain2 });
        Tiles.Add(MapCell.IDLushLand, new List<Tile> { LushLand2, LushLand2, LushLand2 });
        Tiles.Add(MapCell.IDGuardianPass, new List<Tile> { Mountain2, Mountain2, Mountain2 });

        CreateCity();
    }

    public void Init(bool fromSave)
    {
        if (!fromSave)
        {
            NewGame();
        }
    }

    private void NewGame()
    {
        ClearData();
        Scouting = false;
        FillBags();

        // Recreate city only if game has loaded the first time.
        if (Game.I.GameHasBegun)
        {
            CreateCity();
        }
        else
        {
            MapCells.Add(CityCell.Pos, CityCell);
        }
        GenerateT1(Tilemap);
        GenerateT2(Tilemap);
        GenerateT3(Tilemap);
    }

    private void FillBags()
    {
        for (int tier = 1; tier <= 3; tier++)
        {
            foreach (int cellID in BagCounters[tier].Keys)
            {
                for (int i = 0; i < BagCounters[tier][cellID]; i++)
                {
                    Bags[tier].Add(cellID);
                }
            }
        }
    }

    /*
    Pick a random cell type from the grab bag if one is not provided, 
    generate and display it.
    */
    private void CreateCell(int tier, int x, int y, int cellID = -1)
    {
        Pos pos = new Pos(x, y);
        if (cellID == -1)
        {
            cellID = GrabCell(tier);
        }
        Tile tile = Tiles[cellID][tier - 1];
        MapCell cell = MapCell.CreateCell(cellID, tier, tile, pos);
        MapCells.Add(pos, cell);
        
        DisplayCell(cell);
    }

    // Randomly pick tiles from grab bags. 
    private int GrabCell(int tier)
    {
        if (Bags[tier].Count <= 0)
            throw new System.Exception("Out of cells to draw for this tier.");

        int index = Rand.Next(Bags[tier].Count);
        int cellID = Bags[tier][index];

        Bags[tier].RemoveAt(index);
        return cellID;
    }

    private void DisplayCell(MapCell cell)
    {
        PlaceTile(Shadow, cell.Pos.x, cell.Pos.y);
        cell.Fog = MapUI.I.PlaceFogPS(cell);
        cell.Tile.color = HalfOpacityWhite;
    }   

    public bool CanMove(Vector3 destination)
    {
        Vector3 currentPos = GetCurrentCell().Pos.toVec3;
        return CheckAdjacentCells(destination, currentPos) &&
            !TurnPhaser.I.ActiveDisc.HasActedInTurn &&
            !GetCell(destination).HasBattle &&
            !GetCell(destination).HasGroupPending;
    }

    public void Scout(Vector3 pos)
    {
        MapCell cell = MapCells[new Pos((int)pos.x, (int)pos.y)];
        cell.Discover();
        TurnPhaser.I.ActiveDisc.HasScoutedInTurn = true;

        // Draw card in advance to reveal enemy count if applicable.
        cell.Travelcard = TravelDeck.I.DrawCard(cell.Tier, cell.ID);
        if (!cell.HasTravelcard)
            return;
        if (cell.Travelcard.EnemyCount > 0)
        {
            EnemyLoader.I.GenerateNewEnemies(cell, cell.Travelcard.EnemyCount);
            cell.HasSeenCombat = true; // Will bypass enemy generation when cell is entered.
        }
    }

    public bool CanScout(Vector3 pos)
    {
        return GetTile(pos.x, pos.y) != null && CheckAdjacentCells(pos, TurnPhaser.I.ActiveDisc.Position) &&
            TurnPhaser.I.ActiveDisc.Bat.GetUnit(PlayerUnit.SCOUT) != null &&
            GetCurrentCell() != GetCell(pos) && GetCell(pos) != CityCell &&
            !GetCell(pos).Discovered &&
            !TurnPhaser.I.ActiveDisc.HasActedInTurn;
    }

    public bool CanTeleport(Vector3 pos)
    {
        MapCell cell = GetCell(pos);
        return cell.RestoredRuneGate && GetCurrentCell().RestoredRuneGate &&
            GetCurrentCell() != cell && cell != CityCell;
    }

    public static bool CheckAdjacentCells(Vector3 pos1, Vector3 pos2)
    {
        int dx = Mathf.Abs((int)pos1.x - (int)pos2.x);
        int dy = Mathf.Abs((int)pos1.y - (int)pos2.y);
        return dx + dy == 1;
    }

    public Tile GetTile(float x, float y)
    {
        if (x >= 0 && y >= 0)
            return Tilemap.GetTile<Tile>(new Vector3Int((int)x, (int)y, 0));
        return null;
    }

    public MapCell GetCurrentCell(Discipline disc = null)
    {
        return disc == null ? TurnPhaser.I.ActiveDisc.Cell : disc.Cell;
    }

    public void PlaceTile(Tile tile, int x, int y)
    {
        Tilemap.SetTile(new Vector3Int(x, y, 0), tile);
        Tilemap.SetTileFlags(new Vector3Int(y, x, 0), TileFlags.None); // Allow color change
    }

    public bool IsAtCity(Discipline disc)
    {
        return disc.Cell == CityCell;
    }

    public void AddOscillatingCell(MapCell cell)
    {
        OscillatingCells.Add(cell);
    }

    public bool RemoveOscillatingCell(MapCell cell)
    {
        Debug.Log("Contains cell? " + OscillatingCells.Contains(cell));
        if (OscillatingCells.Contains(cell))
        {
            Debug.Log("removing osc cell");
            OscillatingCells.Remove(cell);
            return true;
        }
        return false;
    }

    public MapCell GetCell(Vector3 pos)
    {
        Pos p = new Pos((int)pos.x, (int)pos.y);
        if (!MapCells.ContainsKey(p))
        {
            return null;
        }
        return MapCells[p];
    }

    public List<Enemy> GetEnemiesHere()
    {
        return TurnPhaser.I.ActiveDisc.Cell.GetEnemies();
    }

    public void RetreatBattalion()
    {
        TurnPhaser.I.ActiveDisc.MoveToPreviousCell();
    }

    public void BuildRuneGate(Pos pos)
    {
        MapCells[pos].RestoredRuneGate = true;
    }
    
    public void Teleport() 
    {   
        TurnPhaser.I.ActiveDisc.Teleport();
    }   

    private void CreateCity()
    {
        CityCell = (CityCell)MapCell.CreateCell(MapCell.IDCity, 1, City, new Pos(12, 12));
        CityCell.Discover();
        MapCells.Add(CityCell.Pos, CityCell);
    }

    void GenerateT1(Tilemap tm)
    {
        for (int x = 10; x < 15; x++)
        {
            for (int y = 10; y < 15; y++)
            {
                if (x == 12 && y == 12)
                {
                    PlaceTile(City, x, y);
                }
                else
                {
                    CreateCell(1, x, y);
                }
            }
        }
        CreateCell(1, 12, 9, MapCell.IDGuardianPass);
        CreateCell(1, 12, 15, MapCell.IDGuardianPass);
    }

    void GenerateT2(Tilemap tm)
    {
        // t2 origin is 5, 5
        // Horizontal bars
        for (int x = 6; x < 19; x++)
        {
            for (int y = 6; y < 9; y++)
            {
                CreateCell(2, x, y);
                CreateCell(2, x, y + 10);
            }
        }

        // Vertical bars
        for (int x = 6; x < 9; x++)
        {
            for (int y = 9; y < 16; y++)
            {
                CreateCell(2, x, y);
                CreateCell(2, x + 10, y);
            }
        }
        CreateCell(2, 5, 12, MapCell.IDGuardianPass);
        CreateCell(2, 19, 12, MapCell.IDGuardianPass);
        CreateCell(2, 12, 5, MapCell.IDGuardianPass);
        CreateCell(2, 12, 19, MapCell.IDGuardianPass);
    }

    void GenerateT3(Tilemap tm)
    {
        // (9 wide 3 deep over 2 wide band)
        // Horizontal protrusion
        for (int x = 8; x < 17; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                CreateCell(3, x, y);
                CreateCell(3, x, y + 22);
            }
        }
        // Vertical protrusion
        for (int x = 0; x < 3; x++)
        {
            for (int y = 8; y < 17; y++)
            {
                CreateCell(3, x, y);
                CreateCell(3, x + 22, y);
            }
        }
        // Horizontal bar
        for (int x = 3; x < 22; x++)
        {
            for (int y = 3; y < 5; y++)
            {
                CreateCell(3, x, y);
                CreateCell(3, x, y + 17);
            }
        }
        // Vertical bar
        for (int x = 3; x < 5; x++)
        {
            for (int y = 5; y < 20; y++)
            {
                CreateCell(3, x, y);
                CreateCell(3, x + 17, y);
            }
        }
    }

    public void ToggleScouting()
    {
        Scouting = !Scouting;
    }

    public GameData Save()
    {
        return new MapData(this, Game.MAP);
    }

    public void Load(GameData generic)
    {
        MapData data = generic as MapData;
        ClearData();

        foreach (int num in data.t1_bag)
            Bags[1].Add(num);
        foreach (int num in data.t2_bag)
            Bags[2].Add(num);
        foreach (int num in data.t3_bag)
            Bags[3].Add(num);

        // Recreate map.
        foreach (SMapCell mcs in data.cells)
        {
            Pos pos = new Pos(mcs.x, mcs.y);
            MapCell cell = MapCell.CreateCell(
                mcs.ID, mcs.tier, Tiles[mcs.ID][mcs.tier], pos);
            cell.Minerals = mcs.minerals;
            cell.StarCrystals = mcs.star_crystals;

            if (mcs.discovered)
            {
                cell.Discover();
            }
            else
            {
                PlaceTile(Shadow, pos.x, pos.y);
            }
            MapCells.Add(pos, cell);
        }
    }

    private void ClearData()
    {
        Bags[1].Clear();
        Bags[2].Clear();
        Bags[3].Clear();
        MapCells.Clear();
        MapUI.I.CloseCellUI();
        MapUI.I.CloseTravelcardP();
    }
}