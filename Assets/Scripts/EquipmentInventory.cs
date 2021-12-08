using System.Collections.Generic;
using UnityEngine;

public class EquipmentInventory
{
    // <equipment ID, List of equipment of that type>
    public Dictionary<string, List<Equipment>> Inventory = new Dictionary<string, List<Equipment>>();
    public List<EquipmentSlot> EquipmentSlots = new List<EquipmentSlot>();
    public int[] slot_experience_requirements = new int[4] {
        50, 150, 300, 1000
    };
    public Discipline Disc;

    public EquipmentInventory(Discipline disc)
    {
        this.Disc = disc;
        for (int i = 0; i < slot_experience_requirements.Length; i++)
        {
            EquipmentSlots.Add(new EquipmentSlot(i));
        }
    }

    public EquipmentSlot GetSlot(int i)
    {
        return EquipmentSlots[i];
    }

    public int GetHighestUnlockedSlot()
    {
        for (int i = slot_experience_requirements.Length - 1; i >= 0; i--)
        {
            if (Disc.GetResource(Discipline.Experience) >= slot_experience_requirements[i])
            {
                return i;
            }
        }
        return -1;
    }

    public string AddRandomEquipment(int tier)
    {
        int choice_index = Random.Range(0, Equipment.equipment[tier].Length);
        return (add_to_inventory(Equipment.equipment[tier][choice_index]));
    }

    public string add_to_inventory(string name, int amount = 1)
    {
        Equipment e = Equipment.make_equipment(name);
        if (!Inventory.ContainsKey(name))
            Inventory.Add(name, new List<Equipment>());
        Inventory[name].Add(e);
        Disc.ShowAdjustment(name, amount);
        EquipmentUI.I.FillDropdowns(this);
        return e == null ? "" : e.name;
    }

    public void equip(string name, int slot_num)
    {
        if (name == null || !Inventory.ContainsKey(name))
            return;

        EquipmentSlots[slot_num].fill(Inventory[name][0]);
    }

    public void unequip(int slot_num)
    {
        if (slot_num < 0 || slot_num > EquipmentSlots.Count)
            return;
        EquipmentSlots[slot_num].clear();
    }

    public int get_stat_boost_amount(int unit_ID, int stat_ID)
    {
        int sum = 0;
        foreach (EquipmentSlot es in EquipmentSlots)
        {
            if (!es.full)
                continue;
            if (!check_compatible_unit(unit_ID, es.equipment)
                || !check_compatible_stat(stat_ID, es.equipment))
                continue;
            sum += es.equipment.affect_amount;
        }
        return sum;
    }

    private bool check_compatible_unit(int unit_ID, Equipment e)
    {
        foreach (int type in e.affected_unit_types)
        {
            if (unit_ID == type)
            {
                return true;
            }
        }
        return false;
    }

    private bool check_compatible_stat(int stat_ID, Equipment e)
    {
        foreach (int type in e.affected_stats)
        {
            if (stat_ID == type)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveEquipment(string name)
    {
        if (!Inventory.ContainsKey(name))
        {
            return;
        }
        Inventory[name].RemoveAt(0);
        if (Inventory[name].Count <= 0)
        {
            Inventory.Remove(name);
        }
    }

    public int GetEquipmentAmount(string name)
    {
        if (!Inventory.ContainsKey(name))
            return 0;
        return Inventory[name].Count;
    }

    public bool HasEquipped(string name)
    {
        foreach (EquipmentSlot es in EquipmentSlots)
        {
            if (!es.full)
                continue;
            if (es.equipment.name == name)
                return true;
        }
        return false;
    }

    public bool Has(string name)
    {
        return Inventory.ContainsKey(name);
    }

    public void remove_all_equipment()
    {
        Inventory.Clear();
    }
}

public class EquipmentSlot
{
    public Equipment equipment;
    // public bool locked { get; private set; } = false;
    public int num = 0;
    public EquipmentSlot(int num)
    {
        this.num = num;
    }

    public void fill(Equipment e)
    {
        equipment = e;
        e.equipped = true;
        e.activate();
    }

    public void clear()
    {
        if (full)
        {
            equipment.deactivate();
        }
        equipment.equipped = false;
        equipment = null;
    }

    public bool full { get => equipment != null; }
}
