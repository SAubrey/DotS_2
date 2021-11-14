using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainLoader : MonoBehaviour
{
    public static TerrainLoader I { get; private set; }
    public Dictionary<int, Terrain> Terrains = new Dictionary<int, Terrain>();
    public Terrain Plains, Forest, Titrum;

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
        Terrains.Add(MapCell.IDPlains, Plains);
        Terrains.Add(MapCell.IDForest, Forest);
        Terrains.Add(MapCell.IDTitrum, Titrum);
    }

    public void Load(int TerrainID)
    {
        foreach (Terrain t in Terrains.Values) 
        {
            t.enabled = false;
        }
        //Terrains[TerrainID].enabled = true;
        if (Terrains.ContainsKey(TerrainID))
        {
            Terrains[TerrainID].enabled = true;
        } else
        {
            Plains.enabled = true;
        }

    }
}
