using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FOVZoomer : MonoBehaviour
{
    [SerializeField] private const float ScrollFactor = .5f;
    [SerializeField] private const float ButtonZoomIncrement = 1f;
    [SerializeField] private int MaxFov = 30;
    [SerializeField] private int MinFov = 10;
    public Cinemachine.CinemachineVirtualCamera BattleCam;
    void Start()
    {
        BattleCam = CamSwitcher.I.BattleCam;
    }

    void Update()
    {
        if (CamSwitcher.I.current_cam == CamSwitcher.BATTLE)
        {
            if (VerifyInput(BattleCam.m_Lens.FieldOfView + (Controller.I.mouse.scroll.ReadValue().y * -ScrollFactor)))
                BattleCam.m_Lens.FieldOfView = Controller.I.mouse.scroll.ReadValue().y * -ScrollFactor;
            if (Controller.I.Plus.triggered)
            {
                if (VerifyInput(BattleCam.m_Lens.FieldOfView + ButtonZoomIncrement))
                    BattleCam.m_Lens.FieldOfView += ButtonZoomIncrement;
            }
            else if (Controller.I.Minus.triggered)
            {
                if (VerifyInput(BattleCam.m_Lens.FieldOfView - ButtonZoomIncrement))
                    BattleCam.m_Lens.FieldOfView -= ButtonZoomIncrement;
            }
        }
    }

    private bool VerifyInput(float fov)
    {
        return (fov >= MinFov && fov <= MaxFov);
    }
}