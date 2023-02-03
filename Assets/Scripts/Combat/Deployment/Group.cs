
using System.Collections.Generic;
using UnityEngine;

// Slot group
public class Group : MonoBehaviour
{
    public const int Max = 6;

    public List<Slot> Slots = new List<Slot>();
    public Transform[] SlotPointTransforms = new Transform[0];
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
            Slots[i].SlotPointTransform = SlotPointTransforms[i];
        }
    }

    public void PlaceUnit(Unit unit)
    {
        Slot s = GetHighestEmptySlot();
        if (s != null)
            s.Fill(unit);
    }

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

    public Slot Get(int i)
    {
        return Slots[i];
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