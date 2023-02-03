using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This draws enemies and places them into combat slots. Enemy drawing
// is percentage based with replacement.
public class EnemyLoader : MonoBehaviour
{
    public static EnemyLoader I { get; private set; }
    public static int ThreshUncommon = 60;
    public static int TreshRare = 90;
    public static int MaxRoll = 100;

    public const int T1 = 1;
    public const int T2 = 2;
    public const int T3 = 3;

    public SpawnZone SpawnZone;
    public List<Slot> EnemySlots = new List<Slot>();
    public GameObject SingleEnemySlotPrefab;
    public GameObject DeploymentParent;
    public List<Enemy> Enemies = new List<Enemy>();

    private System.Random Rand;
    private Dictionary<int, List<List<List<int>>>> Biomes =
        new Dictionary<int, List<List<List<int>>>>();


    // Assign possible enemy spawns (by ID) per biome, per enemy tier, per spawn rate.
    //biomes[PLAINS][1][Enemy.UNCOMMON]
    private List<List<List<int>>> PlainsTiers = new List<List<List<int>>>();
    private List<List<List<int>>> ForestTiers = new List<List<List<int>>>();
    private List<List<List<int>>> TitrumTiers = new List<List<List<int>>>();
    private List<List<List<int>>> CliffTiers = new List<List<List<int>>>();
    private List<List<List<int>>> MountainTiers = new List<List<List<int>>>();
    private List<List<List<int>>> CaveTiers = new List<List<List<int>>>();
    private List<List<List<int>>> MeldTiers = new List<List<List<int>>>();

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
        Rand = new System.Random();

        MakeBiome(PlainsTiers);
        MakeBiome(ForestTiers);
        MakeBiome(TitrumTiers);
        MakeBiome(CliffTiers);
        MakeBiome(MountainTiers);
        MakeBiome(CaveTiers);
        MakeBiome(MeldTiers);

        Biomes.Add(MapCell.IDPlains, PlainsTiers);
        Biomes.Add(MapCell.IDForest, ForestTiers);
        Biomes.Add(MapCell.IDTitrum, TitrumTiers);
        Biomes.Add(MapCell.IDCliff, CliffTiers);
        Biomes.Add(MapCell.IDMountain, MountainTiers);
        Biomes.Add(MapCell.IDCave, CaveTiers);
        Biomes.Add(MapCell.IDMeld, MeldTiers);

