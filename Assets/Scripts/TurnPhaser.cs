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

    public event Action<Discipline> onDiscChange;
    public event Action onTurnChange;
    public int turn { get; private set; }
    public Discipline astra, martial, endura;

    public IDictionary<int, Discipline> discs = new Dictionary<int, Discipline>();

    // <arbitrary turn order index, Discipline>
    public Discipline[] discsInPlay;
    public int numPlayers { get; private set; } = 1;

    private int _activeDiscID;
    public int activeDiscID
    {
        get { return _activeDiscID; }
        set
        {
            _activeDiscID = value % numPlayers;
            activeDisc = discsInPlay[value % numPlayers];
            activeDisc.active = true;
            getDisc((_activeDiscID + 1) % 3).active = false;
            getDisc((_activeDiscID + 2) % 3).active = false;
        }
    } // disciplines are like sub factions.
    public Discipline activeDisc { get; private set; }

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

    public void Init(Discipline chosen_disc) {
        astra.ID = Discipline.ASTRA;
        martial.ID = Discipline.MARTIAL;
        endura.ID = Discipline.ENDURA;
        discs.Add(Discipline.ASTRA, astra);
        discs.Add(Discipline.MARTIAL, martial);
        discs.Add(Discipline.ENDURA, endura);

        discsInPlay = new Discipline[] {chosen_disc};
        numPlayers = discsInPlay.Length;
        activeDisc = chosen_disc;
        activeDisc.piece.SetActive(true);
    }

    public void advance_turn()
    {
        turn++;
        onTurnChange();
    }

    public void end_disciplines_turn()
    {
        MapUI.I.close_cell_UI();
        advance_player();
    }

    private void advance_player()
    {
        activeDiscID++;
        if (onDiscChange != null)
            onDiscChange(activeDisc);
        if (activeDiscID == discsInPlay[0].ID)
            advance_turn();

        if (activeDisc.dead)
        {
            activeDisc.respawn();
        }

        reset();
        CamSwitcher.I.set_active(CamSwitcher.MAP, true);
    }

    public void reset()
    { // New game, new player
        MapUI.I.set_active_ask_to_enterP(false);
        MapUI.I.update_cell_text(activeDisc.cell.name);
    }

    public GameData save()
    {
        TurnPhaserData data = new TurnPhaserData();
        data.name = "TurnPhaser";
        data.turn = turn;
        data.activeDiscID = activeDiscID;
        return data;
    }

    public void load(GameData generic)
    {
        TurnPhaserData data = generic as TurnPhaserData;
        activeDiscID = data.activeDiscID;
        turn = data.turn;
    }
    
    public Discipline getDisc(int ID)
    {
        if (!discs.Keys.Contains(ID)) {
            return activeDisc;
        }
        return discs[ID];
    }
}