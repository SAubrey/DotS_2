using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVZoomer : MonoBehaviour
{
    [SerializeField] private const float ScrollFactor = .5f;
    [SerializeField] private const float ButtonZoomIncrement = 1f;
    [SerializeField] private int MaxFov = 30;
    [SerializeField] private int MinFov = 10;
    public Camera BattleCam;
    private CamSwitcher cs;
    void Start()
    {
        cs = GameObject.Find("CamSwitcher").GetComponent<CamSwitcher>();
    }

    void Update()
    {
        if (cs.current_cam == CamSwitcher.BATTLE)
        {
            if (verify_input(BattleCam.fieldOfView + (Input.mouseScrollDelta.y * -ScrollFactor)))
                BattleCam.fieldOfView += Input.mouseScrollDelta.y * -ScrollFactor;

            if (Input.GetKey(KeyCode.Plus))
            {
                if (verify_input(BattleCam.fieldOfView + ButtonZoomIncrement))
                    BattleCam.fieldOfView += ButtonZoomIncrement;
            }
            else if (Input.GetKey(KeyCode.Minus))
            {
                if (verify_input(BattleCam.fieldOfView - ButtonZoomIncrement))
                    BattleCam.fieldOfView -= ButtonZoomIncrement;
            }
        }
    }

    private bool verify_input(float fov)
    {
        return (fov >= MinFov && fov <= MaxFov);
    }
}