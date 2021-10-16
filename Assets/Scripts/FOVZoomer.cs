using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVZoomer : MonoBehaviour
{
    float FOV;
    public Camera battle_cam;
    private const float SCROLL_FACTOR = .5f;
    private const float BUTTON_ZOOM_INCREMENT = 1f;
    private const int MAX_FOV = 5;
    private const int MIN_FOV = 2;
    private CamSwitcher cs;
    void Start()
    {
        cs = GameObject.Find("CamSwitcher").GetComponent<CamSwitcher>();
    }

    void Update()
    {
        if (cs.current_cam == CamSwitcher.BATTLE)
        {
            if (verify_input(battle_cam.fieldOfView + (Input.mouseScrollDelta.y * -SCROLL_FACTOR)))
                battle_cam.fieldOfView += Input.mouseScrollDelta.y * -SCROLL_FACTOR;

            if (Input.GetKey(KeyCode.Plus))
            {
                if (verify_input(battle_cam.fieldOfView + BUTTON_ZOOM_INCREMENT))
                    battle_cam.fieldOfView += BUTTON_ZOOM_INCREMENT;
            }
            else if (Input.GetKey(KeyCode.Minus))
            {
                if (verify_input(battle_cam.fieldOfView - BUTTON_ZOOM_INCREMENT))
                    battle_cam.fieldOfView -= BUTTON_ZOOM_INCREMENT;
            }
        }
    }

    private bool verify_input(float fov)
    {
        return (fov >= MIN_FOV && fov <= MAX_FOV);
    }
}