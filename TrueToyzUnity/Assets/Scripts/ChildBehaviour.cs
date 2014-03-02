using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;
using System.Collections.Generic;

public class ChildBehaviour : MonoBehaviour {

	/* Child-related Variables */
	public 	GameObject 	m_ChildToy; // Toy actually hold in hand
	private GameObject 	m_ChildHand; // The child left hand, recognizable by his tag
	private	GameObject	m_dropIndicator;
	private Animator 	m_HandAnimator;
	private GameObject 	cameraVR;

	/* For automatic generation of hand */
	public GameObject 	m_HandPrefab;

	private bool 		m_ToyInHand = false; // the toy is in hand
	private bool 		m_CanSwitch = true; // the child cannot switch whiel the toy falls
	public	bool		m_canDrop = false; // modified from outside

	/* Environment related variables */
	public 	GameObject 	Environment;
	private	Vector3		m_originalHandPosition;
	private	ParticleSystem	m_moveFeedback;

	/* Events when Switch the environment */
	public delegate void WorldChanger();
	public static event WorldChanger childToToy;
	public static event WorldChanger toyToChild;
	public bool 		m_IsControlled = true;

	/* Vr inputs */
	private	vrButtons	m_WandButtons;
	private float 		timeBeforeNextIteration = 0.0f;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_Music;
	private AudioSource m_ChildAudio;

	// Use this for initialization
	public void Start () 
	{

		// Particle system for world motion
		m_moveFeedback = GetComponentInChildren<ParticleSystem>();

		// Wand retrieval
		m_WandButtons = vrDeviceManager.GetInstance().GetWandButtons();

		// Hand config
		m_ChildHand = GameObject.FindGameObjectWithTag("ChildHand");
		//
		m_dropIndicator =  GameObject.FindGameObjectWithTag("DropIndicator");

		Quaternion myRotation = Quaternion.Euler(0f,90f,0f);
		GameManager.Instance.AttachNodeToHand(m_ChildHand, Vector3.zero, myRotation); // Initialize hand by atatching to vr node

		// Retrieve animator
		m_HandAnimator = m_ChildHand.GetComponent<Animator>();

		// Assign toy
		if (!m_ChildToy)
			m_ChildToy = GameObject.FindGameObjectWithTag("ChildToy");
		
		// Handlers // Callbacks to transition
		childToToy += biggerWorld;
		toyToChild += smallerWorld;

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
			monitorInputs();
	}

	/* ------------------------------------ Control functions --------------------------- */


	// Take control of child, moving VR root hierarchy to se through his eyes and contro l his hand
	void takeControl ()
	{
		m_IsControlled = true;
		if(toyToChild != null)
			toyToChild(); // Launch all callbacks

		// Recreate the hand
		if(m_ChildHand)
			Destroy(m_ChildHand);

		m_ChildHand = (GameObject)Instantiate(m_HandPrefab);
		m_HandAnimator = m_ChildHand.GetComponent<Animator>();
		m_moveFeedback = m_ChildHand.GetComponentInChildren<ParticleSystem>();

		// Attach the hand to the nodes
		GameManager.Instance.AttachNodeToHand(m_ChildHand);

		// Change the root to the origin
		GameManager.Instance.MoveRootTo(gameObject,Vector3.zero,Quaternion.identity);

		// Avoid immediate re-swap
		timeBeforeNextIteration = Time.time;

	}

	// Release control of Toy
	void releaseControl () 
	{
		m_IsControlled = false;
		if(childToToy != null)
			childToToy();

		Destroy(m_ChildHand);
		
	}

	/* --------------------------------------- Interaction functions --------------------------- */

	void	canDrop(bool bValue)
	{
		m_canDrop = bValue;
	}

	bool	canGrab	()
	{
		Collider[] hitColliders = Physics.OverlapSphere(m_ChildHand.transform.position, GameManager.Instance.graspRadius);
		foreach(Collider c in hitColliders)
		{
			if(c.tag == "BuildingBlock" || c.tag == "ChildToy")
			{
				Toy toyScript = c.GetComponent<Toy>() as Toy;
				if(toyScript.m_canBeTaken)
				{
					m_ChildToy = c.gameObject;
					return true;
				}
			}
		}

		return false;
	}

	/*
	bool 	canGrab2 ()
	{
		return Vector3.Distance(m_ChildToy.transform.position, m_ChildHand.transform.position) < GameManager.Instance.graspRadius;
	}
	*/

