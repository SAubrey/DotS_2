
using System.Collections.Generic;
using UnityEngine;

// Slot group
public class Group : MonoBehaviour
{
    public const int Up = 0, Down = 180, Left = 90, Right = 270;
    public const int Max = 6;

    public List<Slot> Slots = new List<Slot>();
    public Transform[] SlotPointTransforms = new Transform[0];
    //public GameObject SlotGroup;
    public Deployment Deployment;
    public GameObject PrefabSlot;
    public GameObject SlotParent;
    public GameObject Parent;

    void Awake()
    {
        Parent = transform.parent.gameObject;
        if (SlotParent != null)
        {
            SpawnSlots();
        }
    }

    public void SpawnSlots() {
        for (int i = 0; i < SlotPointTransforms.Length; i++)
        {
            GameObject sgo = Instantiate(PrefabSlot, transform.position, Quaternion.identity, SlotParent.gameObject.transform);
            Slots.Add(sgo.GetComponent<Slot>());
            Slots[i].Group = this;
            Slots[i].Deployment = Deployment;
            Slots[i].SlotPointTransform = SlotPointTransforms[i];
        }
    }

    public void PlaceUnit(Unit unit)
    {
        Slot s = GetHighestEmptySlot();
        if (s != null)
            s.Fill(unit);
    }

    // Couple groups with slots under respective empty group shells.
   /* public void pair_slot_point_group()
    {
        foreach (Transform child in SlotGroup.transform)
        {
            Slots.Add(child.gameObject.GetComponent<Slot>());
        }
        for (int i = 0; i < Slots.Count; i++)
        {
            Slots[i].deployment = Deployment;
            Slots[i].slot_point_transform = SlotPointTransforms[i];
        }
    }*/

    // Moves units up within their group upon vacancies from unit death/movement.
    public void ValidateUnitOrder()
    {
        if (IsEmpty)
            return;

        for (int i = 0; i < Max; i++)
        {
            if (!Slots[i].IsEmpty)
                continue;
            for (int j = i + 1; j < Max; j++)
            {
                if (Slots[j].IsEmpty)
                    continue;
                // Move unit to higher slot
                Slots[i].Fill(Slots[j].Empty(false));
                break;
            }
        }
    }

    public int CountSameUnits(int unitID)
    {
        int numGrouped = 0;
        foreach (Slot s in Slots)
        {
            if (!s.HasUnit)
                continue;
            if (s.Unit.ID == unitID)
                numGrouped++;
        }
        return numGrouped;
    }

    public void Set(int i, Unit u)
    {
        Slots[i].Fill(u);
    }
    public Slot Get(int i)
    {
        return Slots[i];
    }

    public int SumUnitHealth()
    {
        int sum = 0;
        foreach (Slot s in Slots)
        {
            if (s.HasUnit)
            {
                sum += s.Unit.Health;
            }
        }
        return sum;
    }

    public Slot GetHighestFullSlot()
    {
        for (int i = 0; i < Slots.Count; i++)
            if (Slots[i].HasUnit)
                return Slots[i];
        return null;
    }

    public Slot GetHighestEmptySlot()
    {
        for (int i = 0; i < Slots.Count; i++)
        {
            if (Slots[i].IsEmpty)
                return Slots[i];
        }
        return null;
    }

    public Slot GetHighestEnemySlot()
    {
        for (int i = 0; i < Slots.Count; i++)
            if (Slots[i].HasEnemy)
                return Slots[i];
        return null;
    }

    public Slot GetHighestPlayerSlot()
    {
        foreach (Slot s in Slots)
            if (s.HasPunit)
                return s;
        return null;
    }

    public List<Slot> GetFullSlots()
    {
        List<Slot> full_slots = new List<Slot>();
        foreach (Slot s in Slots)
            if (s.HasUnit)
                full_slots.Add(s);
        return full_slots;
    }

    public bool IsEmpty
    {
        get
        {
            foreach (Slot slot in Slots)
                if (!slot.IsEmpty)
                    return false;
            return true;
        }
    }


    public bool IsFull
    {
        get
        {
            foreach (Slot slot in Slots)
                if (slot.IsEmpty)
                    return false;
            return true;
        }
    }
}