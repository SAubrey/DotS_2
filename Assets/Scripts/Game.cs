using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Game : MonoBehaviour
{
    public static Game I { get; private set; }

    public const string MAP = "Map";
    public const string CONTROLLER = "Controller";

    public Button loadB, saveB, resumeB;
    public GameObject save_warningP, new_game_warningP, load_warningP;
    public City city;
    public bool GameHasBegun { get; private set; } = false;

    public event Action<bool> init; // True = from save, false = new game
    public PlayerDeployment PlayerDeployment;
    public bool DebugMode = true;

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
        Physics2D.gravity = new Vector2(0, 0);
        save_warningP.SetActive(false);
        new_game_warningP.SetActive(false);
        load_warningP.SetActive(false);
    }

    public void Initialize(bool from_save)
    {
        if (init != null)
            init(from_save);

        if (from_save)
        {

        }
        else
        {
            TurnPhaser.I.Astra.Position = new Vector3(12.5f, 12.9f, 0);
            TurnPhaser.I.Martial.Position = new Vector3(12.1f, 12.1f, 0);
            TurnPhaser.I.Endura.Position = new Vector3(12.9f, 12.1f, 0);
        }
        // Clear fields not overwritten by possible load.
        BattlePhaser.I.Reset(from_save);

        // Order matters
        Map.I.Init(from_save);
        TurnPhaser.I.Reset();
        EquipmentUI.I.Init(TurnPhaser.I.ActiveDisc);

        MapUI.I.RegisterDiscChange(TurnPhaser.I.ActiveDisc);

        GameHasBegun = true;
    }

    public void GameOver()
    {
        CamSwitcher.I.SetActive(CamSwitcher.MENU, true);
        Initialize(false);
    }

    // Called by save button
    public void save_game()
    {
        // Double check the user wants to overwrite their save.
        save_warningP.SetActive(false);

        List<GameData> serializables = new List<GameData>() {
            { Map.I.Save() },
            { TurnPhaser.I.ActiveDisc.Save() },
            { city.Save() },
        };

        foreach (var s in serializables)
        {
            FileIO.save_game(s, s.name);
        }
    }

    public void load_game()
    {
        TurnPhaserData data = FileIO.load_game("TurnPhaser") as TurnPhaserData;
        if (data == null)
            return;

        TurnPhaser.I.Load(data);
        Map.I.Load(FileIO.load_game(MAP));
        TurnPhaser.I.Astra.Load(FileIO.load_game("astra"));
        TurnPhaser.I.Martial.Load(FileIO.load_game("martial"));
        TurnPhaser.I.Endura.Load(FileIO.load_game("endura"));
        city.Load(FileIO.load_game("city"));

        Initialize(true);
        CamSwitcher.I.flip_menu_map();
    }


    public void new_game()
    {
        ChooseNewGamediscipline(Discipline.Astra);
        Initialize(false);
        CamSwitcher.I.flip_menu_map();
    }

    public void ChooseNewGamediscipline(int ID) {
        ID = 0; // Force astra
        Discipline d = null;
        if (ID == Discipline.Astra) {
            d = TurnPhaser.I.Astra;
        }
        else if (ID == Discipline.Martial) 
        {
            d = TurnPhaser.I.Martial;
        }
        else if (ID == Discipline.Endura) 
        {
            d = TurnPhaser.I.Endura;
        }
        TurnPhaser.I.Init(d);
        d.Init(false);
    }

    public void check_button_states()
    {
        loadB.interactable = FileIO.load_file_exists();
        saveB.interactable = GameHasBegun;
        resumeB.interactable = GameHasBegun;
    }

    public void save_button()
    {
        //if (FileIO.load_file_exists()) {
        save_warningP.SetActive(true);
        //return;
        //}
        //save_game();
    }

    public void load_button()
    {
        if (GameHasBegun)
        {
            load_warningP.SetActive(true);
            return;
        }
        load_game();
    }

    public void new_game_button()
    {
        if (GameHasBegun)
        {
            new_game_warningP.SetActive(true);
            return;
        }
        new_game();
    }

    public void show_warning_panel(bool active)
    {
        save_warningP.SetActive(active);
    }

    public PlayerDeployment get_deployment()
    {
        return PlayerDeployment;
    }

    // BUTTON HANDLES
    public void inc_stat(string field)
    {
        TurnPhaser.I.ActiveDisc.AdjustResource(field, 1);
    }

    public void dec_stat(string field)
    {
        TurnPhaser.I.ActiveDisc.AdjustResource(field, -1);
    }

    public void inc_city_stat(string field)
    {
        city.AdjustResource(field, 1);
    }

    public void dec_city_stat(string field)
    {
        city.AdjustResource(field, -1);
    }

    public void quit_game()
    {
        Application.Quit();
    }
}

public struct Pos
{
    public int x, y;
    public Pos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public Vector3 toVec3 { get { return new Vector3(x, y, 0); } }
}