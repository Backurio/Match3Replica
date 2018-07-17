using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class to manage all game sounds
/// </summary>
public class SoundManager : MonoBehaviour
{
	// crinkle sound: http://freesound.org/people/volivieri/sounds/37171/

	public AudioClip crinkleAudioClip;
	AudioSource crinkle;

	/// <summary>
	/// Initialize sounds
	/// </summary>
	private void Awake()
	{
		crinkle = AddAudio(crinkleAudioClip);
	}

	/// <summary>
	/// Add an audioclip to the audioSource
	/// </summary>
	/// <param name="audioClip">Audioclip to be added to the audioSource</param>
	/// <returns></returns>
	AudioSource AddAudio(AudioClip audioClip)
	{
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.clip = audioClip;
		return audioSource;
	}

	/// <summary>
	/// Play the crinkle sound
	/// </summary>
	public void PlayCrinkle()
	{
		crinkle.Play();
	}
}
