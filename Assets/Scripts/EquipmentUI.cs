using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EquipmentUI : MonoBehaviour
{
    public static EquipmentUI I { get; private set; }
    public TMP_Dropdown dd0, dd1, dd2, dd3;
    public GameObject descriptionP;
    public TMP_Text descriptionT;
    public Dictionary<TMP_Dropdown, int> Dropdowns = new Dictionary<TMP_Dropdown, int>();
    //public Dictionary<TMP_Dropdown, string> selected_equipment = new Dictionary<TMP_Dropdown, string>();

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

    void Start()
    {
        Dropdowns.Add(dd0, 0);
        Dropdowns.Add(dd1, 1);
        Dropdowns.Add(dd2, 2);
        Dropdowns.Add(dd3, 3);
        foreach (TMP_Dropdown d in Dropdowns.Keys)
        {
            d.onValueChanged.AddListener(delegate { EquipmentSelected(d); });
            //d.OnPointerEnter();//AddListener(delegate {load_equipment_description(d);} );
        }
        TurnPhaser.I.OnDiscChange += LoadDiscipline;
        foreach (Discipline d in TurnPhaser.I.Discs.Values)
        {
            d.OnResourceChange += RegisterResourceChange;
        }
    }

    public void Init(Discipline d)
    {
        LoadDiscipline(d);
    }

    public void RegisterResourceChange(int ID, string field, int amount, int c, int cap)
    {
        if (ID != TurnPhaser.I.ActiveDiscID)
            return;
        if (field == Discipline.Experience)
        {
            UnlockSlots(TurnPhaser.I.GetDisc(ID).EquipmentInventory);
        }
    }

    public bool selecting = false;
    public void EquipmentSelected(TMP_Dropdown d)
    {
        selecting = true;
        string e = d.captionText.text;

        EquipmentInventory ei = TurnPhaser.I.ActiveDisc.EquipmentInventory;
        // Undo the selection, the same equipment cannot be worn more than once,
        // and only one of a type can be worn at once.
        if (ei.HasEquipped(e) || ei.GetEquipmentAmount(e) > 1)
        {
            if (ei.EquipmentSlots[Dropdowns[d]].full)
            {
                string name = ei.EquipmentSlots[Dropdowns[d]].equipment.name;
                d.value = d.options.FindIndex(option => option.text == name);
            }
            else
            {
                d.value = d.options.FindIndex(option => option.text == "-");
            }
            return;
        }
        // Unequip existing worn item.
        if (ei.EquipmentSlots[Dropdowns[d]].full)
        {
            ei.unequip(Dropdowns[d]);
        }
        HideEquipmentDescription();
        ei.equip(e, Dropdowns[d]);
    }

    public void LoadDiscipline(Discipline disc)
    {
        EquipmentInventory ei = disc.EquipmentInventory;
        UnlockSlots(ei);
        FillDropdowns(ei);
    }

    public void UnlockSlots(EquipmentInventory ei)
    {
        if (ei == null)
            return;
        int highest_unlocked_slot = ei.GetHighestUnlockedSlot();

        foreach (TMP_Dropdown d in Dropdowns.Keys)
        {
            ActivateEquipmentSlot(d, Dropdowns[d] <= highest_unlocked_slot);
        }
    }

    public void FillDropdowns(EquipmentInventory ei)
    {
        foreach (TMP_Dropdown d in Dropdowns.Keys)
        {
            FillDropdown(d, ei);
            SelectEquipped(ei);
        }
    }

    // Clear and rebuild dropdown options on addition of an equipment option.
    private void FillDropdown(TMP_Dropdown dropdown, EquipmentInventory ei)
    {
        ClearDropdownOptions(dropdown);
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (string e in ei.Inventory.Keys)
        {
            if (ei.Inventory[e].Count > 1)
            {
                options.Add(new TMP_Dropdown.OptionData(ei.Inventory[e].Count.ToString() + " " + ei.Inventory[e][0].name));
            }
            else
            {
                Debug.Log(options);
                Debug.Log(ei.Inventory[e]);
                Debug.Log(ei.Inventory[e][0]);
                Debug.Log(ei.Inventory[e][0].name);
                options.Add(new TMP_Dropdown.OptionData(ei.Inventory[e][0].name));
            }
        }
        dropdown.AddOptions(options);
    }

    private void SelectEquipped(EquipmentInventory ei)
    {
        foreach (TMP_Dropdown d in Dropdowns.Keys)
        {
            EquipmentSlot es = ei.EquipmentSlots[Dropdowns[d]];
            if (!es.full)
                continue;
            d.captionText.text = es.equipment.name;
        }
    }

    private void ClearDropdownOptions(TMP_Dropdown dropdown)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<TMP_Dropdown.OptionData>() { new TMP_Dropdown.OptionData("Empty") });
    }

    public void LoadEquipmentDescription(EquipmentInventory ei, string e)
    {
        if (!ei.HasEquipped(e))
            return;
        descriptionP.SetActive(true);
        descriptionT.text = ei.Inventory[e][0].description;
    }

    public void LoadEquipmentDescription(string e)
    {
        if (e == "Empty")
            return;
        EquipmentInventory ei = TurnPhaser.I.ActiveDisc.EquipmentInventory;
        if (!ei.Has(e))
            return;
        descriptionP.SetActive(true);
        descriptionT.text = ei.Inventory[e][0].description;
    }

    public void HideEquipmentDescription()
    {
        descriptionP.SetActive(false);
    }

    private void ActivateEquipmentSlot(TMP_Dropdown d, bool state)
    {
        d.interactable = state;
    }
}
