using UnityEngine;
using System.Collections;

public class ChildBehaviour : MonoBehaviour {

	public GameObject m_ToyInHand; // Toy actually hold in hand
	
	// Use this for initialization
	public void Start () {
	
	}
	
	// Update is called once per frame
	public void Update () {
	
	}

	/* ------------------------------------ Control functions --------------------------- */

	/*
	 * Take control of child, moving VR root hierarchy to se through his eyes and contro l his hand
	 * */
	void TakeControl ()
	{

	}

	/*
	 * Release control of child
	 * Automatically release toy if in hand
	 * Pause Child ?
	 * */
	void ReleaseControl () 
	{

	}

	/* --------------------------------------- Interaction functions --------------------------- */


	/* ------------------------------------------ Menu functions ---------------------------------- */

	/*
	 * Pause child when menu is On
	 * */
	void Pause ()
	{

	}
}
