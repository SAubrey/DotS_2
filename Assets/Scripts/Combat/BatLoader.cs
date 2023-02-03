using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/*
Battalion loader.
Pertains to the player unit placement selection bar and placed player/enemy unit images for slots.
*/
public class BatLoader : MonoBehaviour
{
    public static BatLoader I { get; private set; }
    //public Sprite white_fade_img, dark_fade_img;
    public Sprite Empty; // UIMask image for a slot button image.
    // Unit quantity text fields in the unit selection scrollbar.
    public Dictionary<int, TextMeshProUGUI> Texts = new Dictionary<int, TextMeshProUGUI>();
    public TextMeshProUGUI TWarrior, spearman_t, archer_t, miner_t, inspirator_t, seeker_t,
        guardian_t, arbalest_t, skirmisher_t, paladin_t, mender_t, carter_t, dragoon_t,
        scout_t, drummer_t, shield_maiden_t, pikeman_t;


    // Button images in battle scene for highlighting selections.
    public IDictionary<int, Button> UnitButtons = new Dictionary<int, Button>();
    public Button archer_B, warrior_B, spearman_B, inspirator_B, miner_B,
        seeker_B, guardian_B, arbalest_B, skirmisher_B, paladin_B, mender_B, carter_B, dragoon_B,
        scout_B, drummer_B, shield_maiden_B, pikeman_B;


    // Drawn unit images.
    // Player units
    public Dictionary<int, Sprite> UnitImagesBack = new Dictionary<int, Sprite>();
    public Sprite warrior_b, spearman_b, archer_b, miner_b, inspirator_b, seeker_b,
        guardian_b, arbalest_b, skirmisher_b, paladin_b, mender_b, carter_b, dragoon_b,
        scout_b, drummer_b, shield_maiden_b, pikeman_b;
    public Dictionary<int, Sprite> UnitImagesFront = new Dictionary<int, Sprite>();
    public Sprite warrior_f, spearman_f, archer_f, miner_f, inspirator_f, seeker_f,
        guardian_f, arbalest_f, skirmisher_f, paladin_f, mender_f, carter_f, dragoon_f,
        scout_f, drummer_f, shield_maiden_f, pikeman_f;

    // Enemy units
    public Dictionary<int, Sprite> EnemyImagesBack = new Dictionary<int, Sprite>();
    public Dictionary<int, Sprite> EnemyImagesFront = new Dictionary<int, Sprite>();
    // Plains
    public Sprite galtsa_b, grem_b, endu_b, korote_b, molner_b, etuena_b, clypte_b, goliath_b;
    public Sprite galtsa_f, grem_f, endu_f, korote_f, molner_f, etuena_f, clypte_f, goliath_f;
    // Forest
    public Sprite kverm_b, latu_b, eke_tu_b, oetem_b, eke_fu_b, eke_shi_ami_b, eke_lord_b, ketemcol_b;
    public Sprite kverm_f, latu_f, eke_tu_f, oetem_f, eke_fu_f, eke_shi_ami_f, eke_lord_f, ketemcol_f;
    // Titrum
    public Sprite mahukin_b, drongo_b, maheket_b, calute_b, etalket_b, muatem_b;
    public Sprite mahukin_f, drongo_f, maheket_f, calute_f, etalket_f, muatem_f;
    // Mountain/Cliff
    public Sprite drak_b, zerrku_b, gokin_b;
    public Sprite drak_f, zerrku_f, gokin_f;
    // Cave
    public Sprite tajaqar_b, tajaero_b, terra_qual_b, duale_b;
    public Sprite tajaqar_f, tajaero_f, terra_qual_f, duale_f;

    public Sprite meld_warrior_b, meld_spearman_b;
    public Sprite meld_warrior_f, meld_spearman_f;

    public bool SelectingForHeal = false;
    public TextMeshProUGUI discT;
    public PlayerUnit HealingUnit;

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

        Texts.Add(PlayerUnit.WARRIOR, TWarrior);
        Texts.Add(PlayerUnit.SPEARMAN, spearman_t);
        Texts.Add(PlayerUnit.ARCHER, archer_t);
        Texts.Add(PlayerUnit.GUARDIAN, guardian_t);
        Texts.Add(PlayerUnit.ARBALEST, arbalest_t);
        Texts.Add(PlayerUnit.PALADIN, paladin_t);
        Texts.Add(PlayerUnit.MENDER, mender_t);

