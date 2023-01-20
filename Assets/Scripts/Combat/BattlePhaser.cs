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

    //private List<Image> DiscIcons = new List<Image>();
    public Image DiscIcon;//, DiscIcon2, DiscIcon3;
    // <discipline ID, disc Sprite>
    private Dictionary<int, Sprite> DiscSprites = new Dictionary<int, Sprite>();
    public Sprite AstraIcon, MartialIcon, EnduraIcon;
    public Image BiomeIcon;

    private Battle _battle;
    private Battle Battle
    {
        get { return _battle; }
        set
        {
            _battle = value;
        }
    }
    public Battalion ActiveBat
    {
        get
        {
            if (Battle == null)
                return null;
            //return Controller.I.get_disc(battle.active_bat_ID).bat; 
            return TurnPhaser.I.ActiveDisc.Bat;
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
        DiscSprites.Add(Discipline.Astra, AstraIcon);
        DiscSprites.Add(Discipline.Martial, MartialIcon);
        DiscSprites.Add(Discipline.Endura, EnduraIcon);
    }

    // Called at the end of 3 phases.
    public void Reset(bool resetBattlefield = true)
    {
        if (resetBattlefield)
        {
            Battle = null;
        }
    }

    public void BeginNewBattle(MapCell cell)
    {
        Battle = cell.Battle;
        // Must be solo battle if null, because group battles will
        // have been created at least one turn in advance of battle initiation.
        if (Battle == null)
        {
            Battle = new Battle(Map.I, cell, TurnPhaser.I.ActiveDisc, false);
            cell.Battle = Battle;
            Battle.Begin();
        }

        if (!cell.HasSeenCombat)
        {
            Debug.Log("generating enemies");
            EnemyLoader.I.GenerateNewEnemies(cell, cell.Travelcard.EnemyCount);
        }
        MapUI.I.CloseCellUI();
        SetupParticipantUI(cell);

        Debug.Log("loading enemies");
        CamSwitcher.I.SetActive(CamSwitcher.BATTLE, true);
        EnemyLoader.I.LoadEnemies(cell.GetEnemies(), 1);
        PlayerDeployment.I.PlaceUnits(ActiveBat);
        cell.HasSeenCombat = true;
    }

    // At battle initialization.
    private void SetupParticipantUI(MapCell cell)
    {
        Battle = cell.Battle;
        DiscIcon.sprite = DiscSprites[TurnPhaser.I.ActiveDiscID];
        BiomeIcon.sprite = Map.I.Tiles[cell.ID][cell.Tier].sprite;
        BatLoader.I.LoadBat(ActiveBat);
        UpdateParticipantUI();
    }

    private void UpdateParticipantUI()
    {
        BatLoader.I.LoadBat(ActiveBat);
    }

    public void Advance()
    {
        Battle.AdvanceTurn();
        UpdateParticipantUI();
    }

    // Only called by AttackQueuer after battle animations have finished.
    public void PostBattle()
    {
        Battle.PostPhases();

        if (Battle.PostBattle())
        {
            Reset();
            CamSwitcher.I.SetActive(CamSwitcher.MAP, true);
        }
    }

    // Called by Unity button. 
    public void Retreat()
    {
        if (!Battle.PlayerUnitsOnField || Battle.PlayerWon)
            return;

        Battle.Retreat();
        CamSwitcher.I.SetActive(CamSwitcher.MAP, true);
        Reset();
    }

    // check each combat stage end
    // battalion_dead implies enemy_won, enemy_won does not imply battalion_dead.
    private bool BattalionDead
    {
        get => ActiveBat.CountUnits() <= 0;
    }
}
