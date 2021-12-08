using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TravelCardManager : MonoBehaviour
{
    public static TravelCardManager I { get; private set; }
    public Die Die;
    private int NumSides;
    private TravelCard Travelcard;
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

    // Called by the roll button of the travel card. 
    public void roll()
    {
        Die.roll(NumSides);
        Travelcard.Rolled = true;
    }

    public void FinishRoll(int result)
    {
        MapUI.I.SetActiveTravelcardContinueB(true);
        MapUI.I.travelcard_rollB.interactable = false;
        Travelcard.UseRollResult(result);
        Travelcard = null;
    }

    public void SetUpRoll(TravelCard tc, int num_sides)
    {
        this.Travelcard = tc;
        this.NumSides = num_sides;
        MapUI.I.SetActiveTravelcardContinueB(false);
        MapUI.I.SetActiveTravelcardRollB(true);
    }

    // Don't pull a travel card on a discovered cell.
    public void RestartBattleFromDrawnCard(MapCell cell)
    {
        // Load forced travel card from a previous save.
        if (TurnPhaser.I.ActiveDisc.RestartBattleFromDrawnCard)
        {
            MapUI.I.DisplayTravelcard(TurnPhaser.I.ActiveDisc.GetTravelcard());
            TurnPhaser.I.ActiveDisc.RestartBattleFromDrawnCard = false;
            Debug.Log("resuming loaded battle");
            return;
        }
    }


    // Activated when 'Continue' is pressed on travel card or 'Yes' on warning to enter.
    public void ContinueTravelCard(bool show_warning = true)
    {
        MapCell cell = MapUI.I.last_open_cell;
        // Preempt entrance with warning?
        if (show_warning &&
            ((cell.ID == MapCell.IDCave ||
            cell.ID == MapCell.IDRuins) &&
            cell.Travelcard.Rules.enter_combat))
        {
            MapUI.I.SetActiveAskToEnterP(true);
            return;
        }
        ActivateTravelcard(cell);
    }

    // Called by the continue button of the travel card. 
    public void ActivateTravelcard(MapCell cell)
    {
        if (cell == null)
            return;
        if (!cell.CreatesTravelcard || cell.TravelcardComplete)
            return;
        if (cell.Travelcard == null)
        {
            throw new System.ArgumentException("Cell must have a travelcard");
        }

        cell.Travelcard.OnContinue(TravelCardManager.I);
        if (cell.Travelcard.Rules.enter_combat)
        { // If a combat travel card was pulled.
            BattlePhaser.I.BeginNewBattle(cell);
        }
        else if (cell.Travelcard.Rules.affect_resources && !cell.Locked)
        {
            TurnPhaser.I.ActiveDisc.ReceiveTravelcardConsequence();
        }
    }

}
