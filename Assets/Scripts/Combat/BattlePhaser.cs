using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Controls the game logic of the battle phases
public class BattlePhaser : MonoBehaviour
{
    public static BattlePhaser I { get; private set; }

    private List<Image> disc_icons = new List<Image>();
    public Image disc_icon1, disc_icon2, disc_icon3;
    // <discipline ID, disc Sprite>
    private Dictionary<int, Sprite> disc_sprites = new Dictionary<int, Sprite>();
    public Sprite astra_icon, martial_icon, endura_icon;
    public Sprite empty_sprite;
    public Color inactive_color;
    public Image biome_icon;

    private Battle _battle;
    private Battle battle
    {
        get { return _battle; }
        set
        {
            _battle = value;
        }
    }
    public Battalion active_bat
    {
        get
        {
            if (battle == null)
                return null;
            //return Controller.I.get_disc(battle.active_bat_ID).bat; 
            return TurnPhaser.I.activeDisc.bat;
        }
    }

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
        disc_icons.Add(disc_icon1);
        disc_icons.Add(disc_icon2);
        disc_icons.Add(disc_icon3);
        disc_sprites.Add(Discipline.ASTRA, astra_icon);
        disc_sprites.Add(Discipline.MARTIAL, martial_icon);
        disc_sprites.Add(Discipline.ENDURA, endura_icon);

    }

    // Called at the end of 3 phases.
    public void reset(bool reset_battlefield = true)
    {
        if (reset_battlefield)
        {
            battle = null;
        }
    }

    public void begin_new_battle(MapCell cell)
    {
        battle = cell.battle;
        // Must be solo battle if null, because group battles will
        // have been created at least one turn in advance of battle initiation.
        if (battle == null)
        {
            battle = new Battle(Map.I, cell, TurnPhaser.I.activeDisc, false);
            cell.battle = battle;
            battle.begin();
        }

        if (!cell.has_seen_combat)
        {
            Debug.Log("generating enemies");
            EnemyLoader.I.generate_new_enemies(cell, cell.travelcard.enemy_count);
        }
        MapUI.I.close_cell_UI();
        setup_participant_UI(cell);

        Debug.Log("loading enemies");
        CamSwitcher.I.set_active(CamSwitcher.BATTLE, true);
        EnemyLoader.I.load_enemies(cell.get_enemies(), 9);
        PlayerDeployment.I.place_units(active_bat);
        cell.has_seen_combat = true;
    }

    // First determine if a battle is grouped or not, then either resume or reload.
    /*
    Resumed battles are battles that did not finish in a single turn.
    */
    public void resume_battle(MapCell cell)
    {
        battle = cell.battle;
        if (battle == null)
            return;

        setup_participant_UI(cell);
        CamSwitcher.I.set_active(CamSwitcher.BATTLE, true);
    }

    // At battle initialization.
    private void setup_participant_UI(MapCell cell)
    {
        battle = cell.battle;
        if (battle == null)
        {
            // only activate one image
            disc_icon1.sprite = disc_sprites[TurnPhaser.I.activeDiscID];
            Debug.Log("this should never happen");
        }
        else
        {
            // Order discipline images by group join time.
            for (int i = 0; i < 3; i++)
            {
                if (battle.participants.Count - 1 >= i)
                {
                    disc_icons[i].sprite = disc_sprites[battle.participants[i].ID];
                }
                else
                {
                    disc_icons[i].sprite = empty_sprite;
                }
            }
        }
        biome_icon.sprite = Map.I.tiles[cell.ID][cell.tier].sprite;
        BatLoader.I.load_bat(active_bat);
        update_participant_UI();
    }

    private void update_participant_UI()
    {
        BatLoader.I.load_bat(active_bat);

        if (!battle.is_group)
        {
            // Update icons
            disc_icon1.color = Color.white;
            disc_icon2.color = Color.clear;
            disc_icon3.color = Color.clear;
        }
        else
        {
            // Update text

            // Update icons
            /* If in battle and active, color the icon white, 
            if in battle and not active, color half transparent,
            if not in battle color transparent. */
            for (int i = 0; i < 3; i++)
            {
                if (battle.participants.Count - 1 >= i)
                {
                    disc_icons[i].color = Color.white;
                }
                else
                {
                    disc_icons[i].color = Color.clear;
                }
            }
        }
    }

    public void advance()
    {
        battle.advance_turn();
        update_participant_UI();
    }

    // Only called by AttackQueuer after battle animations have finished.
    public void post_battle()
    {
        battle.post_phases();

        if (battle.post_battle())
        {
            reset();
            CamSwitcher.I.set_active(CamSwitcher.MAP, true);
        }
    }

    // Called by Unity button. 
    public void retreat()
    {
        if (!battle.player_units_on_field || battle.player_won)
            return;

        battle.retreat();
        CamSwitcher.I.set_active(CamSwitcher.MAP, true);
        reset();
    }

    // check each combat stage end
    // battalion_dead implies enemy_won, enemy_won does not imply battalion_dead.
    private bool battalion_dead
    {
        get => active_bat.count_units() <= 0;
    }
}
