using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToyPlayerBehaviour : MonoBehaviour {

	public bool m_IsControlled = false;

	/* Hard coded informations */
	public static Vector3 ms_Forward = new Vector3(-1f,0f,0.0f);

	/* Reference toward the owner-child */
	public GameObject m_OwnerChild;

	/* Weapon prefab and subcomponents */

	private GameObject m_Weapon;
	private GameObject m_Aim; // Canon of the gun, -X as the forward vector
	private GameObject m_Loader; 
	private GameObject m_Parachute;

	public GameObject m_WeaponPrefab;
	public GameObject m_ShellPrefab; // Emitter to be instantiated
	public GameObject m_BulletPrefab; // OBject to be launched at gunshot
	public GameObject m_BlastPrefab; // Effecdt to apply when gunshot
	public GameObject m_ParachutePrefab;
	
	/* Graphical components of toy */
	private Renderer[] ml_GraphicComponents;

	/* Combat attributes */
	public float m_FireRate = 3f;
	public float m_FireRange = 200f;
	public int m_ShellNumbers = 3;
	public float m_FireRadius = 0.1f;//
	private float timeBeforeNextShot = 0.0f;

	/* Vr inputs */
	private vrJoystick m_HandRazer;
	private float timeBeforeNextIteration = 0.0f;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_ActionSounds;
	private AudioClip m_MusicToy;
	private AudioSource m_PlayerAudio;

	// Use this for initialization
	public void Start () {
		// Retrieve inputs
		// Note: Might be not the good index
		m_HandRazer = MiddleVR.VRDeviceMgr.GetJoystickByIndex(0);

		// Retrieve renderer components
		ml_GraphicComponents = GetComponentsInChildren<Renderer>() as Renderer[]; 

		ml_ActionSounds = new Dictionary<string, AudioClip>();
		ml_ActionSounds["Shot1"] = Resources.Load("Audio/bullet1") as AudioClip;
		ml_ActionSounds["Shot2"] = Resources.Load("Audio/bullet11") as AudioClip;
		ml_ActionSounds["Shot3"] = Resources.Load("Audio/bullet3") as AudioClip;
		ml_ActionSounds["Shot4"] = Resources.Load("Audio/bullet7") as AudioClip;
		ml_ActionSounds["Shot5"] = Resources.Load("Audio/bullet2") as AudioClip;
		ml_ActionSounds["Shot6"] = Resources.Load("Audio/bullet9") as AudioClip;
		m_MusicToy = Resources.Load ("Audio/ggjpuppetwar") as AudioClip;
		
		m_PlayerAudio = gameObject.GetComponent<AudioSource>();
		if(!m_PlayerAudio)
		{
			gameObject.AddComponent<AudioSource>();
			m_PlayerAudio = gameObject.GetComponent<AudioSource>();
		}

		Debug.Log ("Start player");
		m_PlayerAudio.clip = m_MusicToy;
		m_PlayerAudio.loop = true;
		m_PlayerAudio.volume = 0.5f;
		m_PlayerAudio.Stop ();

	}
	
	// Update is called once per frame
	public void Update () {
		if(m_IsControlled)
		{
			//TestInteraction();
			MonitorInputs();

			// Debug
			if (m_Aim)
				Debug.DrawRay(m_Aim.transform.position, m_Aim.transform.forward*m_FireRange,Color.red);
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

		m_PlayerAudio.Play();

		// If it's not the case, the toy must be kinematic
		rigidbody.isKinematic = true;

		// Move VR root to child, relink hand with VR node
		Vector3 offset = -AvatarManager.GetHeadTrackingOffset() ;
		offset.y = 0.2f; // I want to keep the height of the head

		// Instantiate weapon
		if (m_WeaponPrefab)
		{
			m_Weapon = Instantiate(m_WeaponPrefab) as GameObject;
			
			// Attach the gun to the VR hand
			AvatarManager.AttachNodeToHand(m_Weapon);
			m_Aim = m_Weapon.transform.FindChild("Aim").gameObject;
			
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

		m_PlayerAudio.Pause();

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

		// Animation
		Animator gunAnimator = m_Weapon.GetComponent<Animator>();
		gunAnimator.SetTrigger("Shoots");

		//Audio
		int randDeath = Random.Range(1, 6);
		AudioSource.PlayClipAtPoint(ml_ActionSounds["Shot" + randDeath], transform.position);

		// Emitter of shells
		if(m_ShellPrefab){
			GameObject myShells = (GameObject)Instantiate(m_ShellPrefab);
			myShells.transform.parent = m_Aim.transform;
			myShells.transform.localPosition = Vector3.zero;
			Destroy(myShells,m_FireRate);
		}

		// Gunblast
		if (m_BlastPrefab)
		{
			GameObject myBlast = (GameObject)Instantiate(m_BlastPrefab);
			myBlast.transform.parent = m_Aim.transform;
			myBlast.transform.localPosition = new Vector3(0f,0f,0.15f);
			Destroy(myBlast,m_FireRate);
		}
		
		// Random generation of bullets
		for(int i=0; i<m_ShellNumbers; i++)
		{
			float offset_x = Random.Range(0f,m_FireRadius);
			float offset_z = Random.Range(0f,m_FireRadius);

			// Random direction
			Vector3 randomAim = m_Aim.transform.forward;
			randomAim.z += offset_z;
			randomAim.x += offset_x;
		
			Ray myAim = new Ray(m_Aim.transform.position, randomAim);
			RaycastHit gunHit;

			// Emitter of bullets
			if(m_BulletPrefab)
			{
				Quaternion bulletRot = Quaternion.FromToRotation(new Vector3(0,0,1f), randomAim);
				GameObject myBullet = (GameObject)Instantiate(m_BulletPrefab,myAim.GetPoint(0),bulletRot);
				StartCoroutine(BulletBehavior(myBullet,myAim,2f));
			}

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

	}

	/*
	 * Launches a bullet toward a given direction which ends at a given distance
	 * */
	IEnumerator BulletBehavior(GameObject bullet, Ray direction, float distance) 
	{
		
		while(Vector3.Distance(bullet.transform.position, direction.GetPoint(distance)) > 0.01f)
		{
			bullet.transform.position = Vector3.Lerp(bullet.transform.position, direction.GetPoint(distance), 12f * Time.deltaTime);
			yield return null;
		}

		Destroy(bullet);
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

	/* --------------------------------------------- Parachute ----------------------------------------------- */

	void OpenParachute ()
	{
		if (m_ParachutePrefab)
		{
			m_Parachute = Instantiate(m_ParachutePrefab,Vector3.zero, Quaternion.identity) as GameObject;
			m_Parachute.transform.parent = transform;
			m_Parachute.transform.localPosition = new Vector3(0f,0.05f,0f);
			m_Parachute.transform.localRotation = Quaternion.Euler(-90f,0f,0f);
		
			Destroy(m_Parachute,2f);
		}
	}

	void CloseParachute ()
	{
		if (m_Parachute)
			Destroy(m_Parachute);
	}

	/* --------------------------------------------- GUI ----------------------------------------------- */

	void OnGUI()
	{
		if(m_IsControlled)
		{

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
