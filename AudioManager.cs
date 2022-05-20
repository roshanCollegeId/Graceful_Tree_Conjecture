using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public Sound[] sounds;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        
        if (_instance == null) _instance = this;
        else { Destroy(gameObject); return; }

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.playOnAwake = false;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.outputAudioMixerGroup = s.audioMixerGroup;
        }
    }
    
    public void Play(string n)
    {
        Sound s = Array.Find(sounds, sound => sound.name == n);

        s?.source.Play();
    }
    
    public void Stop(string n)
    {
        Sound s = Array.Find(sounds, sound => sound.name == n);

        s?.source.Stop();
    }
}
