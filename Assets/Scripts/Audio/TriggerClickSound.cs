using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TriggerClickSound : MonoBehaviour, IPointerDownHandler {
    public void OnPointerDown(PointerEventData data) {
        SoundManager.I.UI_player.Play(UIPlayer.CLICK);
    }
}
