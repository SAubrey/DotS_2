using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

public class CamSwitcher : MonoBehaviour
{
    public static CamSwitcher I { get; private set; }
    public const int MENU = 1, MAP = 2, BATTLE = 3;
    public Camera menu_cam, map_cam, BattleCamCaster;
    public CinemachineVirtualCamera BattleCam;
    public GameObject menu_canvas;
    public GameObject battle_canvas;
    public GameObject battleUI_canvas;
    public GameObject map_canvas;
    public GameObject mapUI_canvas;
    public SoundManager sound_manager;
    public GameObject pause_panel, battle_pause_panel;

    public bool Paused {get; protected set;} = false;
    public int current_cam = MENU;
    public int previous_cam = MAP;
    [SerializeField] public Transform FollowTransform;

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
        sound_manager = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        pause_panel.SetActive(false);
        battle_pause_panel.SetActive(false);
        SetActive(MENU, true);

        QualitySettings.SetQualityLevel(2, true); // REMOVE
    }

    void Update()
    {
        
        if (Controller.I.B.triggered && Game.I.DebugMode)
        {
            Cycle(); // Debug
        }
        if (Controller.I.QualityMinus.triggered)
        {
            QualitySettings.SetQualityLevel(0, true); // REMOVE
        }
        if (Controller.I.QualityPlus.triggered)
        {
            QualitySettings.SetQualityLevel(2, true); // REMOVE
        }
    }

    void FixedUpdate()
    {
        if (Controller.I.Escape.triggered)
        {
            Pause();
        }
    }

    void Cycle()
    {
        if (current_cam == MAP)
        {
            SetActive(BATTLE, true);
        }
        else if (current_cam == BATTLE)
        {
            SetActive(MAP, true);
        }
    }

    // If pausing, lose focus and bring up menu. If unpausing, 
    public void Togglepaused()
    {
        Paused = !Paused;
        if (Paused) Pause(); else Unpause();
    }

    public void Pause()
    {
        if (current_cam == MAP)
        {
            pause_panel.SetActive(true);
        }
        else if (current_cam == BATTLE)
        {
            battle_pause_panel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
        }
        Time.timeScale = 0f;
    }

    public void Unpause()
    {
        Debug.Log("Unpausing");
        if (current_cam == MAP)
        {
            pause_panel.SetActive(false);
        }
        else if (current_cam == BATTLE)
        {
            battle_pause_panel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
        Time.timeScale = 1f;
    }

    // Called by buttons
    public void flip_menu_map()
    {
        if (current_cam == MENU)
        {
            SetActive(MAP, true);
        }
        else if (current_cam == MAP)
        {
            SetActive(MENU, true);
        }
    }

    public void flip_map_battle()
    {
        if (current_cam == MAP)
        {
            SetActive(BATTLE, true);
        }
        else if (current_cam == BATTLE)
        {
            SetActive(MAP, true);
        }
    }

    public void flip_menu_battle()
    {
        if (current_cam == MENU)
        {
            SetActive(BATTLE, true);
        }
        else if (current_cam == BATTLE)
        {
            SetActive(MENU, true);
        }
    }

    public void return_previous_cam()
    {
        SetActive(previous_cam, true);
    }

    public void SetActive(int screen, bool active)
    {
        if (screen == MENU)
        {
            menu_canvas.SetActive(active);
            menu_cam.enabled = active;
            if (active)
            {
                SetActive(MAP, false);
                SetActive(BATTLE, false);
                Game.I.check_button_states();
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (screen == MAP)
        {
            map_canvas.SetActive(active);
            map_cam.enabled = active;
            mapUI_canvas.SetActive(active);
            if (active)
            {
                // Set the camera on the active player.
                Vector3 p = new Vector3(TurnPhaser.I.ActiveDisc.Position.x, TurnPhaser.I.ActiveDisc.Position.y, -14);
                map_cam.transform.SetPositionAndRotation(p, Quaternion.identity);
                SetActive(BATTLE, false);
                SetActive(MENU, false);
                Cursor.lockState = CursorLockMode.None;
            }
        }
        else if (screen == BATTLE)
        {
            battle_canvas.SetActive(active);
            BattleCam.enabled = active;
            battleUI_canvas.SetActive(active);
            if (active)
            {
                TerrainLoader.I.Load(Map.I.GetCurrentCell().ID);
                SetActive(MAP, false);
                SetActive(MENU, false);
                Cursor.lockState = CursorLockMode.Locked;
                Debug.Log("screen set to battle");
            }
        }

        if (active)
        { // If not turning this camera off.
            previous_cam = current_cam;
            current_cam = screen;
            if (previous_cam != current_cam)
                sound_manager.background_SFX_player.activate_screen(screen);
        }
    }
}
