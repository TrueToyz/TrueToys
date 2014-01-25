using UnityEngine;
using System.Collections;

public class ChildBehaviour : MonoBehaviour {

	/* Child-related Variables */
	public GameObject m_ToyInHand; // Toy actually hold in hand
	private GameObject m_ChildHand; // The child left hand, recognizable by his tag

	/* Environment related variables */
	public GameObject Environment;

	/* Events when Switch the environment */
	public delegate void WorldChanger();
	public static event WorldChanger childToToy;
	public static event WorldChanger toyToChild;
	public bool m_IsControlled = true;
	
	// Use this for initialization
	public void Start () {
		m_ChildHand = GameObject.FindGameObjectWithTag("ChildHand");
	}
	
	// Update is called once per frame
	public void Update () {
		if(m_IsControlled)
			TestInteraction();
	}

	/* ------------------------------------ Control functions --------------------------- */

	/*
	 * Take control of child, moving VR root hierarchy to se through his eyes and contro l his hand
	 * */
	void TakeControl ()
	{
		m_IsControlled = true;
		if(toyToChild != null)
			toyToChild(); // Launch all callbacks

		AvatarManager.MoveRootTo(gameObject); // Replace Vr hierarchy in child
		AvatarManager.AttachNodeToHand(m_ChildHand); // Relink hand with VR object
	}

	/*
	 * Release control of child
	 * Automatically release toy if in hand
	 * Pause Child ?
	 * */
	void ReleaseControl () 
	{
		m_IsControlled = false;
		if(childToToy != null)
			childToToy();

		// Unlink the hand and the VR hierarchy, relink with child
		m_ChildHand.transform.parent = transform;
	}

	/* --------------------------------------- Interaction functions --------------------------- */


	/*
	 * Verify if the hand is close enough to grab the toy
	 * */
	bool CanGrab (GameObject toy)
	{
		if(Vector3.Distance(toy.transform.position, m_ChildHand.transform.position) < 0.1)
			return true;
		else
			return false;
	}

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

		// Special: Stop the toy from rotating over itself when it falls
		toyBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

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

		// Keyboard motion of the hand
		if (Input.GetKey(KeyCode.UpArrow))
			m_ChildHand.transform.Translate(m_ChildHand.transform.forward * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.DownArrow))
			m_ChildHand.transform.Translate(-m_ChildHand.transform.forward * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.RightArrow))
			m_ChildHand.transform.Translate(m_ChildHand.transform.right * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.LeftArrow))
			m_ChildHand.transform.Translate(-m_ChildHand.transform.right * Time.deltaTime * 3.5f);

		// Control
		if (Input.GetKeyDown(KeyCode.C))
		{
			Debug.Log ("Child to Toy");
			if(m_ToyInHand)
			{
				m_ToyInHand.SendMessage("TakeControl",gameObject);
				Drop ();
				ReleaseControl();
			}
		}
	}
}
