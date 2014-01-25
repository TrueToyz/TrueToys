using UnityEngine;
using System.Collections;

public class ChildBehaviour : MonoBehaviour {

	/* Child-related Variables */
	public GameObject m_ToyInHand; // Toy actually hold in hand
	private GameObject m_ChildHand; // The child left hand, recognizable by his tag

	/* Environment related variables */
	public GameObject Environment;

	/* Switch the environment */
	public delegate void WorldChanger();
	public static event WorldChanger childToToy;
	public static event WorldChanger toyToChild;
	
	// Use this for initialization
	public void Start () {
		m_ChildHand = GameObject.FindGameObjectWithTag("ChildHand");
	}
	
	// Update is called once per frame
	public void Update () {
		TestInteraction();
	}

	/* ------------------------------------ Control functions --------------------------- */

	/*
	 * Take control of child, moving VR root hierarchy to se through his eyes and contro l his hand
	 * */
	void TakeControl ()
	{
		toyToChild(); // Launch all callbacks
	}

	/*
	 * Release control of child
	 * Automatically release toy if in hand
	 * Pause Child ?
	 * */
	void ReleaseControl () 
	{
		childToToy();
	}

	/* --------------------------------------- Interaction functions --------------------------- */


	/*
	 * Grab the given toy, if close enough
	 * */
	void Grab (GameObject toy)
	{
		m_ToyInHand = toy;
		m_ToyInHand.transform.parent = m_ChildHand.transform; // Change toy parent in hierarchy by the hand transform
		m_ToyInHand.transform.localPosition = Vector3.zero; // Optional : put the object in hand

		// Make it kinematic
		Rigidbody toyBody = m_ToyInHand.GetComponent<Rigidbody>();
		toyBody.isKinematic = true;
	}

	/*
	 * Release the toy
	 * */
	void Drop ()
	{
		m_ToyInHand.transform.parent = null; // Toy is not in hierarchy
		m_ToyInHand.transform.position = m_ChildHand.transform.position;

		// Make it physic
		Rigidbody toyBody = m_ToyInHand.GetComponent<Rigidbody>();
		toyBody.isKinematic = false;

		// Erase pointer
		m_ToyInHand = null;
	}

	/* ------------------------------------------ Menu functions ---------------------------------- */

	/*
	 * Pause child when menu is On
	 * */
	void Pause ()
	{

	}

	/* --------------------------------------------- Debug functions --------------------------------- */

	/*
	 * Test if behavior of child is good
	 * */
	void TestInteraction ()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
			if (!m_ToyInHand)
			{
				GameObject ChildToy = GameObject.FindGameObjectWithTag("ChildToy");
				Grab (ChildToy);
			}
			else{
				Drop ();
			}
		}
	}
}
