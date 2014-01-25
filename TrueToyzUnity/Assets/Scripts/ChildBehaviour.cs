using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;

public class ChildBehaviour : MonoBehaviour {

	/* Child-related Variables */
	public GameObject m_ChildToy; // Toy actually hold in hand
	private GameObject m_ChildHand; // The child left hand, recognizable by his tag
	private bool m_ToyInHand = false; // the toy is in hand
	private bool m_CanSwitch = true; // the child cannot switch whiel the toy falls

	/* Environment related variables */
	public GameObject Environment;

	/* Events when Switch the environment */
	public delegate void WorldChanger();
	public static event WorldChanger childToToy;
	public static event WorldChanger toyToChild;
	public bool m_IsControlled = true;

	/* Vr inputs */
	private vrJoystick m_HandRazer;
	private float timeBeforeNextIteration = 0.0f;
	
	// Use this for initialization
	public void Start () {

		// Hand config
		m_ChildHand = GameObject.FindGameObjectWithTag("ChildHand");
		AvatarManager.AttachNodeToHand(m_ChildHand); // Initialize hand by atatching to vr node

		// Assign toy
		if (!m_ChildToy)
			m_ChildToy = GameObject.FindGameObjectWithTag("ChildToy");

		// Retrieve inputs
		// Note: Might be not the good index
		m_HandRazer = MiddleVR.VRDeviceMgr.GetJoystickByIndex(0);

		// Handlers // Callbacks to transition
		childToToy += BiggerWorld;
		toyToChild += SmallerWorld;

	}
	
	// Update is called once per frame
	public void Update () {
		if(m_IsControlled)
		{
			//TestInteraction();
			MonitorInputs();
		}
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

		// Avoid immediate re-swap
		timeBeforeNextIteration = Time.time;
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
	bool CanGrab ()
	{
		if(Vector3.Distance(m_ChildToy.transform.position, m_ChildHand.transform.position) < 0.1)
			return true;
		else
			return true;
	}

	/*
	 * Grab the given toy, if close enough
	 * */
	void Grab ()
	{

		m_ChildToy.transform.parent = m_ChildHand.transform; // Change toy parent in hierarchy by the hand transform
		m_ChildToy.transform.localPosition = Vector3.zero; // Optional : put the object in hand
		m_ToyInHand = true;

	}

	/*
	 * Release the toy
	 * */
	void Drop ()
	{
		m_ChildToy.transform.parent = null; // Toy is not in hierarchy
		m_ChildToy.transform.rotation = Quaternion.identity;

		/* Obsolete code */
		// Make it physic
		//Rigidbody toyBody = m_ChildToy.GetComponent<Rigidbody>();
		//toyBody.isKinematic = false;

		// Special: Stop the toy from rotating over itself when it falls
		//toyBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

		// Toy is not in hand
		m_ToyInHand = false;

		/* Manually move the object above the ground */
		Vector3 groundPos = ToyUtilities.RayCastToGround(m_ChildToy);
		groundPos.y += 0.2f; // offset to be hovering over the ground
		StartCoroutine(FallSoldier(m_ChildToy,groundPos)); // Launch coroutine

	}
	

	/*
	 * Note: you should interrupt this coroutine when a new grab has been sent
	 * */
	IEnumerator FallSoldier (GameObject soldier, Vector3 targetPos)
	{
		// Lock the switch ability
		m_CanSwitch = false;
		while(Vector3.Distance(soldier.transform.position, targetPos) > 0.05f)
		{
			if(m_ToyInHand)
				yield break;
			soldier.transform.position = Vector3.Lerp(soldier.transform.position, targetPos, 1.0f * Time.deltaTime);
			yield return null;
		}
		
		Debug.Log("Reached the target.");
		yield return new WaitForSeconds(1f);
		Debug.Log("Fall has ended.");

		// Unlock the switch ability
		m_CanSwitch = true;

	}
	
	
	/* ------------------------------------------ VR interaction ---------------------------------- */

	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void MonitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			/* Grasp or drop */
			if (m_HandRazer.IsButtonPressed(0))
			{
				if(!m_ToyInHand)
				{
					if(CanGrab())
						Grab ();
				}
				else
					Drop ();

				timeBeforeNextIteration = Time.time;
			}
			/* Can control toy only if child has dropped toy */
			else if (!m_ToyInHand && m_HandRazer.IsButtonPressed(6) && m_CanSwitch)
			{
				Debug.Log ("Child to Toy");
				Drop ();
				Debug.Log ("Child has dropped");
				ReleaseControl();
				m_ChildToy.SendMessage("TakeControl",gameObject);

				// Avoid constant loop of drop/grab...
				timeBeforeNextIteration = Time.time;
			}
		}

		// Display help on the ground
		if (m_ToyInHand)
		{


		}
	}

	/* ------------------------------------------ World modification functions ---------------------------------- */

	/*
	 * Prototype to scale the level, called with an event
	 * */
	void BiggerWorld ()
	{
		Environment.transform.localScale = new Vector3(5.0f,5.0f, 5.0f);
	}

	void SmallerWorld ()
	{
		Environment.transform.localScale = new Vector3(1.0f,1.0f, 1.0f);
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
				Grab ();
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
				m_ChildToy.SendMessage("TakeControl",gameObject);
				Drop ();
				ReleaseControl();
			}
		}
	}
}
