using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Controller : MonoBehaviour
{
    public static Controller I { get; private set; }
    private PlayerInput Input;
    public InputAction Attack;
    public InputAction Block;

    public InputAction Ability1, Ability2, Ability3, Ability4;
    public InputAction LockOn;
    public InputAction Move;
    public InputAction LeftClick;
    public InputAction Escape;
    public InputAction B;
    public InputAction MousePosition;
    public InputAction AnyKey;
    public InputAction Plus, Minus;
    public Mouse mouse;
    public InputAction LeftClickHeld, RightClickHeld;
    public InputAction MouseDelta;
    public InputAction QualityPlus, QualityMinus;
    public InputAction DrawArrow;
    public bool cursorInputForLook = true;
    public Vector2 move;
    public Vector2 look;
    public bool jump;
    public bool sprint;
    public bool analogMovement = false;
    
    private void Awake()
    {
        if (I == null)
        {
            I = this;
        }
        else
        {
            Destroy(gameObject);
        }

        mouse = Mouse.current;
        Input = GetComponent<PlayerInput>();
        MousePosition = Input.actions["MousePosition"];
        MouseDelta = Input.actions["NormalMouseDelta"];

        Attack = Input.actions["Attack"];
        Block = Input.actions["Block"];

        Ability1 = Input.actions["Ability1"];
        Ability2 = Input.actions["Ability2"];
        Ability3 = Input.actions["Ability3"];
        Ability4 = Input.actions["Ability4"];

        LeftClick = Input.actions["LeftClick"];
        LockOn = Input.actions["LockOn"];
        Move = Input.actions["Move"];
        LeftClick = Input.actions["LeftClick"];
        Escape = Input.actions["Escape"];
        B = Input.actions["B"];
        AnyKey = Input.actions["AnyKey"];
        DrawArrow = Input.actions["DrawArrow"];
        RightClickHeld = Input.actions["RightClickHeld"];

        QualityPlus = Input.actions["QualityPlus"];
        QualityMinus = Input.actions["QualityMinus"];
    }

	public void OnLook(InputValue value)
    {
        if(cursorInputForLook)
        {
            look = value.Get<Vector2>();
        }
    }


    public void OnMove(InputValue value)
    {
        move = value.Get<Vector2>();
    }

	public void OnJump(InputValue value)
    {
        jump = value.isPressed;
    }

	public void OnSprint(InputValue value)
    {
        sprint = value.isPressed;
    }
}