        // Populate unit placement button images dictionary.
        UnitButtons.Add(PlayerUnit.WARRIOR, warrior_B);
        UnitButtons.Add(PlayerUnit.SPEARMAN, spearman_B);
        UnitButtons.Add(PlayerUnit.ARCHER, archer_B);
        UnitButtons.Add(PlayerUnit.GUARDIAN, guardian_B);
        UnitButtons.Add(PlayerUnit.ARBALEST, arbalest_B);
        UnitButtons.Add(PlayerUnit.PALADIN, paladin_B);
        UnitButtons.Add(PlayerUnit.MENDER, mender_B);

        // Unit images to be loaded into slots.
        UnitImagesBack.Add(PlayerUnit.WARRIOR, warrior_b);
        UnitImagesBack.Add(PlayerUnit.SPEARMAN, spearman_b);
        UnitImagesBack.Add(PlayerUnit.ARCHER, archer_b);
        UnitImagesBack.Add(PlayerUnit.ARBALEST, arbalest_b);
        UnitImagesBack.Add(PlayerUnit.PALADIN, paladin_b);
        UnitImagesBack.Add(PlayerUnit.MENDER, mender_b);
        UnitImagesBack.Add(PlayerUnit.GUARDIAN, guardian_b);

        UnitImagesFront.Add(PlayerUnit.WARRIOR, warrior_f);
        UnitImagesFront.Add(PlayerUnit.SPEARMAN, spearman_f);
        UnitImagesFront.Add(PlayerUnit.ARCHER, archer_f);
        UnitImagesFront.Add(PlayerUnit.ARBALEST, arbalest_f);
        UnitImagesFront.Add(PlayerUnit.PALADIN, paladin_f);
        UnitImagesFront.Add(PlayerUnit.MENDER, mender_f);
        UnitImagesFront.Add(PlayerUnit.GUARDIAN, guardian_f);

        // Enemies
        // Plains
        EnemyImagesBack.Add(Enemy.GALTSA, galtsa_b);
        EnemyImagesFront.Add(Enemy.GALTSA, galtsa_f);
        EnemyImagesBack.Add(Enemy.GREM, grem_b);
        EnemyImagesFront.Add(Enemy.GREM, grem_f);
        EnemyImagesBack.Add(Enemy.ENDU, endu_b);
        EnemyImagesFront.Add(Enemy.ENDU, endu_f);
        EnemyImagesBack.Add(Enemy.KOROTE, korote_b);
        EnemyImagesFront.Add(Enemy.KOROTE, korote_f);
        EnemyImagesBack.Add(Enemy.MOLNER, molner_b);
        EnemyImagesFront.Add(Enemy.MOLNER, molner_f);
        EnemyImagesBack.Add(Enemy.ETUENA, etuena_b);
        EnemyImagesFront.Add(Enemy.ETUENA, etuena_f);
        EnemyImagesBack.Add(Enemy.CLYPTE, clypte_b);
        EnemyImagesFront.Add(Enemy.CLYPTE, clypte_f);
        EnemyImagesBack.Add(Enemy.GOLIATH, goliath_b);
        EnemyImagesFront.Add(Enemy.GOLIATH, goliath_f);
        // Forest
        EnemyImagesBack.Add(Enemy.KVERM, kverm_b);
        EnemyImagesFront.Add(Enemy.KVERM, kverm_f);
        EnemyImagesBack.Add(Enemy.LATU, latu_b);
        EnemyImagesFront.Add(Enemy.LATU, latu_f);
        EnemyImagesBack.Add(Enemy.EKE_TU, eke_tu_b);
        EnemyImagesFront.Add(Enemy.EKE_TU, eke_tu_f);
        EnemyImagesBack.Add(Enemy.OETEM, oetem_b);
        EnemyImagesFront.Add(Enemy.OETEM, oetem_f);
        EnemyImagesBack.Add(Enemy.EKE_FU, eke_fu_b);
        EnemyImagesFront.Add(Enemy.EKE_FU, eke_fu_f);
        EnemyImagesBack.Add(Enemy.EKE_SHI_AMI, eke_shi_ami_b);
        EnemyImagesFront.Add(Enemy.EKE_SHI_AMI, eke_shi_ami_f);
        EnemyImagesBack.Add(Enemy.EKE_LORD, eke_lord_b);
        EnemyImagesFront.Add(Enemy.EKE_LORD, eke_lord_f);
        EnemyImagesBack.Add(Enemy.KETEMCOL, ketemcol_b);
        EnemyImagesFront.Add(Enemy.KETEMCOL, ketemcol_f);
        // Titrum
        EnemyImagesBack.Add(Enemy.MAHUKIN, mahukin_b);
        EnemyImagesFront.Add(Enemy.MAHUKIN, mahukin_f);
        EnemyImagesBack.Add(Enemy.DRONGO, drongo_b);
        EnemyImagesFront.Add(Enemy.DRONGO, drongo_f);
        EnemyImagesBack.Add(Enemy.MAHEKET, maheket_b);
        EnemyImagesFront.Add(Enemy.MAHEKET, maheket_f);
        EnemyImagesBack.Add(Enemy.CALUTE, calute_b);
        EnemyImagesFront.Add(Enemy.CALUTE, calute_f);
        EnemyImagesBack.Add(Enemy.ETALKET, etalket_b);
        EnemyImagesFront.Add(Enemy.ETALKET, etalket_f);
        EnemyImagesBack.Add(Enemy.MUATEM, muatem_b);
        EnemyImagesFront.Add(Enemy.MUATEM, muatem_f);

