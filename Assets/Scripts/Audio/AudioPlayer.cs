using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AudioPlayer : MonoBehaviour {
    private List<AudioSource> Sources = new List<AudioSource>();
    private List<AudioSource> SpatialSources = new List<AudioSource>();
    public GameObject SpatialSourcePrefab;

    protected virtual void Start() {
    }

    // Play from non-looping, full volume, dynamically sized list of sources. Source becomes spatial if passed a parent object.
    public virtual void Play(AudioClip clip, GameObject parent=null, float volume=1f) {
        AudioSource source;
        if (parent != null)
        {
            source = GetFreeSpatialSource();
            MoveSpatialAudioSource(source, parent);
        } else
        {
            source = GetFreeSource();
        } 
        
        Play(source, clip, volume);
    }

    public void Play(AudioSource source, AudioClip clip, float volume=1f) {
        source.clip = clip;
        source.volume = volume * SoundManager.I.MasterVolume;
        source.Play();
    }

    protected AudioSource GetFreeSource()
    {
        foreach (AudioSource s in Sources) {
            if (!s.isPlaying) {
                return s;
            }
        }
        // Create and add a new source.
        AudioSource source = gameObject.AddComponent<AudioSource>();
        Sources.Add(source);
        return source;
    }

    protected AudioSource GetFreeSpatialSource()
    {
        ClearList();
        // Avoid using AudioSource.isPlaying, 300ms+ latency.
        // Create and add a new source.
        GameObject go = GameObject.Instantiate(SpatialSourcePrefab);
        AudioSource source = go.GetComponent<AudioSource>();
        SpatialSources.Add(source);
        return source;
    }

    private void ClearList()
    {
        // Clean list first.
        List<int> remove = new List<int>();
        for (int i = 0; i < SpatialSources.Count; i++)
        {
            if (SpatialSources[i] == null)
                remove.Add(i);
        }

        foreach (var j in remove)
        {
            SpatialSources.RemoveAt(j);
            Debug.Log("removing " + j);
        }
    }

    public void MoveSpatialAudioSource(AudioSource source, GameObject go)
    {
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.transform.SetParent(go.transform);
        source.transform.position = source.transform.parent.position;
    }

    public AudioClip ChooseRandomClip(AudioClip[] clips) {
        int clip_index = Random.Range(0, clips.Length);
        return clips[clip_index];
    }

    public float GetRandomDelay(float min, float max) {
        return Random.Range(min, max);
    }

    public float GetRandomPitch() {
        return Random.Range(.95f, 1.05f);
    }
}