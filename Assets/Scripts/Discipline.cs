using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Discipline : Storeable, ISaveLoad
{
    public const string Experience = "experience";
    public const int Astra = 0, Endura = 1, Martial = 2;
    public GameObject Piece;
    public Battalion Bat { get; private set; }
    
    public bool Active = false;
    public bool RestartBattleFromDrawnCard = false;
    private int MineQtyMultiplier = 3;
    public int MineQty
    {
        get => MineQtyMultiplier *= Bat.CountUnits(PlayerUnit.MINER);
    }
    public bool HasMinedInTurn, HasMovedInTurn, HasScoutedInTurn = false;
    public bool HasActedInTurn { get => HasMovedInTurn || HasScoutedInTurn; }
    public bool Dead { get; private set; } = false;
    private MapCell _cell;
    // Change cells to change piece location on the map.
    public MapCell Cell
    {
        get => _cell;
        set
        {
            if (value == null)
                return;
            PreviousCell = Cell;
            _cell = value;
            Position = Cell.Pos.to_vec3;
        }
    }
    public MapCell PreviousCell { get; private set; }
    private Vector3 _pos;
    public Vector3 Position
    {
        get { return _pos; }
        set
        {
            _pos = value;
            Piece.transform.position = new Vector3(value.x, value.y, 0);
        }
    }

    public EquipmentInventory EquipmentInventory;
    public event Action OnUnitCountChange;
    public void TriggerUnitCountChange() 
    { 
        if (OnUnitCountChange != null)
            OnUnitCountChange(); 
    }

    protected override void Start()
    {
        base.Start();
        Resources.Add(Experience, 0);
        MineQtyMultiplier = ID == Endura ? 4 : 3;
    }

    public override void Init(bool from_save)
    {
        base.Init(from_save);
        Initialized = true;
        Bat = new Battalion(this);
        EquipmentInventory = new EquipmentInventory(this);

        Resources[LIGHT] = 4;
        Resources[UNITY] = 5;
        Resources[STAR_CRYSTALS] = 1;
        Resources[Experience] = 500;
        Cell = Map.I.CityCell;
        MapUI.I.UpdateDeployment(Bat);
    }

    public override void RegisterTurn()
    {
        if (!Initialized)
            return;

        if (!Dead)
        {
            base.RegisterTurn();
            CheckInsanity(GetResource(UNITY));
        }
        HasMinedInTurn = false;
        HasMovedInTurn = false;
        HasScoutedInTurn = false;
    }

    public void Move(MapCell cell)
    {
        // Offset position on cell to simulate human placement and prevent perfect overlap.
        this.Cell = cell;
        float randx = UnityEngine.Random.Range(-0.2f, 0.2f);
        float randy = UnityEngine.Random.Range(-0.2f, 0.2f);
        Position = new Vector3(cell.Pos.x + 0.5f + randx, cell.Pos.y + 0.5f + randy, 0);
        HasMovedInTurn = true;
        MapUI.I.UpdateCellText(cell.Name);
        cell.Enter();
    }

    public void MoveToPreviousCell()
    {
        Move(PreviousCell);
    }

    public void Teleport()
    {
        // Rune gates allow free travel to other rune gates.
        if (Cell.RestoredRuneGate)
        {
            // Indicate rune gates.

        }
        // Return trip on one-way paid teleport.
        else if (Cell.TeleportDestination != null) 
        {
            // Move player piece to previously logged location.
            MapCell prevCell = Cell;
            Cell = Cell.TeleportDestination;
            prevCell.TeleportDestination = null;
        }
        // If No rune gate or return teleport, teleport to city and reduce SC cost.
        else 
        {
            if (GetResource(Storeable.STAR_CRYSTALS) >= 5)
            {  
                MapCell mc = Cell;
                Cell = Map.I.CityCell;
                Cell.TeleportDestination = mc;
                AdjustResource(Storeable.STAR_CRYSTALS, -5, true);
            }
        }
    }

    public void Die()
    {
        Move(Map.I.CityCell);
        Game.I.GameOver();
        Respawn();
    }

    public void Respawn()
    {
        Bat.AddDefaultTroops();
        Move(Map.I.CityCell);
        HasMovedInTurn = false;
        Dead = false;
    }

    private void CheckInsanity(int unity)
    {
        // 5 == "wavering"
        // 4 == unable to build
        if (unity == 3)
        {
            RollForInsanity(1, 20);
        }
        else if (unity == 2)
        {
            RollForInsanity(2, 50);
        }
        else if (unity < 2)
        {
            RollForInsanity(2, 80);
        }
    }

    private void RollForInsanity(int quantity, int chance)
    {
        int roll = UnityEngine.Random.Range(1, 100);
        if (roll <= chance)
        {
            // Units flee into the darkness.
            for (int i = 0; i < quantity; i++)
            {
                Bat.LoseRandomUnit("Your ranks crumble without the light of the stars.");
            }
        }
    }

    public void ReceiveTravelcardConsequence()
    {
        if (GetTravelcard() == null)
            return;
        ShowAdjustments(Cell.get_travelcard_consequence());
        if (GetTravelcard().EquipmentRewardAmount > 0)
        {
            string name = EquipmentInventory.AddRandomEquipment(Cell.Tier);
            RisingInfo.create_rising_info_map(
                RisingInfo.build_resource_text(name, 1),
                Statics.DisciplineColors[ID],
                origin_of_rise_obj.transform,
                rising_info_prefab);
        }
        Cell.CompleteTravelcard();
    }

    public void AddXPInBattle(int xp, Enemy enemy)
    {
        // Show xp rising over enemy
        AdjustResource(Discipline.Experience, xp, false);
        StartCoroutine(_add_xp_in_battle(xp, enemy));
    }

    private IEnumerator _add_xp_in_battle(int xp, Enemy enemy)
    {
        yield return new WaitForSeconds(1f);
        RisingInfo.create_rising_info_battle(
            RisingInfo.build_resource_text("XP", xp),
            Statics.DisciplineColors[ID],
            enemy.GetSlot().transform,
            rising_info_prefab);
    }

    public TravelCard GetTravelcard()
    {
        return Cell.Travelcard;
    }

    public void Mine(MapCell cell)
    {
        int mined = 0;
        if (cell.ID == MapCell.IDTitrum || cell.ID == MapCell.IDMountain)
        {
            mined = Statics.CalcValidNonnegativeChange(cell.Minerals, MineQty);
            ShowAdjustment(Storeable.MINERALS, mined);
            cell.Minerals -= mined;
        }
        else if (cell.ID == MapCell.IDStar)
        {
            mined = Statics.CalcValidNonnegativeChange(cell.StarCrystals, MineQty);
            ShowAdjustment(Storeable.STAR_CRYSTALS, mined);
            cell.StarCrystals -= mined;
        }
        HasMinedInTurn = true;
        if (MapUI.I.CellUIIsOpen)
            MapUI.I.open_cell_UI_script.update_mineable_text();
    }

    public void Reset()
    {
        RestartBattleFromDrawnCard = false;
        foreach (int type in PlayerUnit.UnitTypes)
        {
            Bat.Units[type].Clear();
        }
    }

    public override GameData Save()
    {
        return new DisciplineData(this, Name);
    }

    public override void Load(GameData generic)
    {
        DisciplineData data = generic as DisciplineData;
        Reset();
        Resources[LIGHT] = data.sresources.light;
        Resources[UNITY] = data.sresources.unity;
        Resources[STAR_CRYSTALS] = data.sresources.star_crystals;
        Resources[MINERALS] = data.sresources.minerals;
        Resources[ARELICS] = data.sresources.arelics;
        Resources[ERELICS] = data.sresources.erelics;
        Resources[MRELICS] = data.sresources.mrelics;

        Position = new Vector3(data.col, data.row);
        Cell.Travelcard = TravelDeck.I.MakeCard(data.redrawn_travel_card_ID);
        RestartBattleFromDrawnCard = Cell.Travelcard != null;

        // Create healthy units.
        Debug.Log("count:" + PlayerUnit.UnitTypes.Count);
        Debug.Log("count:" + data.sbat.unit_types.Count);
        foreach (int type in PlayerUnit.UnitTypes)
        {
            Bat.AddUnits(type, data.sbat.unit_types[type]);
        }
    }
}
