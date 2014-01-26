﻿using UnityEngine;
using System.Collections;

public class EffectManager : MonoBehaviour {

	public static GameObject[] ml_SpecialEffects;

	void Start ()
	{
		ChildBehaviour.childToToy += ActivateEffects;
		ChildBehaviour.toyToChild += TerminateEffects;

		ml_SpecialEffects = GameObject.FindGameObjectsWithTag("Effects");
	}

	public static void ActivateEffects ()
	{

		// Activate ALL scripts
		foreach (GameObject effect in EffectManager.ml_SpecialEffects)
		{
			effect.SetActive(true);
			MonoBehaviour[] scriptComponents = effect.GetComponents<MonoBehaviour>();
			foreach(MonoBehaviour script in scriptComponents) {
				script.enabled = true;
			}
		}
	}

	public static void TerminateEffects ()
	{
		// Activate ALL scripts
		foreach (GameObject effect in EffectManager.ml_SpecialEffects)
		{
			effect.SetActive(false);
			/*
			MonoBehaviour[] scriptComponents = effect.GetComponents<MonoBehaviour>();
			foreach(MonoBehaviour script in scriptComponents) {
				script.enabled = false;
			}
			*/
		}
	}

}
