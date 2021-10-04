using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

// Slot group
public class Group : MonoBehaviour
{
    public const int UP = 0, DOWN = 180, LEFT = 90, RIGHT = 270;
    public const int MAX = 9;

    // Group types. Used to limit unit placement.
    public const int NEUTRAL = 0; // Cannot place here.
    public const int PLAYER = 1; // player can only place here initially.
    public const int ENEMY = 2; // Only enemy can place here.
    public const int PERIPHERY = 3; // Player can place here phase 2+.

    public Color neutral_color;
    public Color player_color;
    public Color enemy_color;
    public Color periphery_color;
    public int type;

    public int col, row;
    public List<Slot> slots = new List<Slot>();
    private bool initialized = false;
    Image img;

    public GameObject slot_group;
    public Transform[] slot_point_transforms = new Transform[0];
    public Deployment deployment;

    void Awake()
    {
        img = GetComponent<Image>();
        pair_slot_point_group();
    }
    void Start()
    {
        init();
    }

    public void init()
    {
        set_color(type);
        //reorder_slots_visually(direction);
        initialized = true;
    }

    public void place_unit(Unit unit)
    {
        Slot s = get_highest_empty_slot();
        if (s != null)
            s.fill(unit);
    }

    public void pair_slot_point_group()
    {
        foreach (Transform child in slot_group.transform)
        {
            slots.Add(child.gameObject.GetComponent<Slot>());
        }
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].deployment = deployment;
            slots[i].slot_point_transform = slot_point_transforms[i];
        }
    }

    // Moves units up within their group upon vacancies from unit death/movement.
    public void validate_unit_order()
    {
        if (is_empty)
            return;

        for (int i = 0; i < MAX; i++)
        {
            if (!slots[i].is_empty)
                continue;
            for (int j = i + 1; j < MAX; j++)
            {
                if (slots[j].is_empty)
                    continue;
                // Move unit to higher slot
                slots[i].fill(slots[j].empty(false));
                break;
            }
        }
    }

    public void rotate_sprites(int direction)
    {
        foreach (Slot s in slots)
        {
            s.rotate_to_direction(direction);
        }
        //reorder_slots_visually(direction);
    }

    /* Adjust the order of slots in the inspector to bring the image
    of the closest slot to the front.
    */
    private void reorder_slots_visually(int direction)
    {
        if (direction == UP)
        {
            slots[0].transform.SetAsFirstSibling();
            slots[1].transform.SetAsLastSibling();
        }
        else if (direction == DOWN)
        {
            slots[1].transform.SetAsFirstSibling();
            slots[0].transform.SetAsLastSibling();
        }
        else if (direction == LEFT)
        {
            slots[2].transform.SetAsFirstSibling();
            slots[0].transform.SetAsLastSibling();
        }
        else if (direction == RIGHT)
        {
            slots[0].transform.SetAsFirstSibling();
            slots[2].transform.SetAsLastSibling();
        }
    }

    public int get_num_of_same_active_units_in_group(int unit_ID)
    {
        int num_grouped = 0;
        foreach (Slot s in slots)
        {
            if (!s.has_unit)
                continue;
            if (s.get_unit().get_ID() == unit_ID)
                num_grouped++;
        }
        return num_grouped;
    }

    // Toggles the group color if no parameter is set. 
    private void set_color(int type)
    {
        if (type == NEUTRAL)
            img.color = neutral_color;
        else if (type == PLAYER)
        {
            img.color = player_color;
        }
        else if (type == ENEMY)
            img.color = enemy_color;
        else if (type == PERIPHERY)
            img.color = periphery_color;
    }

    public void reset_dir()
    {
        rotate_sprites(Group.UP);
    }

    public void set(int i, Unit u)
    {
        slots[i].fill(u);
    }
    public Slot get(int i)
    {
        return slots[i];
    }

    public int sum_unit_health()
    {
        int sum = 0;
        foreach (Slot s in slots)
        {
            if (s.has_unit)
            {
                sum += s.get_unit().health;
            }
        }
        return sum;
    }

    public Slot get_highest_full_slot()
    {
        for (int i = 0; i < MAX; i++)
            if (slots[i].has_unit)
                return slots[i];
        return null;
    }

    public Slot get_highest_empty_slot()
    {
        for (int i = 0; i < MAX; i++)
        {
            if (slots[i].is_empty)
                return slots[i];
        }
        return null;
    }

    public Slot get_highest_enemy_slot()
    {
        for (int i = 0; i < MAX; i++)
            if (slots[i].has_enemy)
                return slots[i];
        return null;
    }

    public Slot get_highest_player_slot()
    {
        foreach (Slot s in slots)
            if (s.has_punit)
                return s;
        return null;
    }

    public List<Slot> get_full_slots()
    {
        List<Slot> full_slots = new List<Slot>();
        foreach (Slot s in slots)
            if (s.has_unit)
                full_slots.Add(s);
        return full_slots;
    }

    public bool has_punit
    {
        get
        {
            foreach (Slot slot in slots)
                if (slot.get_punit() != null)
                    return true;
            return false;
        }
    }

    public bool has_enemy
    {
        get
        {
            foreach (Slot slot in slots)
                if (slot.get_enemy() != null)
                    return true;
            return false;
        }
    }

    public void empty()
    {
        foreach (Slot s in slots)
            if (!s.is_empty)
                s.empty(false);
    }

    public bool is_empty
    {
        get
        {
            foreach (Slot slot in slots)
                if (!slot.is_empty)
                    return false;
            return true;
        }
    }


    public bool is_full
    {
        get
        {
            foreach (Slot slot in slots)
                if (slot.is_empty)
                    return false;
            return true;
        }
    }
}