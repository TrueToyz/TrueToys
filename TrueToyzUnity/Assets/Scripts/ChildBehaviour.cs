using UnityEngine;
using System.Collections;
using MiddleVR_Unity3D;
using System.Collections.Generic;

public class ChildBehaviour : MonoBehaviour {

	/* Child-related Variables */
	public 	GameObject 	m_ChildToy; // Toy actually hold in hand
	private GameObject 	m_ChildHand; // The child left hand, recognizable by his tag
	public	GameObject	m_dropIndicator;
	private Animator 	m_HandAnimator;
	private GameObject 	cameraVR;

	/* For automatic generation of hand */
	public GameObject 	m_HandPrefab;

	private bool 		m_ToyInHand = false; // the toy is in hand
	private bool 		m_CanSwitch = true; // the child cannot switch whiel the toy falls
	public	bool		m_canDrop = false; // modified from outside
	private	bool		m_isMoving = false;

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
	
	void 	grab ()
	{
		timeBeforeNextIteration = Time.time;

		// Interrupt any ongoing motion
		m_ChildToy.SendMessage("take");

		// Acgtivate drop projector
		m_dropIndicator.SetActive(true);
		m_dropIndicator.SendMessage("assignTarget",m_ChildToy);

		m_ChildToy.transform.parent = m_ChildHand.transform; // Change toy parent in hierarchy by the hand transform
		m_ChildToy.transform.localPosition = Vector3.zero; // Optional : put the object in hand
		m_ToyInHand = true;

		/* Note: animation onyl concerned the player toy */
		// The object is hidden
		//m_HandAnimator.SetTrigger("Grasp");
		//m_ChildToy.SetActive(false);

	}
	
	/*
	 * Release the toy
	 * */
	void 	drop ()
	{
		timeBeforeNextIteration = Time.time;
		m_ToyInHand = false;

		// Launch PlayerToy parachute fall
		Toy toyScript = m_ChildToy.GetComponent<Toy>() as Toy;

		m_ChildToy.transform.parent = Environment.transform; // Toy is not in hierarchy

		// Specific behavior for player toy
		m_HandAnimator.SetTrigger("Drop");
		if(m_ChildToy.tag == "ChildToy")
		{
			// Orient the toy only on the Y axis
			// TODO: It might be useful to actually CONTROL this orientation !
			Vector3 newRot = m_ChildToy.transform.rotation.eulerAngles;
			newRot.x = 0; 
			newRot.z = 0;
			m_ChildToy.transform.rotation = Quaternion.Euler(newRot);

			// While the player toy is descending, we can't control it
			m_CanSwitch = false;
			
			// Don't forget to specify the callbacks
			toyScript.addCallback(landing);
		}
		else if(m_ChildToy.tag == "BuildingBlock")
		{
			m_ChildToy.transform.rotation = Quaternion.identity;
		}

		// Reenable the collider
		m_ChildToy.SendMessage("drop");

		// Begin fall
		StartCoroutine(toyScript.ParachuteFall(m_ChildToy,m_dropIndicator.transform.position)); 

		// The indicator has no more target
		m_dropIndicator.SendMessage("clearTarget");
		m_dropIndicator.SetActive(false);

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


	void 	monitorInputs ()
	{

		if (Time.time > timeBeforeNextIteration + 0.6f)
		{
			/* Grasp or drop */
			if (m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton0()))
			{
				if(!m_ToyInHand && this.canGrab())
				{
					this.grab ();
				}
			}
			else
			{
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

		// Special : MOTION of the world
		if (m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton0()))
		{
			if(!m_ToyInHand)
			{
				moveWorld();
				timeBeforeNextIteration = Time.time ;
			}
		}
		else
		{
			m_originalHandPosition = GameManager.Instance.vrHandNode.transform.position;
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
