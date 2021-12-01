using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MapCellUI : MonoBehaviour
{
    public TextMeshProUGUI cell_typeT, enemy_countT, mineableT, battleT;
    public Button scoutB, teleportB, moveB, unlockB, battleB, mineB, show_travelcardB;
    public MapCell cell { get; private set; }

    public void init(MapCell cell)
    {
        this.cell = cell;
        cell_typeT.text = build_titleT();
        enemy_countT.text = build_enemy_countT(cell);
        update_mineable_text();
        Vector3 pos = new Vector3(cell.Pos.x, cell.Pos.y, 0);
        //transform.position =
        //map.c.cam_switcher.mapCam.WorldToScreenPoint(new Vector3(pos.x + 0.5f, pos.y - 2.5f, 0));
        transform.position = new Vector3(pos.x + 0.5f, pos.y - 1.5f, 0); // camera mode, not overlay
        gameObject.transform.SetAsFirstSibling();

        enable_button(moveB, Map.I.CanMove(pos));
        enable_button(scoutB, Map.I.CanScout(pos));
        enable_button(teleportB, Map.I.CanTeleport(pos));
        enable_button(unlockB, can_unlock());
        enable_button(battleB, cell.CanSetupGroupBattle());
        enable_button(mineB, cell.CanMine(TurnPhaser.I.ActiveDisc.Bat));
        enable_button(show_travelcardB, can_show_travelcard());
    }

    public void update_mineable_text()
    {
        if (!cell.Discovered)
        {
            mineableT.text = "?";
            return;
        }
        if (cell.StarCrystals > 0)
            mineableT.text = cell.StarCrystals.ToString() + " Star Crystals";
        else if (cell.Minerals > 0)
            mineableT.text = cell.Minerals.ToString() + " Minerals";
        else
            mineableT.text = "No mineable resources.";
    }

    private string build_titleT()
    {
        if (cell.HasRuneGate)
        {
            return cell.RestoredRuneGate ? "Active Rune Gate" : "Inactive Rune Gate";
        }
        else
        {
            return cell.Discovered ? cell.Name : "Unknown";
        }
    }

    private string build_enemy_countT(MapCell cell)
    {
        int enemy_count = cell.GetEnemies().Count;
        if (cell.HasTravelcard)
        {
            // Enemies have not spawned yet but we know they are there.
            if (enemy_count == 0 && cell.Discovered && !cell.TravelcardComplete
                    && cell.Travelcard.EnemyCount > 0)
            {
                enemy_count = cell.Travelcard.EnemyCount;
            }
        }
        if (!cell.Discovered)
        {
            return "We know not what waits in the darkness.";
        }
        if (enemy_count > 0)
        {
            return enemy_count + " enemies are lurking in the darkness.";
        }
        return "This land is free from darkness.";
    }


    public void move()
    {
        TurnPhaser.I.ActiveDisc.Move(cell);
        close();
    }

    public void close()
    {
        MapUI.I.open_cell_UI_script = null;
        Destroy(gameObject);
    }

    public void scout()
    {
        Map.I.Scout(new Vector3(cell.Pos.x, cell.Pos.y, 0));
        enable_button(scoutB, false);
        close();
    }

    public void teleport()
    {
        TurnPhaser.I.ActiveDisc.Move(cell);
        close();
    }

    public void mine()
    {
        TurnPhaser.I.ActiveDisc.Mine(cell);
    }

    public bool can_show_travelcard()
    {
        return cell.CreatesTravelcard && cell.Discovered;
    }

    public void show_travelcard()
    {
        //MapUI.I.toggle_travelcard(cell.travelcard);
        MapUI.I.DisplayTravelcard(cell.Travelcard);
    }

    // To determine if the unlock button can be pressed, including that the 
    // requirements can be met if it is an unlockable cell.
    private bool can_unlock()
    {
        bool on_cell = TurnPhaser.I.ActiveDisc.Cell == cell;
        if (!cell.Locked || !on_cell)
            return false;
        if (cell.HasRuneGate && !cell.RestoredRuneGate &&
            TurnPhaser.I.ActiveDisc.GetResource(Storeable.STAR_CRYSTALS) >= 10)
        {
            return true;
        }

        TravelCardUnlockable u = cell.GetUnlockable();
        if (u.RequiresSeeker)
        {
            return TurnPhaser.I.ActiveDisc.Bat.HasSeeker;
        }
        // Must be a resource requirement.
        if (TurnPhaser.I.ActiveDisc.GetResource(u.ResourceType) >=
            Mathf.Abs(u.ResourceCost))
        {
            return true;
        }
        return false;
    }

    public void unlock()
    {
        if (cell.HasRuneGate)
        {
            TurnPhaser.I.ActiveDisc.ShowAdjustment(Storeable.STAR_CRYSTALS, -10);
            cell.RestoredRuneGate = true;
        }
        else if (cell.HasTravelcard)
        {
            if (cell.GetUnlockable().RequiresSeeker)
            {
                TurnPhaser.I.ActiveDisc.ReceiveTravelcardConsequence();
            }
            else
            {
                TurnPhaser.I.ActiveDisc.ShowAdjustment(cell.GetUnlockType(), cell.GetUnlockCost());
            }
            cell.CompleteTravelcard();
        }
        cell.Locked = false;
    }

    private void enable_button(Button b, bool state)
    {
        b.interactable = state;
    }
}
