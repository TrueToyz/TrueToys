﻿using UnityEngine;
using System.Collections;

public class ToyPlayerBehaviour : MonoBehaviour {

	public bool m_IsControlled = false;

	/* Reference toward the owner-child */
	public GameObject m_OwnerChild;

	/* Weapon prefab */
	public GameObject m_WeaponPrefab;
	private GameObject m_Weapon;
	
	/* Graphical components of toy */
	private Renderer[] ml_GraphicComponents;

	/* Combat attributes */
	public float m_FireRate = 0.1f;

	/* Vr inputs */
	private vrJoystick m_HandRazer;
	private float timeBeforeNextIteration = 0.0f;

	// Use this for initialization
	public void Start () {
		// Retrieve inputs
		// Note: Might be not the good index
		m_HandRazer = MiddleVR.VRDeviceMgr.GetJoystickByIndex(0);

		// Retrieve renderer components
		ml_GraphicComponents = GetComponentsInChildren<Renderer>() as Renderer[]; 
	}
	
	// Update is called once per frame
	public void Update () {
		if(m_IsControlled)
		{
			//TestInteraction();
			MonitorInputs();
		}
	}

	/* ------------------------------------ Effects functions --------------------------- */

	/*
	 * Show or Hides graphical aspects of soldier during control
	 * */
	void ShowSoldier (bool bState)
	{
		foreach(Renderer graphic_comp in ml_GraphicComponents)
		{
			graphic_comp.enabled = bState;
		}
	}

	
	/* ------------------------------------ Control functions --------------------------- */
	
	/*
	 * Take control of the toy
	 * Moves VR root node hierarchy to toy
	 * Display weapon
	 * */
	void TakeControl (GameObject child)
	{
		m_IsControlled = true;
		m_OwnerChild = child;

		// Move VR root to child, relink hand with VR node
		AvatarManager.MoveRootTo(gameObject, -AvatarManager.GetHeadTrackingOffset() / AvatarManager.swapScale);

		// Instantiate weapon
		if (m_WeaponPrefab)
		{
			m_Weapon = Instantiate(m_WeaponPrefab) as GameObject;

			// Attach the gun to the VR hand
			AvatarManager.AttachNodeToHand(m_Weapon);
		}

		// Hide character
		ShowSoldier(false);

		// Avoid immediate swap
		timeBeforeNextIteration = Time.time;

	}
	
	/*
	 * Release control of the toy, notifies the child
	 * Hides the weapon
	 * */
	void ReleaseControl () 
	{
		m_IsControlled = false;
		m_OwnerChild = null;

		if (m_Weapon)
			Destroy(m_Weapon);

		// Show character
		ShowSoldier(true);
	}

	/* ------------------------------------------ Actions ---------------------------------- */

	/*
	 * Shoot'em while they're on fire !
	 * Throw a projectile
	 * */
	void Shoot ()
	{
		// Do stuff
	}

	/* ------------------------------------------ VR interaction ---------------------------------- */
	
	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void MonitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			/* Shoot behavior */
			if (m_HandRazer.IsButtonPressed(0))
			{
				Shoot ();
			}
			/* Swap behavior */
			else if (m_HandRazer.IsButtonPressed(6))
			{
				Debug.Log ("Toy to Child");
				m_OwnerChild.SendMessage("TakeControl",gameObject);
				ReleaseControl();

				timeBeforeNextIteration = Time.time;
			}
		}
	}

	/* --------------------------------------------- Debug functions --------------------------------- */
	
	/*
	 * Test if behavior of child is good
	 * */
	void TestInteraction ()
	{

		// Keyboard motion of the toy
		if (Input.GetKey(KeyCode.UpArrow))
			transform.Translate(transform.forward * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.DownArrow))
			transform.Translate(-transform.forward * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.RightArrow))
			transform.Translate(transform.right * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.LeftArrow))
			transform.Translate(-transform.right * Time.deltaTime * 3.5f);
		
		// Control
		if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.Log ("Toy to child");
			m_OwnerChild.SendMessage("TakeControl");
			ReleaseControl();
		}
	}

}
