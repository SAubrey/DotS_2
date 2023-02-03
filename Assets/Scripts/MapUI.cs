using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class MapUI : MonoBehaviour
{
    public static MapUI I { get; private set; }
    public TextMeshProUGUI TurnNumberT, TeleportT;
    public Button TeleportB;

    // ---City UI---
    public GameObject CityP;
    public TextMeshProUGUI TextCityCapacity;
    public TextMeshProUGUI c_light, c_star_crystals, c_minerals,
        c_arelics, c_mrelics, c_erelics, c_equimares;
    public IDictionary<string, TextMeshProUGUI> CityInv = new Dictionary<string, TextMeshProUGUI>();


    // Battalion Resource UI
    public GameObject InvP;
    public TextMeshProUGUI b_light, b_unity, b_experience, b_star_crystals,
         b_minerals, b_arelics, b_mrelics, b_erelics, b_equimares;


    // Battalion Unit UI
    public GameObject UnitsP;
    private bool UnitsPActive = true;
    public TextMeshProUGUI TextBatCapacity;
    public Dictionary<int, TextMeshProUGUI> TextUnitCounts = new Dictionary<int, TextMeshProUGUI>();
    public TextMeshProUGUI CountWarrior, CountSpearman, CountArcher,
        CountMiner, CountInspirator, CountSeeker,
        CountGuardian, CountArbalest, CountSkirmisher,
        CountPaladin, CountMender, CountCarter, CountDragoon,
        CountScout, CountDrummer, CountShieldMaiden, CountPikeman;

    public IDictionary<string, TextMeshProUGUI> DiscInv = new Dictionary<string, TextMeshProUGUI>();
    public TextMeshProUGUI TextDisc, TextMapCell, TextBattleCell;
    public Button NextTurnB;
    public GameObject ask_to_enterP, game_overP, travel_cardP;
    public TextMeshProUGUI travelcard_descriptionT, travelcard_typeT,
        travelcard_subtextT, travelcard_consequenceT;
    public Image discI;
    public Sprite astraI, martialI, enduraI;
    public Button travelcard_continueB, travelcard_rollB, travelcard_exitB;

    public GameObject cell_UI_prefab, dropped_XP_prefab;
    public MapCellUI open_cell_UI_script;
    public MapCell last_open_cell;
    public GraphicRaycaster graphic_raycaster;
    public Canvas canvas;
    public GameObject map_canvas;
    public GameObject map_cell_light_prefab, map_cell_sparkles_prefab,
        map_cell_fog_prefab;
    private int max_discovered_tile_distance = 0;
    public UnityEngine.Rendering.Universal.Light2D city_light;
    public Color star_light_color, titrum_light_color, rune_gate_light_color,
        forest_light_color, lush_land_color, cave_color, mountain_color, plains_color,
        ruins_color;
    public Dictionary<int, Color> CellLightColors;
    public PlayerDeploymentUI PlayerDeploymentUI;

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

        CellLightColors = new Dictionary<int, Color>() {
            {MapCell.IDStar, star_light_color},
            {MapCell.IDTitrum, titrum_light_color},
            {MapCell.IDRuneGate, rune_gate_light_color},
            {MapCell.IDForest, forest_light_color},
            {MapCell.IDLushLand, lush_land_color},
            {MapCell.IDCave, cave_color},
            {MapCell.IDMountain, mountain_color},
            {MapCell.IDPlains, plains_color},
            //{MapCell.SETTLEMENT_ID, settlement_color},
            {MapCell.IDRuins, ruins_color},
        };

        // Populate city dictionary
        CityInv.Add(Storeable.LIGHT, c_light);
        CityInv.Add(Storeable.STAR_CRYSTALS, c_star_crystals);
        CityInv.Add(Storeable.MINERALS, c_minerals);
        CityInv.Add(Storeable.ARELICS, c_arelics);
        CityInv.Add(Storeable.MRELICS, c_mrelics);
        CityInv.Add(Storeable.ERELICS, c_erelics);
        CityInv.Add(Storeable.EQUIMARES, c_equimares);

        // Populate batallion dictionary
        DiscInv.Add(Storeable.LIGHT, b_light);
        DiscInv.Add(Storeable.UNITY, b_unity);
        DiscInv.Add(Discipline.Experience, b_experience);
        DiscInv.Add(Storeable.STAR_CRYSTALS, b_star_crystals);
        DiscInv.Add(Storeable.MINERALS, b_minerals);
        DiscInv.Add(Storeable.ARELICS, b_arelics);
        DiscInv.Add(Storeable.MRELICS, b_mrelics);
        DiscInv.Add(Storeable.ERELICS, b_erelics);
        DiscInv.Add(Storeable.EQUIMARES, b_equimares);

        TextUnitCounts.Add(PlayerUnit.WARRIOR, CountWarrior);
        TextUnitCounts.Add(PlayerUnit.SPEARMAN, CountSpearman);
        TextUnitCounts.Add(PlayerUnit.ARCHER, CountArcher);
        TextUnitCounts.Add(PlayerUnit.GUARDIAN, CountGuardian);
        TextUnitCounts.Add(PlayerUnit.ARBALEST, CountArbalest);
        TextUnitCounts.Add(PlayerUnit.PALADIN, CountPaladin);
        TextUnitCounts.Add(PlayerUnit.MENDER, CountMender);
    }

    void Start()
    {
        SetActiveTravelcardContinueB(true);
        SetActiveTravelcardRollB(false);
        TurnPhaser.I.OnDiscChange += RegisterDiscChange;
        TurnPhaser.I.OnTurnChange += RegisterTurn;

        foreach (Discipline d in TurnPhaser.I.Discs.Values)
        {
            d.OnResourceChange += UpdateStatText;
            d.OnCapacityChange += RegisterCapacityChange;
            d.OnUnitCountChange += LoadBattalionCount;
        }
        Game.I.city.OnResourceChange += UpdateStatText;
    }

    void Update()
    {
        if (CamSwitcher.I.current_cam != CamSwitcher.MAP)
            return;

        // Oscillate tile colors
        foreach (MapCell mc in Map.I.OscillatingCells)
        {
            mc.OscillateColor();
        }

        //if (!Controller.I.AnyKey.triggered)
          //  return;

        // Process left mouse click
        if (Controller.I.LeftClick.triggered)
        {
            HandleLeftClick();
            return;
        }

        // Decide whether to advance or close windows.
        bool advance = Controller.I.LeftClick.triggered;
        bool exit = Controller.I.Escape.triggered;
        bool UIopen = CityUI.I.cityP.activeSelf || CellUIIsOpen ||
            game_overP.activeSelf || travel_cardP.activeSelf;
        if (advance)
        {
            if (NextTurnB.IsActive() && !UIopen)
            {
                TurnPhaser.I.EndDisciplinesTurn();
            }

        }
        else if (exit && UIopen)
        {
            if (CellUIIsOpen)
                CloseCellUI();
            else if (CityUI.I.upgradesP.activeSelf)
            {
                CityUI.I.toggle_upgrades_panel();
            }
            else if (CityUI.I.cityP.activeSelf)
            {
                CityUI.I.toggle_city_panel();
            }
        }
    }

    public void RegisterDiscChange(Discipline d)
    {
        LoadBattalionCount();
        CityUI.I.load_unit_counts();
        HighlightDiscipline(TextDisc, discI, d.ID);

        UpdateStoreableResourceUI(Game.I.city);
        UpdateStoreableResourceUI(d);
    }

    public void RegisterTurn()
    {
        TurnNumberT.text = TurnPhaser.I.Turn.ToString();
    }

    public void RegisterCapacityChange(int ID, int sum, int capacity)
    {
        if (TurnPhaser.I.ActiveDiscID != ID)
            return;
        UpdateCapacityText(TextBatCapacity, sum, capacity);
    }

    public void UpdateDeployment(Battalion bat)
    {
        foreach (List<PlayerUnit> pu in bat.Units.Values)
        {
            foreach (Unit u in pu)
            {
                PlayerDeploymentUI.PlaceUnit(u);
            }
        }
    }

    public void AdjustLightSize(MapCell cell)
    {
        int dist = Statics.CalcMapDistance(cell.Pos, new Pos(10, 10));
        Debug.Log("distance from city: " + dist);
        if (dist > max_discovered_tile_distance)
        {
            max_discovered_tile_distance = dist;
            city_light.pointLightInnerRadius = dist;
            city_light.pointLightOuterRadius = dist + 3;
        }
    }

    public void PlaceCellLight(MapCell cell)
    {
        if (cell == Map.I.CityCell || cell == null)
            return;
        GameObject light = Instantiate(map_cell_light_prefab);
        light.transform.SetParent(map_canvas.transform);
        Vector3 p = cell.Pos.toVec3;
        light.transform.position = new Vector3(p.x + 0.5f, p.y + 0.5f, 0);
        light.GetComponent<MapCellLight>().Init(cell);
    }

    public void PlaceSparklePS(MapCell cell)
    {
        if (cell == null)
            return;
        GameObject ps = Instantiate(map_cell_sparkles_prefab);
        ps.transform.SetParent(map_canvas.transform);
        Vector3 p = cell.Pos.toVec3;
        ps.transform.position = new Vector3(p.x + 0.5f, p.y + 0.5f, 0);
    }

    public GameObject PlaceFogPS(MapCell cell)
    {
        if (cell == null)
            return null;
        GameObject fog = Instantiate(map_cell_fog_prefab);
        fog.transform.SetParent(map_canvas.transform);
        Vector3 p = cell.Pos.toVec3;
        fog.transform.position = new Vector3(p.x + 0.5f, p.y + 0.5f, 0);
        return fog;
    }

    public void UpdateStoreableResourceUI(Storeable s)
    {
        // Trigger resource property UI updates by setting values to themselves.
        foreach (string resource in s.Resources.Keys)
        {
            UpdateStatText(s.ID, resource, s.GetResource(resource), s.GetSumStoreableResources(), s.Capacity);
        }
    }


    private void HandleLeftClick()
    {
        Vector2 mousePos = Controller.I.MousePosition.ReadValue<Vector2>();
        PointerEventData m_PointerEventData = new PointerEventData(EventSystem.current);
        m_PointerEventData.position = mousePos;
        List<RaycastResult> objects = new List<RaycastResult>();

        graphic_raycaster.Raycast(m_PointerEventData, objects);
        // Close the open cell window if clicking anywhere other than on the window.
        // (The tilemap does not register as a game object)
        bool hit_cell_window = false;
        bool hit_window = false;
        foreach (RaycastResult o in objects)
        {
            if (o.gameObject.tag == "Cell Window")
            {
                hit_cell_window = true;
                hit_window = true;
                continue;
            }
            else if (o.gameObject.tag == "MapWindow")
            {
                hit_window = true;
                continue;
            }
        }
        if (EquipmentUI.I.selecting)
        {
            EquipmentUI.I.selecting = false;
            hit_window = true;
        }

        Vector3 pos = CamSwitcher.I.map_cam.ScreenToWorldPoint(mousePos);
        bool over_tile = Map.I.GetTile(pos.x, pos.y) != null;
        bool forced_tc_interaction = (travel_cardP.activeSelf && !travelcard_exitB.interactable)
            || ask_to_enterP.activeSelf;

        if (forced_tc_interaction)
            return;

        if (!hit_cell_window)
        {
            CloseCellUI();

            if (objects.Count <= 0 && !hit_window)
            {
                CloseTravelcardP();

                if (over_tile)
                {
                    if (Game.I.DebugMode)
                        Map.I.GetCell(pos).Discover(); // DEBUG
                    GenerateCellUI(Map.I.GetCell(pos));
                }

            }
        }
    }

    private void GenerateCellUI(MapCell cell)
    {
        GameObject cell_UI = Instantiate(cell_UI_prefab, canvas.transform);
        open_cell_UI_script = cell_UI.GetComponentInChildren<MapCellUI>();
        last_open_cell = cell;
        open_cell_UI_script.init(cell);
    }

    public void CloseCellUI()
    {
        if (!CellUIIsOpen)
            return;
        open_cell_UI_script.close();
        open_cell_UI_script = null;
    }

    public bool CellUIIsOpen { get => open_cell_UI_script != null; }

    public void LoadBattalionCount()
    {
        Battalion b = TurnPhaser.I.ActiveDisc.Bat;
        if (b == null)
            return;
        foreach (int type_ID in b.Units.Keys)
        {
            TextUnitCounts[type_ID].text = BuildUnitText(b, type_ID);
        }
    }

    public string BuildUnitText(Battalion b, int ID)
    {
        if (!TextUnitCounts.ContainsKey(ID))
            return "";

        string num = b.CountUnits(ID).ToString();
        int total_num = b.Units[ID].Count;
        return TextUnitCounts[ID].text = total_num.ToString();
    }

    public static void UpdateCapacityText(TextMeshProUGUI text, int sum_resources, int capacity)
    {
        if (text == null)
            return;
        text.text = sum_resources + " / " + capacity;
    }

    public void UpdateStatText(int disc_ID, string field, int val, int sum, int capacity)
    {
        TextMeshProUGUI t = null;
        if (disc_ID == City.CITY)
        {
            CityInv.TryGetValue(field, out t);
            UpdateCapacityText(TextCityCapacity, sum, capacity);
        }
        else if (disc_ID == TurnPhaser.I.ActiveDiscID)
        {
            DiscInv.TryGetValue(field, out t);
            UpdateCapacityText(TextBatCapacity, sum, capacity);
        }
        if (t != null)
            t.text = val.ToString();
    }

    public void HighlightDiscipline(TextMeshProUGUI txt, Image img, int disc_ID)
    {
        Sprite s = null;
        string t = "";
        if (disc_ID == Discipline.Astra)
        {
            t = "Astra";
            s = astraI;
        }
        else if (disc_ID == Discipline.Martial)
        {
            t = "Martial";
            s = martialI;
        }
        else if (disc_ID == Discipline.Endura)
        {
            t = "Endura";
            s = enduraI;
        }
        txt.text = t;
        if (img != null)
            img.sprite = s;
        txt.color = Statics.DisciplineColors[disc_ID];
    }

    public void ToggleCityPanel()
    {
        // If the battalion is at the city, toggle the main city panel.
        if (Map.I.IsAtCity(TurnPhaser.I.ActiveDisc))
        {
            CityUI.I.toggle_city_panel();
        }
        else
        {
            CityP.SetActive(!CityP.activeSelf);
        }
    }

    public void ToggleInvPanel()
    {
        InvP.SetActive(!InvP.activeSelf);
    }

    public void UpdateCellText(string tile_name)
    {
        TextMapCell.text = tile_name;
        TextBattleCell.text = tile_name;
    }

    public void DisplayTravelcard(TravelCard tc)
    {
        if (tc == null || travel_cardP.activeSelf)
            return;
        bool activeDisc_at_selected_cell =
            TurnPhaser.I.ActiveDisc.Cell == open_cell_UI_script.cell;
        bool interactable = !tc.complete && activeDisc_at_selected_cell;
        SetActiveTravelcardContinueB(interactable);
        SetActiveTravelcardRollB(interactable && tc.DieNumSides > 0 && !tc.Rolled);
        SetActiveTravelcardExitB(!interactable);
        SetActiveNextStageB(false);

        tc.OnOpen(TravelCardManager.I);

        // Update text
        travelcard_typeT.text = tc.type_text;
        travelcard_descriptionT.text = tc.description;
        travelcard_subtextT.text = tc.subtext;
        travelcard_consequenceT.text = tc.consequence_text;
        travel_cardP.SetActive(true);
    }

    public void CloseTravelcardP()
    {
        SetActiveAskToEnterP(false);
        SetActiveTravelcardContinueB(false);
        SetActiveTravelcardRollB(false);
        travel_cardP.SetActive(false);
        SetActiveNextStageB(true);
    }

    public void ToggleTravelcard(TravelCard tc)
    {
        if (travel_cardP.activeSelf)
        {
            CloseTravelcardP();
        }
        else
        {
            DisplayTravelcard(tc);
        }
    }

    public void SetActiveTravelcardRollB(bool state)
    {
        travelcard_rollB.interactable = state;
        TextMeshProUGUI t = travelcard_rollB.GetComponentInChildren<TextMeshProUGUI>();
        t.text = state ? "Roll" : "";
    }

    public void SetActiveTravelcardExitB(bool state)
    {
        travelcard_exitB.interactable = state;
    }

    public void SetActiveTravelcardContinueB(bool state)
    {
        travelcard_continueB.interactable = state;
    }

    public void SetActiveAskToEnterP(bool state)
    {
        ask_to_enterP.SetActive(state);
    }

    public void SetActiveGameOverP(bool state)
    {
        game_overP.SetActive(state);
    }

    // Button driven
    public void ToggleUnitsPanel()
    {
        UnitsPActive = !UnitsPActive;
        UnitsP.SetActive(UnitsPActive);
    }

    public void SetActiveNextStageB(bool state)
    {
        NextTurnB.interactable = state;
    }
}