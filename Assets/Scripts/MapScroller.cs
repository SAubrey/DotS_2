using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MapScroller : MonoBehaviour
{

    public CamSwitcher cs;
    private const float ARROW_TRANS = 0.5f;
    private const float MAP_MIN_X = 0;
    private const float MAP_MAX_X = 20f;
    private const float MAP_MIN_Y = 0;
    private const float MAP_MAX_Y = 20f;
    private const float BATTLE_MIN_X = -2000f;
    private const float BATTLE_MAX_X = 2100f;
    private const float BATTLE_MIN_Y = -1600f;
    private const float BATTLE_MAX_Y = 1500f;

    void Start()
    {
        cs = GameObject.Find("CamSwitcher").GetComponent<CamSwitcher>();
    }

    void Update()
    {
        if (cs.current_cam == CamSwitcher.MAP)
            CheckInput(-.3f);
        else if (cs.current_cam == CamSwitcher.BATTLE)
            CheckInput(-20f);
    }

    private void CheckInput(float scale)
    {
        if (Controller.I.RightClickHeld.phase == InputActionPhase.Performed)
        {
            //dx = Mathf.Abs(dx - Controller.I.MousePosition.ReadValue<Vector2>().x);
            //dy = Mathf.Abs(dy - Controller.I.MousePosition.ReadValue<Vector2>().y);
            var vec = Controller.I.MouseDelta.ReadValue<Vector2>();
            Translate(vec.x * scale, vec.y * scale);
            return;
        }

      /*  if (Input.GetKey(KeyCode.UpArrow))
            Translate(0, scale * -ARROW_TRANS);
        else if (Input.GetKey(KeyCode.DownArrow))
            Translate(0, scale * ARROW_TRANS);
        else if (Input.GetKey(KeyCode.LeftArrow))
            Translate(scale * ARROW_TRANS, 0);
        else if (Input.GetKey(KeyCode.RightArrow))
            Translate(scale * -ARROW_TRANS, 0); */
    }

    private void Translate(float h, float v)
    {
        float x = transform.localPosition.x;
        float y = transform.localPosition.y;
        if (cs.current_cam == CamSwitcher.MAP)
        {
            if (x + h > MAP_MAX_X || x + h < MAP_MIN_X)
                h = 0;
            if (y + v > MAP_MAX_Y || y + v < MAP_MIN_Y)
                v = 0;
        }
        else if (cs.current_cam == CamSwitcher.BATTLE)
        {
            if (x + h > BATTLE_MAX_X || x + h < BATTLE_MIN_X)
                h = 0;
            if (y + v > BATTLE_MAX_Y || y + v < BATTLE_MIN_Y)
                v = 0;
        }
        transform.Translate(h, v, 0);
    }
}
