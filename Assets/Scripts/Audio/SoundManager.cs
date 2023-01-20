using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager I { get; private set; }

    public BackgroundSFXPlayer background_SFX_player;
    public ImpactPlayer impact_player;
    public UIPlayer UI_player;
    public PlayerAudioPlayer playerAudioPlayer;
    public float MasterVolume = 1f;
    
    void Awake() {
        if (I == null) {
            I = this;
        } else {
            Destroy(gameObject);
        }
    }
}

