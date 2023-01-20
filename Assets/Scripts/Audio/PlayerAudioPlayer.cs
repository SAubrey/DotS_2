using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAudioPlayer : AudioPlayer
{
    public AudioClip[] footsteps;
    public AudioClip DrawBowClip, FireBowClip, ArrowHitClip;
    public AudioSource DrawBowSource;
    protected override void Start() {
        //footsteps = new AudioClip[2] {};
        
    }

    public void DrawBow() 
    {
        Play(DrawBowSource, DrawBowClip);
    }

    public void FireBow()
    {
        DrawBowSource.Stop();
        Play(FireBowClip);
    }

    public void ArrowHit(GameObject go)
    {
        Play(ArrowHitClip, go);
    }
}
