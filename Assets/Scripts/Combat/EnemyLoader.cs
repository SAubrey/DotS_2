using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


// This draws enemies and places them into combat slots. Enemy drawing
// is percentage based with replacement.
public class EnemyLoader : MonoBehaviour {
    public static EnemyLoader I { get; private set; }
    public static int COMMON_THRESH = 0;
    public static int UNCOMMON_THRESH = 60;
    public static int RARE_THRESH = 90;
    public static int MAX_ROLL = 100;

    // Spawning zones - 2 x,y coordinates from low to high.

    public const int T1 = 1;
    public const int T2 = 2;
    public const int T3 = 3;

    private Controller c;
    public System.Random rand;
    public Dictionary<int, List<List<List<int>>>> biomes = 
        new Dictionary<int, List<List<List<int>>>>();
    

    // Assign possible enemy spawns (by ID) per biome, per enemy tier, per spawn rate.
    //biomes[PLAINS][1][Enemy.UNCOMMON]
    public List<List<List<int>>> plains_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> forest_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> titrum_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> cliff_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> mountain_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> cave_tiers = new List<List<List<int>>>();
    public List<List<List<int>>> meld_tiers = new List<List<List<int>>>();

    
    Zone zone = new Zone(new Vector2(100, 500), new Vector2(3300, 2900));
    public List<EnemyDeployment> enemy_deployments = new List<EnemyDeployment>();
    //public List<GameObject> enemy_deployments = new List<GameObject>();
    public GameObject small_enemy_deployment_prefab;
    public GameObject field_panel;
    //public List<EnemyDeployment> enemy_melee_deployments = new List<EnemyDeployment>();
    //public List<EnemyDeployment> enemy_melee_deployments = new List<EnemyDeployment>();

    void Awake() {
        if (I == null) {
            I = this;
        } else {
            Destroy(gameObject);
        }
    }

    void Start() {
        c = GameObject.Find("Controller").GetComponent<Controller>();
        rand = new System.Random();

        make_biome(plains_tiers);
        make_biome(forest_tiers);
        make_biome(titrum_tiers);
        make_biome(cliff_tiers);
        make_biome(mountain_tiers);
        make_biome(cave_tiers);
        make_biome(meld_tiers);

        biomes.Add(MapCell.PLAINS_ID, plains_tiers);
        biomes.Add(MapCell.FOREST_ID, forest_tiers);
        biomes.Add(MapCell.TITRUM_ID, titrum_tiers);
        biomes.Add(MapCell.CLIFF_ID, cliff_tiers);
        biomes.Add(MapCell.MOUNTAIN_ID, mountain_tiers);
        biomes.Add(MapCell.CAVE_ID, cave_tiers);
        biomes.Add(MapCell.MELD_ID, meld_tiers);

        populate_biomes();
    }

    private void make_biome(List<List<List<int>>> biome_tiers) {
        for (int i = 0; i <= T3; i++) { // for each tier
            List<List<int>> rarities = new List<List<int>>();
            for (int j = 0; j <= 2; j++) { // for each rarity
                rarities.Add(new List<int>());
            }
            biome_tiers.Insert(i, rarities);
        }
    }

    private void reset() {
        remove_enemy_deployments();
    }

    public void remove_enemy_deployments() {
        foreach (EnemyDeployment ed in enemy_deployments) {
            ed.delete();
        }
        enemy_deployments.Clear();
    }

    public void generate_new_enemies(MapCell cell, int quantity) {
        for (int i = 0; i < quantity; i++) {
            int rarity = roll_rarity(); 
            int enemyID = -1;
            if (cell.ID == MapCell.RUINS_ID) {
                enemyID = pick_enemy(cell.travelcard.enemy_biome_ID, cell.tier, rarity);
            } else {
                enemyID = pick_enemy(cell.ID, cell.tier, rarity);
            }
            cell.add_enemy(Enemy.create_enemy(enemyID));
        }
        reset();
    }