        // Mountains/Cliff
        EnemyImagesBack.Add(Enemy.DRAK, drak_b);
        EnemyImagesFront.Add(Enemy.DRAK, drak_f);
        EnemyImagesBack.Add(Enemy.ZERRKU, zerrku_b);
        EnemyImagesFront.Add(Enemy.ZERRKU, zerrku_f);
        EnemyImagesBack.Add(Enemy.GOKIN, gokin_b);
        EnemyImagesFront.Add(Enemy.GOKIN, gokin_f);
        // Cave
        EnemyImagesBack.Add(Enemy.TAJAQAR, tajaqar_b);
        EnemyImagesFront.Add(Enemy.TAJAQAR, tajaqar_f);
        EnemyImagesBack.Add(Enemy.TAJAERO, tajaero_b);
        EnemyImagesFront.Add(Enemy.TAJAERO, tajaero_f);
        EnemyImagesBack.Add(Enemy.TERRA_QUAL, terra_qual_b);
        EnemyImagesFront.Add(Enemy.TERRA_QUAL, terra_qual_f);
        EnemyImagesBack.Add(Enemy.DUALE, duale_b);
        EnemyImagesFront.Add(Enemy.DUALE, duale_f);

        // Meld
        EnemyImagesBack.Add(Enemy.MELD_WARRIOR, meld_warrior_b);
        EnemyImagesFront.Add(Enemy.MELD_WARRIOR, meld_warrior_f);

        EnemyImagesBack.Add(Enemy.MELD_SPEARMAN, meld_spearman_b);
        EnemyImagesFront.Add(Enemy.MELD_SPEARMAN, meld_spearman_f);
    }

    // This loads the player's battalion composition into the static
    // slots in the battle scene. 
    public void LoadBat(Battalion b)
    {
        foreach (int type_ID in b.Units.Keys)
        {
            LoadUnitText(b, type_ID);
            UnitButtons[type_ID].interactable = b.Units[type_ID].Count > 0;
        }
        MapUI.I.HighlightDiscipline(discT, null, b.Disc.ID);
    }

    public void LoadUnitText(Battalion b, int ID)
    {
        Texts[ID].text = BuildUnitText(b, ID);
    }

    /*
    Load unit counts in unit placement sidebar.
    */
    private string BuildUnitText(Battalion b, int ID)
    {
        if (!Texts.ContainsKey(ID))
            return "";
        string num = b.CountUnits(ID).ToString();
        int total_num = b.Units[ID].Count;
        return num + " / " + total_num.ToString();
    }
}
