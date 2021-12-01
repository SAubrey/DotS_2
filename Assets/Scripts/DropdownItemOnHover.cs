using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using TMPro;

public class DropdownItemOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData data)
    {
        GameObject go = data.pointerEnter;
        EquipmentUI.I.LoadEquipmentDescription(go.GetComponent<TMP_Text>().text);
    }

    public void OnPointerExit(PointerEventData data)
    {
        EquipmentUI.I.HideEquipmentDescription();
    }
}