    public void load_enemies(List<Enemy> enemies, int group_size) {
        // Enemies are grouped by combat style and assigned to deployments with 75/25 distribution.
        List<Enemy> melee_enemies = extract_enemies_of_type(enemies, Unit.MELEE);
        List<Enemy> range_enemies = extract_enemies_of_type(enemies, Unit.RANGE);
        int num_melee_groups = 
            (int)UnityEngine.Random.Range(melee_enemies.Count / group_size, melee_enemies.Count / 2f) + 1;
        int num_range_groups = 
            (int)UnityEngine.Random.Range(range_enemies.Count / group_size, range_enemies.Count / 2f) + 1;

        int num_enemies;
        int num_groups;
        if (melee_enemies.Count > 0) {
            // Half of groups take 3/4 of enemies. 
            num_enemies = Mathf.Max((int)(melee_enemies.Count * .75f), 1);
            num_groups = Mathf.Max((int)(num_melee_groups / 2f), 1);
            melee_enemies = spawn_deployments(melee_enemies, num_enemies, num_groups);
            
            if (num_groups > 1) {
                // Other half takes 1/4 of enemies. 
                num_enemies = melee_enemies.Count - num_enemies;
                num_groups = num_melee_groups - num_groups;
                melee_enemies = spawn_deployments(melee_enemies, num_enemies, num_groups);
                Debug.Log("Should equal zero: " + melee_enemies.Count);
            }
        }
        
        if (range_enemies.Count > 0) {
            // Again for ranged units.
            num_enemies = Mathf.Max((int)(range_enemies.Count * .75f), 1);
            num_groups = Mathf.Max((int)(num_range_groups / 2f), 1);
            range_enemies = spawn_deployments(range_enemies, num_enemies, num_groups);

            if (num_groups > 1) {
                num_enemies = range_enemies.Count - num_enemies;
                num_groups = num_range_groups - num_groups;
                range_enemies = spawn_deployments(range_enemies, num_enemies, num_groups);
            }
        }
        //reset();
    }

    // Returns the remaining enemies not placed.
    public List<Enemy> spawn_deployments(List<Enemy> enemies, int num_enemies, int num_groups) {
        int enemies_per_group = (int)(num_enemies / num_groups);
        GameObject ed;
        EnemyDeployment ed_script;

        int enemies_placed = 0;
        for (int i = 0; i < num_groups; i++) {
            ed = Instantiate(small_enemy_deployment_prefab);
            ed_script = ed.GetComponentInChildren<SmallEnemyDeployment>();
            for (int j = 0; j < enemies_per_group; j++) {
                ed_script.place_unit(enemies[(i * enemies_per_group) + j]);
                enemies_placed++;
            }
            enemy_deployments.Add(ed_script);
            place_deployment(ed_script, zone);
        }
        // Remove placed enemies.
        enemies.RemoveRange(0, enemies_placed);
        return enemies;
    }

    public void place_deployment(EnemyDeployment ed_script, Zone zone) {
        ed_script.gameObject.transform.parent.transform.SetParent(field_panel.transform);
        float x = UnityEngine.Random.Range(zone.low.x, zone.high.x);
        float y = UnityEngine.Random.Range(zone.low.y, zone.high.y);
        ed_script.gameObject.transform.parent.position = new Vector3(x, y, 0);
    }

    public List<Enemy> extract_enemies_of_type(List<Enemy> enemies, int type) {
        List<Enemy> units = new List<Enemy>();
        foreach (Enemy e in enemies) {
            if (e.combat_style == type) {
                units.Add(e);
            }
        }
        return units;
    }

    public int count_num_enemies_of_type(List<Enemy> enemies, int type) {
        int count = 0;
        foreach (Enemy e in enemies) {
            if (e.combat_style == type) {
                count++;
            }
        }
        return count;
    }

    private int roll_rarity() {
        int rarity = rand.Next(0, MAX_ROLL);
        if (rarity < UNCOMMON_THRESH) 
            return Enemy.COMMON;
        else if (rarity < RARE_THRESH)
            return Enemy.UNCOMMON;
        else
            return Enemy.RARE;
    }

