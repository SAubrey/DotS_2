using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
This class operates in solo and group battles,
and will  dynamically adapt to additional participants.

A battle exists immediately before a solo battle or while a group battle is pending,
and for the duration of the battle until retreat/defeat/victory.
*/
public class Battle
{
    public bool Active = false;
    public bool GroupPending = true;
    public int InitTurn = -1;

    // Listed in order of join.
    public List<Discipline> Participants = new List<Discipline>(3);

    public Discipline Leader;
    private Map Map;
    public MapCell Cell { get; private set; }
    private int Turn = 1;

    public Battle(Map map, MapCell cell, Discipline leader, bool grouping)
    {
        this.Map = map;
        this.Cell = cell;
        this.Leader = leader;
        InitTurn = TurnPhaser.I.Turn;
        AddParticipant(leader);
        GroupPending = grouping;
    }

    public void AddParticipant(Discipline participant)
    {
        Participants.Add(participant);
        if (GroupPending)
            participant.Bat.PendingGroupBattleCell = Cell;
    }

    public void RemoveParticipant(Discipline participant)
    {
        Participants.Remove(participant);
        participant.Bat.PendingGroupBattleCell = null;
    }

    /*
    A group battle begins from MapCellUI. A solo battle begins from travel card UI.
    */
    public void Begin()
    {
        Active = true;
        GroupPending = false;
        foreach (Discipline d in Participants)
        {
            d.Bat.InBattle = true;
            if (is_group)
                d.Bat.PendingGroupBattleCell = null;
        }
        // Move player triggers turn phase advancement, triggering 
        if (is_group)
        {
            MovePlayers();
            BattlePhaser.I.BeginNewBattle(Cell);
        }
        // Do not move single battalion in a solo battle since their battle
        // is generated as the result of movement.
    }

    public void AdvanceTurn()
    {
        //active_bat_ID++;
        Turn++;
    }

    private void MovePlayers()
    {
        foreach (Discipline d in Participants)
        {
            d.Move(Cell);
        }
    }

    public void Retreat()
    {
        Debug.Log("attempting retreat. Participants: " + Participants.Count);
        //cell.map.c.line_drawer.clear();

        foreach (Discipline d in Participants)
        {
            d.Bat.InBattle = false;
            d.Move(d.PreviousCell);
            // Penalize
            d.AdjustResource(Storeable.UNITY, -1, true);
        }
        Cell.SetTileColor();
        End();
    }

    public void End()
    {
        foreach (Discipline d in Participants)
        {
            d.Bat.InBattle = false;
        }
        GroupPending = false;
        Cell.ClearBattle();
    }

    /* Battlion deaths trigger their respawn.
    The game is only over when the city runs out of health or
    the player quits.
    */
    public void PostPhases()
    {
        if (PlayerWon)
        {
            Debug.Log("Player won the battle.");
            Leader.ReceiveTravelcardConsequence();
        }
        else if (EnemyWon)
        {
           Leader.Die();
        }
        End();
    }

    public bool PostBattle()
    {
        foreach (Discipline d in Participants)
        {
            d.Bat.PostBattle();
        }
        Cell.PostBattle();
        if (Finished)
        {
            PostPhases();
            return true;
        }
        return false;
    }

    public bool Finished { get { return PlayerWon || EnemyWon; } }


    public bool PlayerWon
    {
        //get { return Formation.I.get_all_full_slots(Unit.ENEMY).Count <= 0; }
        get { return Cell.GetEnemies().Count <= 0; }
    }

    public bool EnemyWon
    {
        get { return !PlayerUnitsOnField; } //&& !units_in_reserve; }
    }

    public bool PlayerUnitsOnField
    {
        //get { return Controller.I.get_active_bat().get_all_placed_units().Count > 0; }
        // Ignores if an individual battalion should be retreated and ignored int eh turn cycle.
        get { return count_all_placed_units() > 0; }
    }

    public List<Battalion> get_dead_battalions()
    {
        List<Battalion> dead = new List<Battalion>();
        foreach (Discipline d in Participants)
        {
            if (d.Bat.CountUnits() <= 0)
            {
                dead.Add(d.Bat);
            }
        }
        return dead;
    }

    public int count_all_placed_units()
    {
        int sum = 0;
        foreach (Discipline d in Participants)
        {
            sum += d.Bat.GetAllPlacedUnits().Count;
        }
        return sum;
    }

    public int count_all_units_in_reserve()
    {
        int sum = 0;
        foreach (Discipline d in Participants)
            sum += d.Bat.CountUnits();
        return sum;
    }

    public bool is_group { get => Participants.Count > 1; }

    public bool can_begin_group
    {
        get { return TurnPhaser.I.Turn > InitTurn && Participants.Count > 1; }
    }

    public bool leader_is_active_on_map
    {
        get { return TurnPhaser.I.ActiveDisc == Leader; }
    }

    public bool last_battalions_turn
    {
        get =>
(Turn - 1) % Participants.Count == Participants.Count - 1;
    }

    public bool includes_disc(Discipline d)
    {
        return Participants.Contains(d);
    }
}
