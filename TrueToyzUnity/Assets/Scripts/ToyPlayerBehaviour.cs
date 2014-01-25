using UnityEngine;
using System.Collections;

public class ToyPlayerBehaviour : MonoBehaviour {

	public bool m_IsControlled = false;

	/* Reference toward the owner-child */
	public GameObject m_OwnerChild;

	/* Weapon prefab */
	public GameObject m_WeaponPrefab;
	private GameObject m_Weapon;

	/* Vr inputs */
	private vrJoystick m_HandRazer;
	private float timeBeforeNextIteration = 0.0f;

	// Use this for initialization
	public void Start () {
		// Retrieve inputs
		// Note: Might be not the good index
		m_HandRazer = MiddleVR.VRDeviceMgr.GetJoystickByIndex(0);
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
	 * Take control of the toy
	 * Moves VR root node hierarchy to toy
	 * Display weapon
	 * */
	void TakeControl (GameObject child)
	{
		m_IsControlled = true;
		m_OwnerChild = child;

		// Move VR root to child, relink hand with VR node
		AvatarManager.MoveRootTo(gameObject);

		// Instantiate weapon
		if (m_WeaponPrefab)
		{
			m_Weapon = Instantiate(m_WeaponPrefab,Vector3.zero, Quaternion.identity) as GameObject;
			m_Weapon.transform.parent = transform;
		}

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
	}

	/* ------------------------------------------ VR interaction ---------------------------------- */
	
	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void MonitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			if (m_HandRazer.IsButtonPressed(6))
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