        PopulateBiomes();
    }

    private void MakeBiome(List<List<List<int>>> biomeTiers)
    {
        for (int i = 0; i <= T3; i++)
        { // for each tier
            List<List<int>> rarities = new List<List<int>>();
            for (int j = 0; j <= 2; j++)
            { // for each rarity
                rarities.Add(new List<int>());
            }
            biomeTiers.Insert(i, rarities);
        }
    }

    private void Reset()
    {
        RemoveEnemySlots();
    }

    public void RemoveEnemySlots()
    {
        foreach (Slot slot in EnemySlots)
        {
            slot.Empty();
        }
        EnemySlots.Clear();
    }

    public void GenerateNewEnemies(MapCell cell, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            int rarity = RollRarity();
            int enemyID = -1;
            if (cell.ID == MapCell.IDRuins)
            {
                enemyID = PickEnemy(cell.Travelcard.EnemyBiomeID, cell.Tier, rarity);
            }
            else
            {
                enemyID = PickEnemy(cell.ID, cell.Tier, rarity);
            }
            cell.AddEnemy(Enemy.CreateEnemy(enemyID));
        }
        //reset();
    }

    public void SpawnEnemies(List<Enemy> enemies)
    {
        Enemies.AddRange(enemies);

        GameObject ed;
        Slot eScript;

        for (int i = 0; i < enemies.Count; i++)
        {
            ed = Instantiate(SingleEnemySlotPrefab, SpawnZone.GetSpawnPos(), Quaternion.identity, DeploymentParent.transform);

            Group g = ed.GetComponent<Group>();
            g.SlotParent = DeploymentParent;
            g.SpawnSlots();

            //eScript = ed.GetComponentInChildren<Slot>();
            eScript = g.Slots[0];
            eScript.Fill(enemies[i]);
            EnemySlots.Add(eScript);
        }
    }

    private int RollRarity()
    {
        int rarity = Rand.Next(0, MaxRoll);
        if (rarity < ThreshUncommon)
            return Enemy.COMMON;
        else if (rarity < TreshRare)
            return Enemy.UNCOMMON;
        else
            return Enemy.RARE;
    }

    private int PickEnemy(int biome, int tier, int rarity)
    {
        List<int> candidates = Biomes[biome][tier][rarity];

        // Try lower rarities if one is missing. (There should always be a common)
        for (int i = 0; i < 3; i++)
        {
            candidates = Biomes[biome][tier][rarity];
            if (candidates.Count > 0)
                break;
            rarity--;
        }
        int r = Rand.Next(0, candidates.Count);
        //Debug.Log("candidates: " + candidates.Count + ". " +
        //biome + ", " + tier + ", " + rarity + ", " + r);
        return Biomes[biome][tier][rarity][r];
    }

    private void PopulateBiomes()
    {
        Biomes[MapCell.IDPlains][T1][Enemy.COMMON].Add(Enemy.GALTSA);
        Biomes[MapCell.IDPlains][T1][Enemy.COMMON].Add(Enemy.GREM);
        Biomes[MapCell.IDPlains][T1][Enemy.UNCOMMON].Add(Enemy.ENDU);
        Biomes[MapCell.IDPlains][T1][Enemy.COMMON].Add(Enemy.KOROTE);
        Biomes[MapCell.IDPlains][T2][Enemy.COMMON].Add(Enemy.MOLNER);
        Biomes[MapCell.IDPlains][T2][Enemy.COMMON].Add(Enemy.ETUENA);
        Biomes[MapCell.IDPlains][T2][Enemy.UNCOMMON].Add(Enemy.CLYPTE);
        Biomes[MapCell.IDPlains][T2][Enemy.RARE].Add(Enemy.GOLIATH);
        Biomes[MapCell.IDPlains][T3][Enemy.COMMON].Add(Enemy.GALTSA); //false

        Biomes[MapCell.IDForest][T1][Enemy.COMMON].Add(Enemy.KVERM);
        Biomes[MapCell.IDForest][T1][Enemy.UNCOMMON].Add(Enemy.LATU);
        Biomes[MapCell.IDForest][T1][Enemy.COMMON].Add(Enemy.EKE_TU);
        Biomes[MapCell.IDForest][T1][Enemy.COMMON].Add(Enemy.OETEM);
        Biomes[MapCell.IDForest][T2][Enemy.COMMON].Add(Enemy.EKE_FU);
        Biomes[MapCell.IDForest][T2][Enemy.UNCOMMON].Add(Enemy.EKE_SHI_AMI);
        Biomes[MapCell.IDForest][T2][Enemy.RARE].Add(Enemy.EKE_LORD);
        Biomes[MapCell.IDForest][T2][Enemy.UNCOMMON].Add(Enemy.KETEMCOL);
        Biomes[MapCell.IDForest][T3][Enemy.COMMON].Add(Enemy.KVERM);//false

        Biomes[MapCell.IDTitrum][T1][Enemy.COMMON].Add(Enemy.MAHUKIN);
        Biomes[MapCell.IDTitrum][T1][Enemy.UNCOMMON].Add(Enemy.DRONGO);
        Biomes[MapCell.IDTitrum][T2][Enemy.COMMON].Add(Enemy.MAHEKET);
        Biomes[MapCell.IDTitrum][T2][Enemy.UNCOMMON].Add(Enemy.CALUTE);
        Biomes[MapCell.IDTitrum][T2][Enemy.UNCOMMON].Add(Enemy.ETALKET);
        Biomes[MapCell.IDTitrum][T2][Enemy.RARE].Add(Enemy.MUATEM);
        Biomes[MapCell.IDTitrum][T3][Enemy.COMMON].Add(Enemy.MAHUKIN);//false

        Biomes[MapCell.IDMountain][T2][Enemy.COMMON].Add(Enemy.DRAK);
        Biomes[MapCell.IDMountain][T2][Enemy.COMMON].Add(Enemy.ZERRKU);
        Biomes[MapCell.IDMountain][T2][Enemy.COMMON].Add(Enemy.GOKIN);
        Biomes[MapCell.IDMountain][T3][Enemy.COMMON].Add(Enemy.DRAK);// false?
        Biomes[MapCell.IDMountain][T3][Enemy.COMMON].Add(Enemy.ZERRKU);
        Biomes[MapCell.IDMountain][T3][Enemy.COMMON].Add(Enemy.GOKIN);

        Biomes[MapCell.IDCliff][T1][Enemy.COMMON].Add(Enemy.DRAK);
        Biomes[MapCell.IDCliff][T1][Enemy.COMMON].Add(Enemy.ZERRKU);
        Biomes[MapCell.IDCliff][T1][Enemy.COMMON].Add(Enemy.GOKIN);
        Biomes[MapCell.IDCliff][T2][Enemy.COMMON].Add(Enemy.DRAK);
        Biomes[MapCell.IDCliff][T2][Enemy.COMMON].Add(Enemy.ZERRKU);
        Biomes[MapCell.IDCliff][T2][Enemy.COMMON].Add(Enemy.GOKIN);
        Biomes[MapCell.IDCliff][T3][Enemy.COMMON].Add(Enemy.DRAK);//false
        Biomes[MapCell.IDCliff][T3][Enemy.COMMON].Add(Enemy.ZERRKU);
        Biomes[MapCell.IDCliff][T3][Enemy.COMMON].Add(Enemy.GOKIN);

        Biomes[MapCell.IDCave][T1][Enemy.COMMON].Add(Enemy.TAJAQAR);
        Biomes[MapCell.IDCave][T1][Enemy.COMMON].Add(Enemy.TAJAERO);
        Biomes[MapCell.IDCave][T1][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        Biomes[MapCell.IDCave][T1][Enemy.UNCOMMON].Add(Enemy.DUALE);
        Biomes[MapCell.IDCave][T2][Enemy.COMMON].Add(Enemy.TAJAQAR);
        Biomes[MapCell.IDCave][T2][Enemy.COMMON].Add(Enemy.TAJAERO);
        Biomes[MapCell.IDCave][T2][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        Biomes[MapCell.IDCave][T2][Enemy.UNCOMMON].Add(Enemy.DUALE);
        Biomes[MapCell.IDCave][T3][Enemy.COMMON].Add(Enemy.TAJAQAR);//false
        Biomes[MapCell.IDCave][T3][Enemy.COMMON].Add(Enemy.TAJAERO);
        Biomes[MapCell.IDCave][T3][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        Biomes[MapCell.IDCave][T3][Enemy.UNCOMMON].Add(Enemy.DUALE);

        Biomes[MapCell.IDMeld][T1][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        Biomes[MapCell.IDMeld][T1][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
        Biomes[MapCell.IDMeld][T2][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        Biomes[MapCell.IDMeld][T2][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
        Biomes[MapCell.IDMeld][T3][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        Biomes[MapCell.IDMeld][T3][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
    }
}