	void 	grab ()
	{
		timeBeforeNextIteration = Time.time;

		// Interrupt any ongoing motion
		m_ChildToy.SendMessage("take");

		// Acgtivate drop projector
		m_dropIndicator.SendMessage("assignTarget",m_ChildToy);

		m_ChildToy.transform.parent = m_ChildHand.transform; // Change toy parent in hierarchy by the hand transform
		m_ChildToy.transform.localPosition = Vector3.zero; // Optional : put the object in hand
		m_ToyInHand = true;

		// The object is hidden
		if(m_ChildToy.tag == "ChildToy")
		{
			m_ChildToy.SetActive(false);
			m_HandAnimator.SetTrigger("Grasp");
		}
		else
		{
			m_ChildToy.SendMessage("makeKinematic");
		}
		
	}
	
	/*
	 * Release the toy
	 * */
	void 	drop ()
	{
		timeBeforeNextIteration = Time.time;
		m_ToyInHand = false;

		/* Manually move the object above the ground */
		RaycastHit hit;
		ToyUtilities.RayCastToGround(m_ChildToy, out hit);

		// Launch PlayerToy parachute fall
		Toy toyScript = m_ChildToy.GetComponent<Toy>() as Toy;

		m_ChildToy.transform.parent = Environment.transform; // Toy is not in hierarchy

		// Specific behavior for player toy
		if(m_ChildToy.tag == "ChildToy")
		{
			// Orient the toy only on the Y axis
			// TODO: It might be useful to actually CONTROL this orientation !
			Vector3 newRot = m_ChildToy.transform.rotation.eulerAngles;
			newRot.x = 0; 
			newRot.z = 0;
			
			m_ChildToy.transform.rotation = Quaternion.Euler(newRot);

			m_HandAnimator.SetTrigger("Drop");
			m_ChildToy.SetActive(true);
			m_CanSwitch = false;
			
			// Don't forget to specify the callbacks
			toyScript.addCallback(landing);
		}
		else if(m_ChildToy.tag == "BuildingBlock")
		{
			m_ChildToy.transform.rotation = Quaternion.identity;

		}


		m_dropIndicator.SendMessage("clearTarget");
		StartCoroutine(toyScript.ParachuteFall(m_ChildToy,hit.point)); 

	}

	public void	landing()
	{

		m_CanSwitch = true;
	}
	

	/* ------------------------------------------ VR interaction ---------------------------------- */

	void	moveWorld ()
	{
		// Launch feedback
		if(m_moveFeedback)
		{
			if(!m_moveFeedback.isPlaying)
				m_moveFeedback.Play();
		}

		Vector3 worldPosition = transform.position - (GameManager.Instance.vrHandNode.transform.position-m_originalHandPosition);

		// Never change this value
		worldPosition.y = transform.position.y;

		// Chanbge the world !
		transform.position = worldPosition;
	}

	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void 	monitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			/* Grasp or drop */
			if (m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton0()))
			{
				if(!m_ToyInHand)
				{
					if(this.canGrab())
						this.grab ();
					else
					{
						moveWorld();
					}
				}
	
			}
			else
			{
				// When not moving the world around, we monitor the offset between the world and the hand
				m_originalHandPosition = GameManager.Instance.vrHandNode.transform.position;

				// 
				if(m_canDrop && m_ToyInHand)
				{
					this.drop ();
				}
			}


			/* Can control toy only if child has dropped toy */
			if (!m_ToyInHand && m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton1()) && m_CanSwitch)
			{
				releaseControl();
				GameManager.Instance.playerToy.SendMessage("takeControl",gameObject);

				// Avoid constant loop of drop/grab...
				timeBeforeNextIteration = Time.time;
			}
		}

	}

	/* ------------------------------------------ World modification functions ---------------------------------- */

	/*
	 * Prototype to scale the level, called with an event
	 * */
	void 	biggerWorld ()
	{
		Environment.transform.localScale = Vector3.one * GameManager.Instance.swapScale;
		GameManager.Instance.ResetScale();

		m_ChildAudio.Pause();
	}

	void 	smallerWorld ()
	{
		Environment.transform.localScale = Vector3.one;
		GameManager.Instance.ResetScale();

		m_ChildAudio.Play();
	}



}
