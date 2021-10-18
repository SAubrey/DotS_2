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
    public bool active = false;
    public bool group_pending = true;
    public int init_turn = -1;

    // Listed in order of join.
    public List<Discipline> participants = new List<Discipline>(3);

    public Discipline leader;
    private Map map;
    public MapCell cell { get; private set; }
    private int turn = 1;

    public Battle(Map map, MapCell cell, Discipline leader, bool grouping)
    {
        this.map = map;
        this.cell = cell;
        this.leader = leader;
        init_turn = TurnPhaser.I.turn;
        add_participant(leader);
        leader.bat.pending_group_battle_cell = cell;
        group_pending = grouping;
    }

    public void add_participant(Discipline participant)
    {
        participants.Add(participant);
        if (group_pending)
            participant.bat.pending_group_battle_cell = cell;
    }

    public void remove_participant(Discipline participant)
    {
        participants.Remove(participant);
        participant.bat.pending_group_battle_cell = null;
    }

    /*
    A group battle begins from MapCellUI. A solo battle begins from travel card UI.
    */
    public void begin()
    {
        active = true;
        group_pending = false;
        foreach (Discipline d in participants)
        {
            d.bat.in_battle = true;
            if (is_group)
                d.bat.pending_group_battle_cell = null;
        }
        // Move player triggers turn phase advancement, triggering 
        if (is_group)
        {
            move_players();
            BattlePhaser.I.begin_new_battle(cell);
        }
        // Do not move single battalion in a solo battle since their battle
        // is generated as the result of movement.
    }

    public void advance_turn()
    {
        //active_bat_ID++;
        turn++;
    }

    private void move_players()
    {
        foreach (Discipline d in participants)
        {
            d.move(cell);
        }
    }

    public void retreat()
    {
        Debug.Log("attempting retreat. Participants: " + participants.Count);
        //cell.map.c.line_drawer.clear();

        foreach (Discipline d in participants)
        {
            d.bat.in_battle = false;
            d.move(d.previous_cell);
            // Penalize
            d.change_var(Storeable.UNITY, -1, true);
        }
        cell.set_tile_color();
        end();
    }

    public void end()
    {
        foreach (Discipline d in participants)
        {
            d.bat.in_battle = false;
        }
        group_pending = false;
        cell.clear_battle();
    }

    /* Battlion deaths trigger their respawn.
    The game is only over when the city runs out of health or
    the player quits.
    */
    public void post_phases()
    {
        // Are there no units on the field after combat? Both win (lose)
        if (player_won && enemy_won)
        {
            leader.receive_travelcard_consequence();
            if (!TurnPhaser.I.activeDisc.dead)
            {
                retreat();
            }
        }
        else if (player_won)
        {
            Debug.Log("Player won the battle.");
            leader.receive_travelcard_consequence();
        }
        else if (enemy_won)
        {
            if (!TurnPhaser.I.activeDisc.dead)
            {
                retreat();
            }
            // Forced retreat.
            Debug.Log("Enemy won the battle. This will only be called if the player has no units on the field, but some in reserve.");
        }
        end();
    }

    public bool post_battle()
    {
        foreach (Discipline d in participants)
        {
            d.bat.post_battle();
        }
        cell.post_battle();
        if (finished)
        {
            post_phases();
            return true;
        }
        return false;
    }

    public bool finished { get { return player_won || enemy_won; } }


    public bool player_won
    {
        //get { return Formation.I.get_all_full_slots(Unit.ENEMY).Count <= 0; }
        get { return cell.get_enemies().Count <= 0; }
    }

    public bool enemy_won
    {
        get { return !player_units_on_field; } //&& !units_in_reserve; }
    }


    public bool units_in_reserve
    {
        get { return count_all_units_in_reserve() > 0; }
    }

    public bool player_units_on_field
    {
        //get { return Controller.I.get_active_bat().get_all_placed_units().Count > 0; }
        // Ignores if an individual battalion should be retreated and ignored int eh turn cycle.
        get { return count_all_placed_units() > 0; }
    }

    private bool check_all_dead()
    {
        foreach (Discipline d in participants)
        {
            if (!d.dead)
                return false;
        }
        return true;
    }

    public List<Battalion> get_dead_battalions()
    {
        List<Battalion> dead = new List<Battalion>();
        foreach (Discipline d in participants)
        {
            if (d.bat.count_units() <= 0)
            {
                dead.Add(d.bat);
            }
        }
        return dead;
    }

    public int count_all_placed_units()
    {
        int sum = 0;
        foreach (Discipline d in participants)
        {
            sum += d.bat.get_all_placed_units().Count;
        }
        return sum;
    }

    public int count_all_units_in_reserve()
    {
        int sum = 0;
        foreach (Discipline d in participants)
            sum += d.bat.count_units();
        return sum;
    }

    public bool is_group { get => participants.Count > 1; }

    public bool can_begin_group
    {
        get { return TurnPhaser.I.turn > init_turn && participants.Count > 1; }
    }

    public bool leader_is_active_on_map
    {
        get { return TurnPhaser.I.activeDisc == leader; }
    }

    public bool last_battalions_turn
    {
        get =>
(turn - 1) % participants.Count == participants.Count - 1;
    }

    public bool includes_disc(Discipline d)
    {
        return participants.Contains(d);
    }
}
