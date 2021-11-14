using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class City : Storeable
{
    public const int CITY = 3;

    protected override void Start()
    {
        base.Start();
        Resources[LIGHT] = 8;
        Resources[UNITY] = 3;
        ID = CITY;
        Name = "City";
    }

    public override void LightDecayCascade()
    {
        Dictionary<string, int> d = new Dictionary<string, int>();
        d.Add(LIGHT, -1);
        if (Resources[LIGHT] <= 0)
        {
            if (Resources[STAR_CRYSTALS] > 0)
            {
                d.Add(STAR_CRYSTALS, -1);
                d[LIGHT] = LightRefreshAmount;
            }
            else if (Resources[UNITY] > 0)
            {
                d.Add(UNITY, -1);
                d[LIGHT] = LightRefreshAmount;
            }
            else if (Resources[UNITY] <= 0)
            {
                // Game over
                // Raise game over window, game over accept button calls game over in Controller.
                MapUI.I.SetActiveGameOverP(true);
            }
        }
        ShowAdjustments(d);
    }
    public override void Init(bool from_save)
    {
        base.Init(from_save);
        LightRefreshAmount = 8;
    }

    public override GameData Save()
    {
        return new CityData(this, Name);
    }

    public override void Load(GameData generic)
    {
        CityData data = generic as CityData;
        Resources[LIGHT] = data.sresources.light;
        Resources[UNITY] = data.sresources.unity;
        Resources[STAR_CRYSTALS] = data.sresources.star_crystals;
        Resources[MINERALS] = data.sresources.minerals;
        Resources[ARELICS] = data.sresources.arelics;
        Resources[ERELICS] = data.sresources.erelics;
        Resources[MRELICS] = data.sresources.mrelics;

        CityUI cui = CityUI.I;
        for (int i = 0; i < cui.upgrades.Count; i++)
        {
            cui.selected_upgrade_ID = cui.upgrades[i].ID;
            cui.PurchaseUpgrade();
        }
    }
}