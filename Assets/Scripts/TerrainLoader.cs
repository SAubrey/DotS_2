using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerrainLoader : MonoBehaviour
{
    public static TerrainLoader I { get; private set; }
    //public Dictionary<int, Terrain> Terrains = new Dictionary<int, Terrain>();
    public Dictionary<int, GameObject> Terrains = new Dictionary<int, GameObject>();
    public GameObject Plains, Forest, Titrum;

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
        foreach (GameObject t in Terrains.Values) 
        {
            t.SetActive(false);
        }
        Plains.SetActive(true);
        return;
        //Terrains[TerrainID].enabled = true;
        if (Terrains.ContainsKey(TerrainID))
        {
            Terrains[TerrainID].SetActive(true);
        } else
        {
            Plains.SetActive(true);
        }

    }
}
