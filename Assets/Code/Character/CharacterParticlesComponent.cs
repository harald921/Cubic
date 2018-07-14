﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterParticlesComponent : MonoBehaviour
{
	CharacterDatabase.ViewData _data;

	ParticleSystem _trail;
	ParticleSystem _hit;

	public void ManualAwake(CharacterDatabase.ViewData data, Transform parent)
	{
		_data = data;
		CreateTrail(parent);
	}

	void CreateTrail(Transform parent)
	{
		if (_data.trailParticle == null)
			return;

		_trail = Instantiate(_data.trailParticle, transform.position, _data.trailParticle.transform.rotation, parent);
		_trail.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
	}

	public void EmitTrail(bool emit)
	{
		if (_data.trailParticle == null)
			return;

		if (emit)
			_trail.Play(true);
		else
			_trail.Stop(true, ParticleSystemStopBehavior.StopEmitting);
	}

	public void SpawnHitEffect(Vector2DInt a, Vector2DInt b)
	{
		if (_data.hitParticle == null)
			return;

		// spawn hit particle abit away from dahing player in the direction of player getting dashed
		Vector3 spawnPosition = new Vector3(a.x, 1, a.y) + ((new Vector3(b.x, 1, b.y) - new Vector3(a.x, 1, a.y)) * 2.0f);
		ParticleSystem p = Instantiate(_data.hitParticle, spawnPosition, _data.hitParticle.transform.rotation);
		Destroy(p, 8);
	}

}
