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

public class SoundManager : Singleton<SoundManager>
{
	private const string VolumePostfix = "Volume";
	private AudioMixer masterMixer;
	private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();
	private Dictionary<MixerGroup, AudioMixerGroup> audioMixerGroups = new Dictionary<MixerGroup, AudioMixerGroup>();

	public void Awake()
	{
		masterMixer = Resources.Load<AudioMixer>("Mixers/MasterMixer");
		FindAudioMixerGroups();
	}

    public void Start()
    {
		SetSavedVolumes();
	}

    private void FindAudioMixerGroups()
	{
		foreach (MixerGroup group in System.Enum.GetValues(typeof(MixerGroup)))
		{
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
		float decibelValue = Mathf.Log10(linearValue) * 20;
		if (decibelValue < -80f) decibelValue = -80f;
		return decibelValue;
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

	private AudioSource GetAudioSource(AudioClip audioClip)
	{
		if (!audioSources.ContainsKey(audioClip.name))
		{
			AudioSource audioSource = gameObject.AddComponent<AudioSource>();
			audioSources.Add(audioClip.name, audioSource);
			return audioSource;
		}
		return audioSources[audioClip.name];
	}

	public AudioSource Play(MixerGroup group, string resource, float volume = 1f)
	{
		AudioClip clip = Resources.Load<AudioClip>(resource);
		AudioSource audioSource = GetAudioSource(resource);
		audioSource.outputAudioMixerGroup = audioMixerGroups[group];
		audioSource.PlayOneShot(clip, volume);
		return audioSource;
	}

    public AudioSource Play(MixerGroup group, AudioClip clip, float volume = 1f)
    {
		AudioSource audioSource = GetAudioSource(clip);
		audioSource.outputAudioMixerGroup = audioMixerGroups[group];
		audioSource.PlayOneShot(clip, volume);
		return audioSource;
	}

	public void PlayClipAtPoint(MixerGroup group, AudioClip clip, Vector3 position, float volume = 1f)
    {
		GameObject gameObject = new GameObject("One shot audio");
		gameObject.transform.position = position;
		AudioSource audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
		audioSource.outputAudioMixerGroup = audioMixerGroups[group];
		audioSource.clip = clip;
		audioSource.spatialBlend = 1f;
		audioSource.volume = volume;
		audioSource.Play();
		Object.Destroy(gameObject, clip.length * (Time.timeScale < 0.009999999776482582 ? 0.01f : Time.timeScale));
	}
}