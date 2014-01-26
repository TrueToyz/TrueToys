using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;
using System.Collections.Generic;

public class ChildBehaviour : MonoBehaviour {

	/* Balance variables */
	private float m_GraspRadius = 0.35f;

	/* Child-related Variables */
	public GameObject m_ChildToy; // Toy actually hold in hand
	private GameObject m_ChildHand; // The child left hand, recognizable by his tag
	private Animator m_HandAnimator;
	private GameObject cameraVR;

	/* For automatic generation of hand */
	public GameObject m_HandPrefab;

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

	/*Audio*/
	private Dictionary<string,AudioClip> ml_Music;
	private AudioSource m_ChildAudio;

	// Use this for initialization
	public void Start () {

		// Hand config
		m_ChildHand = GameObject.FindGameObjectWithTag("ChildHand");

		Quaternion myRotation = Quaternion.Euler(0f,90f,0f);
		AvatarManager.AttachNodeToHand(m_ChildHand, Vector3.zero, myRotation); // Initialize hand by atatching to vr node

		// Retrieve animator
		m_HandAnimator = m_ChildHand.GetComponent<Animator>();

		// Assign toy
		if (!m_ChildToy)
			m_ChildToy = GameObject.FindGameObjectWithTag("ChildToy");

		// Retrieve inputs
		// Note: Might be not the good index
		m_HandRazer = MiddleVR.VRDeviceMgr.GetJoystickByIndex(0);

		// Handlers // Callbacks to transition
		childToToy += BiggerWorld;
		toyToChild += SmallerWorld;

		ml_Music = new Dictionary<string, AudioClip>();
		ml_Music["Child"] = Resources.Load("Audio/ggjpuppetambchild") as AudioClip;
		
		m_ChildAudio = gameObject.GetComponent<AudioSource>();
		if(!m_ChildAudio)
		{
			gameObject.AddComponent<AudioSource>();
			m_ChildAudio = gameObject.GetComponent<AudioSource>();
		}

		m_ChildAudio.clip = ml_Music["Child"];
		m_ChildAudio.loop = true;
		m_ChildAudio.Play();

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

		// Recreate the hand
		m_ChildHand = (GameObject)Instantiate(m_HandPrefab);
		m_HandAnimator = m_ChildHand.GetComponent<Animator>();

		// Attach the hand to the node
		AvatarManager.AttachNodeToHand(m_ChildHand);
		
		// Change the root to the origin
		AvatarManager.MoveRootTo(gameObject,Vector3.zero,Quaternion.identity);

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
		//m_ChildHand.transform.parent = transform;
		//m_ChildHand.SetActive(false);

		Destroy(m_ChildHand);
		
	}

	/* --------------------------------------- Interaction functions --------------------------- */


	/*
	 * Verify if the hand is close enough to grab the toy
	 * */
	bool CanGrab ()
	{
		
		if(Vector3.Distance(m_ChildToy.transform.position, m_ChildHand.transform.position) < m_GraspRadius)
			return true;
		else
			return false;
	}

	/*
	 * Grab the given toy, if close enough
	 * */
	void Grab ()
	{

		m_ChildToy.transform.parent = m_ChildHand.transform; // Change toy parent in hierarchy by the hand transform
		m_ChildToy.transform.localPosition = Vector3.zero; // Optional : put the object in hand
		m_ToyInHand = true;

		// If it's not the case, the toy must returns to normal state
		m_ChildToy.rigidbody.isKinematic = true;

		// The object is hidden
		m_ChildToy.SetActive(false);

		m_HandAnimator.SetTrigger("Grasp");
		
	}

	/*
	 * Stops player from putting the toy into the wall !
	 * */
	bool CanDrop ()
	{
		/*
		int layer2 = LayerMask.NameToLayer("Default");
		int layermask2 = 1 << layer2;
		Collider[] potentialThreats = Physics.OverlapSphere(m_ChildHand.transform.position,0.1f,layermask2);
		foreach( Collider other in potentialThreats)
		{
			Debug.Log (other.name);
		}

		return true;
		*/

		// TODO

		return true;

	}

	/*
	 * Release the toy
	 * */
	void Drop ()
	{
		m_ChildToy.transform.parent = Environment.transform; // Toy is not in hierarchy

		// Orient the toy
		Vector3 newRot = m_ChildToy.transform.rotation.eulerAngles;
		newRot.x = 0; 
		newRot.z = 0;

		m_ChildToy.transform.rotation = Quaternion.Euler(newRot);
		
		// Toy is not in hand
		m_ToyInHand = false;

		/* Manually move the object above the ground */
		Vector3 groundPos = ToyUtilities.RayCastToGround(m_ChildToy);
		
		m_HandAnimator.SetTrigger("Drop");
		
		// Launch coroutine
		StartCoroutine(FallSoldier(m_ChildToy,groundPos)); 

	}
	

	/*
	 * Note: you should interrupt this coroutine when a new grab has been sent
	 * */
	IEnumerator FallSoldier (GameObject soldier, Vector3 targetPos)
	{
		// Give time for animation
		yield return new WaitForSeconds(0.4f);

		// The object is not
		m_ChildToy.SetActive(true);

		// Lock the switch ability
		m_CanSwitch = false;

		// Only keep the Z component
		Vector3 newTarget = new Vector3(soldier.transform.position.x, targetPos.y, soldier.transform.position.z);

		// PArachute !
		soldier.SendMessage("OpenParachute");

		while(Vector3.Distance(soldier.transform.position, newTarget) > 0.01f)
		{
			if(m_ToyInHand)
				yield break;
			soldier.transform.position = Vector3.Lerp(soldier.transform.position, newTarget, 4f * Time.deltaTime);
			yield return null;
		}
		
		Debug.Log("Reached the target.");
		yield return new WaitForSeconds(0.5f);
		Debug.Log("Fall has ended.");

		soldier.SendMessage("CloseParachute");

		// the toy must returns to normal state
		m_ChildToy.rigidbody.isKinematic = false;
		m_ChildToy.rigidbody.AddForce(-m_ChildToy.transform.up *50); // One of the worst hack ever
		
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
				else if(CanDrop())
				{
					Drop ();
				}

				timeBeforeNextIteration = Time.time;
			}
			/* Can control toy only if child has dropped toy */
			else if (!m_ToyInHand && m_HandRazer.IsButtonPressed(6) && m_CanSwitch)
			{
				Debug.Log ("SWAP: Child to Toy");
				ReleaseControl();
				m_ChildToy.SendMessage("TakeControl",gameObject);

				// Avoid constant loop of drop/grab...
				timeBeforeNextIteration = Time.time;
			}
		}

		// Display help on the ground
		if (m_ToyInHand)
		{
			// TODO : particles ? 

		}
	}

	/* ------------------------------------------ World modification functions ---------------------------------- */

	/*
	 * Prototype to scale the level, called with an event
	 * */
	void BiggerWorld ()
	{
		Environment.transform.localScale = new Vector3(5.0f,5.0f, 5.0f);
		AvatarManager.ResetScale();

		m_ChildAudio.Pause();
	}

	void SmallerWorld ()
	{
		Environment.transform.localScale = new Vector3(1.0f,1.0f, 1.0f);
		AvatarManager.ResetScale();

		m_ChildAudio.Play();
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
