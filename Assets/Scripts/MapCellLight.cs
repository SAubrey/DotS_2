using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCellLight : MonoBehaviour
{
    public UnityEngine.Experimental.Rendering.Universal.Light2D CellLight;
    public LightFlicker LightFlicker;
    public LightGlow LightGlow;

    public void Init(MapCell cell)
    {
        if (LightFlicker == null && LightGlow == null)
            return;
        LightFlicker.light2d = CellLight;
        LightGlow.light2d = CellLight;

        if (MapUI.I.CellLightColors.ContainsKey(cell.ID))
            SetColor(MapUI.I.CellLightColors[cell.ID]);
        SetGlow(cell.Glows);
        SetFlicker(cell.Flickers);
    }

    public void SetGlow(bool active)
    {
        LightGlow.active = active;
    }

    public void SetFlicker(bool active)
    {
        LightFlicker.active = active;
    }

    public void SetColor(Color c)
    {
        CellLight.color = c;
    }
}