    private int pick_enemy(int biome, int tier, int rarity) {
        List<int> candidates = biomes[biome][tier][rarity];

        // Try lower rarities if one is missing. (There should always be a common)
        for (int i = 0; i < 3; i++) {
            candidates = biomes[biome][tier][rarity];
            if (candidates.Count > 0)
                break;
            rarity--;
        }
        int r = rand.Next(0, candidates.Count);
        //Debug.Log("candidates: " + candidates.Count + ". " +
             //biome + ", " + tier + ", " + rarity + ", " + r);
        return biomes[biome][tier][rarity][r];
    }

    private void slot_enemy(Enemy enemy) {
        //Zone zone = get_appropriate_zone(enemy);
        //if (fill_slot(enemy, zone.get_spawn_pos()))
          //  zone.increment_pos();
    }

    private bool spawn_left = false;
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

    private void populate_biomes() {
        biomes[MapCell.PLAINS_ID][T1][Enemy.COMMON].Add(Enemy.GALTSA);
        biomes[MapCell.PLAINS_ID][T1][Enemy.COMMON].Add(Enemy.GREM);
        biomes[MapCell.PLAINS_ID][T1][Enemy.UNCOMMON].Add(Enemy.ENDU);
        biomes[MapCell.PLAINS_ID][T1][Enemy.COMMON].Add(Enemy.KOROTE);
        biomes[MapCell.PLAINS_ID][T2][Enemy.COMMON].Add(Enemy.MOLNER);
        biomes[MapCell.PLAINS_ID][T2][Enemy.COMMON].Add(Enemy.ETUENA);
        biomes[MapCell.PLAINS_ID][T2][Enemy.UNCOMMON].Add(Enemy.CLYPTE);
        biomes[MapCell.PLAINS_ID][T2][Enemy.RARE].Add(Enemy.GOLIATH);
        biomes[MapCell.PLAINS_ID][T3][Enemy.COMMON].Add(Enemy.GALTSA); //false

        biomes[MapCell.FOREST_ID][T1][Enemy.COMMON].Add(Enemy.KVERM);
        biomes[MapCell.FOREST_ID][T1][Enemy.UNCOMMON].Add(Enemy.LATU);
        biomes[MapCell.FOREST_ID][T1][Enemy.COMMON].Add(Enemy.EKE_TU);
        biomes[MapCell.FOREST_ID][T1][Enemy.COMMON].Add(Enemy.OETEM);
        biomes[MapCell.FOREST_ID][T2][Enemy.COMMON].Add(Enemy.EKE_FU);
        biomes[MapCell.FOREST_ID][T2][Enemy.UNCOMMON].Add(Enemy.EKE_SHI_AMI);
        biomes[MapCell.FOREST_ID][T2][Enemy.RARE].Add(Enemy.EKE_LORD);
        biomes[MapCell.FOREST_ID][T2][Enemy.UNCOMMON].Add(Enemy.KETEMCOL);
        biomes[MapCell.FOREST_ID][T3][Enemy.COMMON].Add(Enemy.KVERM);//false

        biomes[MapCell.TITRUM_ID][T1][Enemy.COMMON].Add(Enemy.MAHUKIN);
        biomes[MapCell.TITRUM_ID][T1][Enemy.UNCOMMON].Add(Enemy.DRONGO);
        biomes[MapCell.TITRUM_ID][T2][Enemy.COMMON].Add(Enemy.MAHEKET);
        biomes[MapCell.TITRUM_ID][T2][Enemy.UNCOMMON].Add(Enemy.CALUTE);
        biomes[MapCell.TITRUM_ID][T2][Enemy.UNCOMMON].Add(Enemy.ETALKET);
        biomes[MapCell.TITRUM_ID][T2][Enemy.RARE].Add(Enemy.MUATEM);
        biomes[MapCell.TITRUM_ID][T3][Enemy.COMMON].Add(Enemy.MAHUKIN);//false

        biomes[MapCell.MOUNTAIN_ID][T2][Enemy.COMMON].Add(Enemy.DRAK);
        biomes[MapCell.MOUNTAIN_ID][T2][Enemy.COMMON].Add(Enemy.ZERRKU);
        biomes[MapCell.MOUNTAIN_ID][T2][Enemy.COMMON].Add(Enemy.GOKIN);
        biomes[MapCell.MOUNTAIN_ID][T3][Enemy.COMMON].Add(Enemy.DRAK);// false?
        biomes[MapCell.MOUNTAIN_ID][T3][Enemy.COMMON].Add(Enemy.ZERRKU);
        biomes[MapCell.MOUNTAIN_ID][T3][Enemy.COMMON].Add(Enemy.GOKIN);

        biomes[MapCell.CLIFF_ID][T1][Enemy.COMMON].Add(Enemy.DRAK);
        biomes[MapCell.CLIFF_ID][T1][Enemy.COMMON].Add(Enemy.ZERRKU);
        biomes[MapCell.CLIFF_ID][T1][Enemy.COMMON].Add(Enemy.GOKIN);
        biomes[MapCell.CLIFF_ID][T2][Enemy.COMMON].Add(Enemy.DRAK);
        biomes[MapCell.CLIFF_ID][T2][Enemy.COMMON].Add(Enemy.ZERRKU);
        biomes[MapCell.CLIFF_ID][T2][Enemy.COMMON].Add(Enemy.GOKIN);
        biomes[MapCell.CLIFF_ID][T3][Enemy.COMMON].Add(Enemy.DRAK);//false
        biomes[MapCell.CLIFF_ID][T3][Enemy.COMMON].Add(Enemy.ZERRKU);
        biomes[MapCell.CLIFF_ID][T3][Enemy.COMMON].Add(Enemy.GOKIN);

        biomes[MapCell.CAVE_ID][T1][Enemy.COMMON].Add(Enemy.TAJAQAR);
        biomes[MapCell.CAVE_ID][T1][Enemy.COMMON].Add(Enemy.TAJAERO);
        biomes[MapCell.CAVE_ID][T1][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        biomes[MapCell.CAVE_ID][T1][Enemy.UNCOMMON].Add(Enemy.DUALE);
        biomes[MapCell.CAVE_ID][T2][Enemy.COMMON].Add(Enemy.TAJAQAR);
        biomes[MapCell.CAVE_ID][T2][Enemy.COMMON].Add(Enemy.TAJAERO);
        biomes[MapCell.CAVE_ID][T2][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        biomes[MapCell.CAVE_ID][T2][Enemy.UNCOMMON].Add(Enemy.DUALE);
        biomes[MapCell.CAVE_ID][T3][Enemy.COMMON].Add(Enemy.TAJAQAR);//false
        biomes[MapCell.CAVE_ID][T3][Enemy.COMMON].Add(Enemy.TAJAERO);
        biomes[MapCell.CAVE_ID][T3][Enemy.RARE].Add(Enemy.TERRA_QUAL);
        biomes[MapCell.CAVE_ID][T3][Enemy.UNCOMMON].Add(Enemy.DUALE);

        biomes[MapCell.MELD_ID][T1][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        biomes[MapCell.MELD_ID][T1][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
        biomes[MapCell.MELD_ID][T2][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        biomes[MapCell.MELD_ID][T2][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
        biomes[MapCell.MELD_ID][T3][Enemy.COMMON].Add(Enemy.MELD_WARRIOR);
        biomes[MapCell.MELD_ID][T3][Enemy.COMMON].Add(Enemy.MELD_SPEARMAN);
    }
}

public class Zone {
    public Vector2 low;
    public Vector2 high;
    //public Pos current_pos;
    int enemies_slotted = 0;

    public Zone(Vector2 low, Vector2 high) {
        this.low = low;
        this.high = high;
    }

    public Vector2 get_spawn_pos() {
        float x = UnityEngine.Random.Range(low.x, high.x);
        float y = UnityEngine.Random.Range(low.y, high.y);
        return new Vector2(x, y);
    }

    public void reset() {
        low = Vector2.zero;
        high = Vector2.zero;
    }
}
