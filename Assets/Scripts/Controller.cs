using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Controller : MonoBehaviour
{
    public static Controller I { get; private set; }

    public const string MAP = "Map";
    public const string CONTROLLER = "Controller";

    public Button loadB, saveB, resumeB;
    public GameObject save_warningP, new_game_warningP, load_warningP;
    public City city;
    public bool game_has_begun { get; private set; } = false;

    public event Action<bool> init; // True = from save, false = new game
    public PlayerDeployment playerDeployment;

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
            TurnPhaser.I.astra.pos = new Vector3(12.5f, 12.9f, 0);
            TurnPhaser.I.martial.pos = new Vector3(12.1f, 12.1f, 0);
            TurnPhaser.I.endura.pos = new Vector3(12.9f, 12.1f, 0);
        }
        // Clear fields not overwritten by possible load.
        BattlePhaser.I.reset(from_save);

        // Order matters
        Map.I.init(from_save);
        TurnPhaser.I.reset();
        EquipmentUI.I.init(TurnPhaser.I.activeDisc);

        MapUI.I.register_disc_change(TurnPhaser.I.activeDisc);

        game_has_begun = true;
    }

    public void game_over()
    {
        CamSwitcher.I.set_active(CamSwitcher.MENU, true);
        Initialize(false);
    }

    // Called by save button
    public void save_game()
    {
        // Double check the user wants to overwrite their save.
        save_warningP.SetActive(false);

        List<GameData> serializables = new List<GameData>() {
            { Map.I.save() },
            { TurnPhaser.I.activeDisc.save() },
            { city.save() },
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

        TurnPhaser.I.load(data);
        Map.I.load(FileIO.load_game(MAP));
        TurnPhaser.I.astra.load(FileIO.load_game("astra"));
        TurnPhaser.I.martial.load(FileIO.load_game("martial"));
        TurnPhaser.I.endura.load(FileIO.load_game("endura"));
        city.load(FileIO.load_game("city"));

        Initialize(true);
        CamSwitcher.I.flip_menu_map();
    }


    public void new_game()
    {
        ChooseNewGamediscipline(Discipline.ASTRA);
        Initialize(false);
        CamSwitcher.I.flip_menu_map();
    }

    public void ChooseNewGamediscipline(int ID) {
        ID = 0; // Force astra
        Discipline d = null;
        if (ID == Discipline.ASTRA) {
            d = TurnPhaser.I.astra;
        }
        else if (ID == Discipline.MARTIAL) 
        {
            d = TurnPhaser.I.martial;
        }
        else if (ID == Discipline.ENDURA) 
        {
            d = TurnPhaser.I.endura;
        }
        TurnPhaser.I.Init(d);
        d.init(false);
    }

    public void check_button_states()
    {
        loadB.interactable = FileIO.load_file_exists();
        saveB.interactable = game_has_begun;
        resumeB.interactable = game_has_begun;
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
        if (game_has_begun)
        {
            load_warningP.SetActive(true);
            return;
        }
        load_game();
    }

    public void new_game_button()
    {
        if (game_has_begun)
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
        return playerDeployment;
    }

    // BUTTON HANDLES
    public void inc_stat(string field)
    {
        TurnPhaser.I.activeDisc.change_var(field, 1);
    }

    public void dec_stat(string field)
    {
        TurnPhaser.I.activeDisc.change_var(field, -1);
    }

    public void inc_city_stat(string field)
    {
        city.change_var(field, 1);
    }

    public void dec_city_stat(string field)
    {
        city.change_var(field, -1);
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

    public Vector3 to_vec3 { get { return new Vector3(x, y, 0); } }
}