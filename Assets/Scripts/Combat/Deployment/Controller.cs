using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class Controller : MonoBehaviour
{
    public static Controller I { get; private set; }
    private PlayerInput Input;
    public InputAction Attack;
    public InputAction Block;
    public InputAction MoveForwardSwordsmen;
    public InputAction MoveForwardPolearm;
    public InputAction MoveForwardRanger;
    public InputAction MoveForwardMage;
    public InputAction Ability1, Ability2, Ability3, Ability4;
    public InputAction FireArrow;
    public InputAction LockOn;
    public InputAction Move;
    public InputAction Escape;
    public InputAction B;
    public InputAction MousePosition;
    public InputAction AnyKey;
    public InputAction Plus, Minus;
    public Mouse mouse;
    public InputAction LeftClickHeld, RightClickHeld;
    public InputAction MouseDelta;
    public InputAction QualityPlus, QualityMinus;
    
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
        MouseDelta = Input.actions["MouseDelta"];
        Attack = Input.actions["Attack"];
        Block = Input.actions["Block"];
        MoveForwardSwordsmen = Input.actions["MoveForwardSwordsmen"];
        MoveForwardPolearm = Input.actions["MoveForwardPolearm"];
        MoveForwardRanger = Input.actions["MoveForwardRanger"];
        MoveForwardMage = Input.actions["MoveForwardMage"];
        Ability1 = Input.actions["Ability1"];
        Ability2 = Input.actions["Ability2"];
        Ability3 = Input.actions["Ability3"];
        Ability4 = Input.actions["Ability4"];

        FireArrow = Input.actions["FireArrow"];
        LockOn = Input.actions["LockOn"];
        Move = Input.actions["Move"];
        Escape = Input.actions["Escape"];
        B = Input.actions["B"];
        AnyKey = Input.actions["AnyKey"];
        LeftClickHeld = Input.actions["LeftClickHeld"];
        RightClickHeld = Input.actions["RightClickHeld"];

        QualityPlus = Input.actions["QualityPlus"];
        QualityMinus = Input.actions["QualityMinus"];
    }


}
