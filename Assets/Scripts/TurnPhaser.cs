using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System;

// Manages intra-turn progression. Stages are within phases which are within a game turn.
// There are 3 stages for each phase, and a phase for each player.
public class TurnPhaser : MonoBehaviour, ISaveLoad
{
    public static TurnPhaser I { get; private set; }

    public event Action<Discipline> OnDiscChange;
    public event Action OnTurnChange;
    public int Turn { get; private set; }
    public Discipline Astra, Martial, Endura;

    public IDictionary<int, Discipline> Discs = new Dictionary<int, Discipline>();

    // <arbitrary turn order index, Discipline>
    public Discipline[] DiscsInPlay;
    public int NumPlayers { get; private set; } = 1;

    private int _activeDiscID;
    public int ActiveDiscID
    {
        get { return _activeDiscID; }
        set
        {
            _activeDiscID = value % NumPlayers;
            ActiveDisc = DiscsInPlay[value % NumPlayers];
            ActiveDisc.Active = true;
            GetDisc((_activeDiscID + 1) % 3).Active = false;
            GetDisc((_activeDiscID + 2) % 3).Active = false;
        }
    } // disciplines are like sub factions.
    public Discipline ActiveDisc { get; private set; }

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

    public void Init(Discipline chosenDisc) {
        Astra.ID = Discipline.Astra;
        Martial.ID = Discipline.Martial;
        Endura.ID = Discipline.Endura;
        Discs.Add(Discipline.Astra, Astra);
        Discs.Add(Discipline.Martial, Martial);
        Discs.Add(Discipline.Endura, Endura);

        DiscsInPlay = new Discipline[] {chosenDisc};
        NumPlayers = DiscsInPlay.Length;
        ActiveDisc = chosenDisc;
        ActiveDisc.Piece.SetActive(true);
    }

    public void AdvanceTurn()
    {
        Turn++;
        OnTurnChange();
    }

    public void EndDisciplinesTurn()
    {
        MapUI.I.CloseCellUI();
        AdvancePlayer();
    }

    private void AdvancePlayer()
    {
        ActiveDiscID++;
        if (OnDiscChange != null)
            OnDiscChange(ActiveDisc);
        if (ActiveDiscID == DiscsInPlay[0].ID) {
            AdvanceTurn();
        }

        Reset();
        CamSwitcher.I.SetActive(CamSwitcher.MAP, true);
    }

    public void Reset()
    { // New game, new player
        MapUI.I.SetActiveAskToEnterP(false);
        MapUI.I.UpdateCellText(ActiveDisc.Cell.Name);
    }

    public GameData Save()
    {
        TurnPhaserData data = new TurnPhaserData();
        data.name = "TurnPhaser";
        data.turn = Turn;
        data.activeDiscID = ActiveDiscID;
        return data;
    }

    public void Load(GameData generic)
    {
        TurnPhaserData data = generic as TurnPhaserData;
        ActiveDiscID = data.activeDiscID;
        Turn = data.turn;
    }
    
    public Discipline GetDisc(int ID)
    {
        if (!Discs.Keys.Contains(ID)) {
            return ActiveDisc;
        }
        return Discs[ID];
    }
}