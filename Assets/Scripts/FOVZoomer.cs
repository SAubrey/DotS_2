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
    void Start()
    {
        BattleCam = CamSwitcher.I.battle_cam;
    }

    void Update()
    {
        if (CamSwitcher.I.current_cam == CamSwitcher.BATTLE)
        {
            if (VerifyInput(BattleCam.fieldOfView + (Controller.I.mouse.scroll.ReadValue().y * -ScrollFactor)))
                BattleCam.fieldOfView += Controller.I.mouse.scroll.ReadValue().y * -ScrollFactor;

            if (Controller.I.Plus.triggered)
            {
                if (VerifyInput(BattleCam.fieldOfView + ButtonZoomIncrement))
                    BattleCam.fieldOfView += ButtonZoomIncrement;
            }
            else if (Controller.I.Minus.triggered)
            {
                if (VerifyInput(BattleCam.fieldOfView - ButtonZoomIncrement))
                    BattleCam.fieldOfView -= ButtonZoomIncrement;
            }
        }
    }

    private bool VerifyInput(float fov)
    {
        return (fov >= MinFov && fov <= MaxFov);
    }
}