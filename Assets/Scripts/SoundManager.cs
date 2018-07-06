using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
	// crinkle sound: http://freesound.org/people/volivieri/sounds/37171/

	public AudioClip crinkleAudioClip;
	AudioSource crinkle;

	private void Awake()
	{
		crinkle = AddAudio(crinkleAudioClip);
	}

	AudioSource AddAudio(AudioClip audioClip)
	{
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.clip = audioClip;
		return audioSource;
	}

	public void PlayCrinkle()
	{
		crinkle.Play();
	}
}
