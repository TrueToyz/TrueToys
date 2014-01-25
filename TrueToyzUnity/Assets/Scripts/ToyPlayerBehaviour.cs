using UnityEngine;
using System.Collections;

public class ToyPlayerBehaviour : MonoBehaviour {

	public bool m_IsControlled = false;

	/* Reference toward the owner-child */
	public GameObject m_OwnerChild;

	// Use this for initialization
	public void Start () {
	
	}
	
	// Update is called once per frame
	public void Update () {
		if(m_IsControlled)
			TestInteraction();
	}
	
	
	/* ------------------------------------ Control functions --------------------------- */
	
	/*
	 * Take control of the toy
	 * Moves VR root node hierarchy
	 * */
	void TakeControl (GameObject child)
	{
		m_IsControlled = true;
		m_OwnerChild = child;

		// Move VR root to child, relink hand with VR node
		AvatarManager.MoveRootTo(gameObject);
	}
	
	/*
	 * Release control of the toy, notifies the child
	 * */
	void ReleaseControl () 
	{
		m_IsControlled = false;
		m_OwnerChild = null;
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
