using UnityEngine;
using System.Collections;

public class ToyPlayerBehaviour : MonoBehaviour {

	public bool m_IsControlled = false;

	/* Hard coded informations */
	public static Vector3 ms_Forward = new Vector3(-1f,0f,0.0f);

	/* Reference toward the owner-child */
	public GameObject m_OwnerChild;

	/* Weapon prefab */
	public GameObject m_WeaponPrefab;
	private GameObject m_Weapon;
	private GameObject m_Aim;
	
	/* Graphical components of toy */
	private Renderer[] ml_GraphicComponents;

	/* Combat attributes */
	public float m_FireRate = 3f;
	public float m_FireRange = 200f;
	private float timeBeforeNextShot = 0.0f;

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

			// Debug
			if (m_Aim)
				Debug.DrawRay(m_Aim.transform.position, m_Aim.transform.forward*1000,Color.red);
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

		// If it's not the case, the toy must be kinematic
		rigidbody.isKinematic = true;
		//collider.enabled = false;

		// Move VR root to child, relink hand with VR node
		Vector3 offset = -AvatarManager.GetHeadTrackingOffset() ;
		offset.y = 0; // I want to keep the height of the head

		// Instantiate weapon
		if (m_WeaponPrefab)
		{
			m_Weapon = Instantiate(m_WeaponPrefab) as GameObject;
			
			// Attach the gun to the VR hand
			AvatarManager.AttachNodeToHand(m_Weapon);
			m_Aim = m_Weapon.transform.Find("Aim").gameObject;
			
		}

		// Rotate to face the front of the toy. This information is given HARD-CODED in static ms_Forward
		Quaternion rotOffset = Quaternion.FromToRotation(AvatarManager.vrRootNode.transform.forward, ms_Forward); 

		// Apply offset
		AvatarManager.MoveRootTo(gameObject, offset/AvatarManager.swapScale, rotOffset);

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

		// If it's not the case, the toy must returns to normal state
		rigidbody.isKinematic = false;
		//collider.enabled = true;
		
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
		Debug.Log ("Shoot someone!");

		Ray myAim = new Ray(m_Aim.transform.position, m_Aim.transform.forward);
		RaycastHit gunHit;

		Animator gunAnimator = m_Weapon.GetComponent<Animator>();
		gunAnimator.SetTrigger("Shoots");

		int layer1 = LayerMask.NameToLayer("Enemies");
		int layer2 = LayerMask.NameToLayer("Default");

		int layermask1  = 1 << layer1;
		int layermask2 = 1 << layer2;
		int layermask = layermask1 | layermask2;


		// Physical hardcoded raycast
		if (Physics.Raycast(myAim, out gunHit, m_FireRange,layermask))
		{
			if (gunHit.collider.gameObject.tag == "Enemy")
			{
				gunHit.collider.gameObject.SendMessage("ReceiveDamage");
			}
			else
			{
				Debug.Log ("Touché ! :" +gunHit.collider.name);
			}
		}

	}

	/* ------------------------------------------ VR interaction ---------------------------------- */
	
	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void MonitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			/* Swap behavior */
			if (m_HandRazer.IsButtonPressed(6))
			{
				Debug.Log ("Toy to Child");
				m_OwnerChild.SendMessage("TakeControl",gameObject);
				ReleaseControl();

				timeBeforeNextIteration = Time.time;
			}
		}

		/* Shoot behavior */
		if (m_HandRazer.IsButtonPressed(0))
		{
			if(Time.time > timeBeforeNextShot + m_FireRate) 
			{
				Shoot ();
				timeBeforeNextShot = Time.time;
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
