using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum MixerGroup
{
    Master,
    Dialogue,
    Music,
    Sound
}

public class SoundManager
{
    private const string VolumePostfix = "Volume";
    private AudioMixer masterMixer;
    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
    private Dictionary<MixerGroup, AudioMixerGroup> audioMixerGroups = new Dictionary<MixerGroup, AudioMixerGroup>();
    
    public void Awake()
    {
        masterMixer = Resources.Load<AudioMixer>("Mixers/MasterMixer");
        FindAudioMixerGroups();
        SetSavedVolumes();
    }

    private void FindAudioMixerGroups()
    {
        foreach (MixerGroup group in System.Enum.GetValues(typeof(MixerGroup))) {
            audioMixerGroups.Add(group, masterMixer.FindMatchingGroups(group.ToString())[0]);
        }
    }

    private void SetSavedVolumes()
    {
        foreach (MixerGroup group in System.Enum.GetValues(typeof(MixerGroup)))
        {
            string volumeName = GetMixerGroupVolumeName(group);
            SetVolume(group, PlayerPrefs.GetFloat(volumeName), false);
        }
    }

    private string GetMixerGroupVolumeName(MixerGroup group)
    {
        return group.ToString() + VolumePostfix;
    }

    private float LinearToDecibel(float linearValue)
    {
        return Mathf.Log10(linearValue) * 20;
    }

    private float DecibelToLinear(float decibelValue)
    {
        return Mathf.Pow(10.0f, decibelValue / 20.0f);
    }

    public void SetVolume(MixerGroup group, float value, bool save = true)
    {
        string volumeName = GetMixerGroupVolumeName(group);
        float decibelValue = LinearToDecibel(value);
        masterMixer.SetFloat(volumeName, decibelValue);
        if (save)
        {
            PlayerPrefs.SetFloat(volumeName, value);
        }
    }

    public float GetVolume(MixerGroup group)
    {
        string volumeName = GetMixerGroupVolumeName(group);
        masterMixer.GetFloat(volumeName, out float value);
        return DecibelToLinear(value);
    }

    private AudioSource GetAudioSource(string resource)
    {
        if (!audioSources.ContainsKey(resource))
        {
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSources.Add(resource, audioSource);
            return audioSource;
        }
        return audioSources[resource];
    }

    public AudioSource Play(MixerGroup group, string resource, float volume = 1f)
    {
        AudioClip clip = Resources.Load<AudioClip>(resource);
        AudioSource audioSource = GetAudioSource(resource);
        audioSource.outputAudioMixerGroup = audioMixerGroups[group];
        audioSource.PlayOneShot(clip, volume);
        return audioSource;
    }
}