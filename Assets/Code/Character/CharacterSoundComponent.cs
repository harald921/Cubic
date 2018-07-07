using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSoundComponent : MonoBehaviour
{
	public enum CharacterSound
	{
		Walk,
		Dash,
		Punch,
		Death,

		Count,
	}

	AudioSource[] _sounds;

	public void ManualAwake(CharacterDatabase.ViewData data, Transform parent)
	{
		_sounds = new AudioSource[(int)CharacterSound.Count];

		CreateSounds(data, parent);
	}
	
	void CreateSounds(CharacterDatabase.ViewData data, Transform parent)
	{
		GameObject soundHolderWalk = new GameObject("walkSound", typeof(AudioSource));
		soundHolderWalk.transform.SetParent(parent);

		GameObject soundHolderDash = new GameObject("dashSound", typeof(AudioSource));
		soundHolderDash.transform.SetParent(parent);

		GameObject soundHolderPunch = new GameObject("punchSound", typeof(AudioSource));
		soundHolderPunch.transform.SetParent(parent);

		GameObject soundHolderDeath = new GameObject("DeathSound", typeof(AudioSource));
		soundHolderDeath.transform.SetParent(parent);

		_sounds[(int)CharacterSound.Walk] = soundHolderWalk.GetComponent<AudioSource>();
		_sounds[(int)CharacterSound.Walk].clip = data.walkSound;

		_sounds[(int)CharacterSound.Dash] = soundHolderDash.GetComponent<AudioSource>();
		_sounds[(int)CharacterSound.Dash].clip = data.dashSound;

		_sounds[(int)CharacterSound.Punch] = soundHolderPunch.GetComponent<AudioSource>();
		_sounds[(int)CharacterSound.Punch].clip = data.hitSound;

		_sounds[(int)CharacterSound.Death] = soundHolderDeath.GetComponent<AudioSource>();
		_sounds[(int)CharacterSound.Death].clip = data.deathSound;
	}

	public void PlaySound(CharacterSound type)
	{
		_sounds[(int)type].Play();
	}
}
