using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactPlayer : AudioPlayer {
    
    public AudioClip impact_medium_dry;
    public AudioClip impact_medium_dry1;
    public AudioClip impact_fat_wet;
    private AudioClip[] impact_mediums;
    public AudioClip blocked;
    public AudioClip dead_sound;

    protected override void Start() {
        base.Start();
        impact_mediums = new AudioClip[2] {impact_medium_dry, impact_medium_dry1};
    }

    public void PlayHitFromDamage(GameObject go, int dmg, bool dead) {
        
        if (dmg <= 0) {
            Play(blocked, go);
        } else if (dmg <= 2) {
            Play(ChooseRandomClip(impact_mediums), go);
        } else {
            Play(impact_fat_wet, go);
        } 
        
        if (dead) {
            //play(dead_sound);
        }
    }
}
