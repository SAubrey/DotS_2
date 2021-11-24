using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


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

    public System.Random Rand;
    public Dictionary<int, List<List<List<int>>>> Biomes =
        new Dictionary<int, List<List<List<int>>>>();


    // Assign possible enemy spawns (by ID) per biome, per enemy tier, per spawn rate.
    //biomes[PLAINS][1][Enemy.UNCOMMON]
    public List<List<List<int>>> PlainsTiers = new List<List<List<int>>>();
    public List<List<List<int>>> ForestTiers = new List<List<List<int>>>();
    public List<List<List<int>>> TitrumTiers = new List<List<List<int>>>();
    public List<List<List<int>>> CliffTiers = new List<List<List<int>>>();
    public List<List<List<int>>> MountainTiers = new List<List<List<int>>>();
    public List<List<List<int>>> CaveTiers = new List<List<List<int>>>();
    public List<List<List<int>>> MeldTiers = new List<List<List<int>>>();

    public SpawnZone SpawnZone;
    public List<EnemyDeployment> EnemyDeployments = new List<EnemyDeployment>();
    public GameObject SmallEnemyDeploymentPrefab;
    public GameObject DeploymentParent;
    public List<Enemy> Enemies = new List<Enemy>();

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
        RemoveEnemyDeployments();
    }

    public void RemoveEnemyDeployments()
    {
        foreach (EnemyDeployment ed in EnemyDeployments)
        {
            ed.Delete();
        }
        EnemyDeployments.Clear();
    }

    public void GenerateNewEnemies(MapCell cell, int quantity)
    {
        for (int i = 0; i < quantity; i++)
        {
            int rarity = RollRarity();
            int enemyID = -1;
            if (cell.ID == MapCell.IDRuins)
            {
                enemyID = PickEnemy(cell.Travelcard.enemy_biome_ID, cell.Tier, rarity);
            }
            else
            {
                enemyID = PickEnemy(cell.ID, cell.Tier, rarity);
            }
            cell.AddEnemy(Enemy.CreateEnemy(enemyID));
        }
        //reset();
    }

    public void LoadEnemies(List<Enemy> enemies, int groupSize)
    {
        // Enemies are grouped by combat style and assigned to deployments with 75/25 distribution.
        List<Enemy> meleeEnemies = ExtractEnemiesOfType(enemies, true);
        List<Enemy> rangeEnemies = ExtractEnemiesOfType(enemies, false);
        int numMeleeGroups =
            (int)UnityEngine.Random.Range(meleeEnemies.Count / groupSize, meleeEnemies.Count / 2f) + 1;
        int numRangeGroups =
            (int)UnityEngine.Random.Range(rangeEnemies.Count / groupSize, rangeEnemies.Count / 2f) + 1;

        DistributeEnemiesToGroups(meleeEnemies, numMeleeGroups);
        DistributeEnemiesToGroups(rangeEnemies, numRangeGroups);
    }

    public void DistributeEnemiesToGroups(List<Enemy> enemies, int numGroups)
    {
        if (enemies.Count <= 0 || numGroups <= 0)
        {
            Debug.Log("ERROR: CANNOT TAKE 0 AS INPUT");
            return;
        }
        if (numGroups == 1)
        {
            Debug.Log("Spawning a single group w/ enemies: " + enemies.Count);
            SpawnDeployments(enemies, enemies.Count, numGroups);
            return;
        }
        // Half of groups take 3/4 of enemies. 
        int num_enemies = Mathf.Max((int)(enemies.Count * .75f), 1);
        int num_some_groups = Mathf.Max((int)(numGroups / 2f), 1);
        Debug.Log("num groups: " + numGroups + "num_some_groups: " + num_some_groups);
        enemies = SpawnDeployments(enemies, num_enemies, num_some_groups);

        if (enemies.Count > 0)
        {
            // Other half takes 1/4 of enemies. 
            num_some_groups = numGroups - num_some_groups;
            enemies = SpawnDeployments(enemies, enemies.Count, num_some_groups);
            Debug.Log("Should equal zero: " + enemies.Count);
        }
    }

    // Returns the remaining enemies not placed.
    public List<Enemy> SpawnDeployments(List<Enemy> enemies, int numEnemies, int numGroups)
    {
        Enemies.AddRange(enemies);

        int enemiesPerGroup = (int)(numEnemies / numGroups);
        GameObject ed;
        EnemyDeployment edScript;

        int enemiesPlaced = 0;
        for (int i = 0; i < numGroups; i++)
        {
            ed = Instantiate(SmallEnemyDeploymentPrefab, SpawnZone.GetSpawnPos(), Quaternion.identity, DeploymentParent.transform);

            Group g = ed.GetComponentInChildren<Group>();
            g.SlotParent = DeploymentParent;
            g.SpawnSlots();

            edScript = ed.GetComponentInChildren<SmallEnemyDeployment>();
            for (int j = 0; j < enemiesPerGroup; j++)
            {
                edScript.PlaceUnit(enemies[(i * enemiesPerGroup) + j]);
                enemiesPlaced++;
            }
            EnemyDeployments.Add(edScript);
            //SpawnZone.PlaceDeployment(ed, DeploymentParent);
        }
        // Remove placed enemies.
        Debug.Log("before: " + enemies.Count);
        enemies.RemoveRange(0, enemiesPlaced);
        Debug.Log("after: " + enemies.Count);
        return enemies;
    }

    public List<Enemy> ExtractEnemiesOfType(List<Enemy> enemies, bool isMelee)
    {
        List<Enemy> units = new List<Enemy>();
        foreach (Enemy e in enemies)
        {
            if (e.IsMelee == isMelee)
            {
                units.Add(e);
            }
        }
        return units;
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

    private void SlotEnemy(Enemy enemy)
    {
        //Zone zone = get_appropriate_zone(enemy);
        //if (fill_slot(enemy, zone.get_spawn_pos()))
        //  zone.increment_pos();
    }

    //private Zone get_appropriate_zone(Enemy enemy) {
    //}

    /*  private bool fill_slot(Enemy enemy, Pos pos) {
          //Debug.Log(pos.x + " : " + pos.y);
          //Formation.I.check_groups();
          Group g = Formation.I.get_group(pos.x, pos.y);
          if (g == null) {
              //Debug.Log("null group")
          }
          Slot s = Formation.I.get_group(pos.x, pos.y).get_highest_empty_slot();
          if (s != null) {
              s.fill(enemy);
              return true;
          }
          return false;
      }*/

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
