using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerToy : Toy {

	public 	bool 	m_IsControlled = false;

	/* Hard coded informations */
	public static Vector3 ms_Forward = new Vector3(-1f,0f,0.0f);

	/* Combat attributes */
	public 	float 	m_FireRate = 3f;
	public 	float 	m_FireRange = 200f;
	public 	int 	m_ShellNumbers = 3;
	public 	float 	m_FireRadius = 0.1f;
	private float 	timeBeforeNextShot = 0.0f;

	/* Reference toward the owner-child */
	public 	GameObject 	m_OwnerChild;

	/* Weapon prefab and subcomponents */
	private GameObject 	m_Weapon;
	private GameObject 	m_Aim; // Canon of the gun, -X as the forward vector

	public 	GameObject 	m_WeaponPrefab;
	public	GameObject	m_BulletPrefab;
	
	/* Graphical components of toy */
	private Renderer[] 	ml_GraphicComponents;
	
	/* Vr inputs */
	private		vrButtons	m_WandButtons;
	private 	float 	timeBeforeNextIteration = 0.0f;

	/*Audio*/
	private 	Dictionary<string,AudioClip> ml_ActionSounds;
	private 	AudioClip 	m_MusicToy;
	private 	AudioSource m_PlayerAudio;

	// Use this for initialization
	public void Start () 
	{
		// Player toy can always be taken
		m_canBeTaken = true;

		// Wand retrieval
		m_WandButtons = vrDeviceManager.GetInstance().GetWandButtons();

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

		m_PlayerAudio.clip = m_MusicToy;
		m_PlayerAudio.loop = true;
		m_PlayerAudio.volume = 0.5f;
		m_PlayerAudio.Stop ();

	}
	
	// Update is called once per frame
	public override void Update () {

		// Behavior
		base.Update();

		// Controls
		if(m_IsControlled)
			monitorInputs();
	}

	/* ------------------------------------ Effects functions --------------------------- */


	// Show or Hides graphical aspects of soldier during control
	void showSoldier (bool bState)
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
	void takeControl (GameObject child)
	{
		m_IsControlled = true;
		m_OwnerChild = child;

		m_PlayerAudio.Play();

		// Move VR root to child, relink hand with VR node
		// This offset allows to move the root in order to have the head exactly where the toy was located
		Vector3 offset = GameManager.Instance.vrRootNode.transform.position-GameManager.Instance.vrHeadNode.transform.position;

		offset.y = 0.2f; // An offset is necessary to keep the height of the head at a correct position

		// Instantiate weapon
		if (m_WeaponPrefab)
		{
			m_Weapon = Instantiate(m_WeaponPrefab) as GameObject;
			
			// Attach the gun to the VR hand
			GameManager.Instance.AttachNodeToHand(m_Weapon);
			m_Aim = m_Weapon.transform.FindChild("Aim").gameObject;
			
		}

		// Rotate to face the front of the toy. This information is given HARD-CODED in static ms_Forward
		// Beware, it must be used to rotate the translation offset between head and root
		// TODO: Make sure there is no cleaner alternative
		Quaternion rotOffset = Quaternion.FromToRotation(GameManager.Instance.vrRootNode.transform.forward, ms_Forward); 

		// Apply offset
		GameManager.Instance.MoveRootTo(gameObject, rotOffset*offset/GameManager.Instance.swapScale, rotOffset);

		// Hide character
		showSoldier(false);

		// Avoid immediate swap
		timeBeforeNextIteration = Time.time;

	}
	
	/*
	 * Release control of the toy, notifies the child
	 * Hides the weapon
	 * */
	void releaseControl () 
	{
		m_IsControlled = false;
		m_OwnerChild = null;

		m_PlayerAudio.Pause();
		
		if (m_Weapon)
			Destroy(m_Weapon);

		// Show character
		showSoldier(true);
	}

	/* ------------------------------------------ Actions ---------------------------------- */

	/*
	 * Shoot'em while they're on fire !
	 * Throw multiple projectiles
	 * */
	void shoot ()
	{
		// Animation
		Animator gunAnimator = m_Weapon.GetComponent<Animator>();
		gunAnimator.SetTrigger("Shoots");

		//Audio
		int randDeath = Random.Range(1, 6);
		AudioSource.PlayClipAtPoint(ml_ActionSounds["Shot" + randDeath], transform.position);

		// Emitter of shells
		ParticleSystem[] particles = m_Weapon.GetComponentsInChildren<ParticleSystem>();
		foreach(ParticleSystem p in particles)
		{
			p.Play();
		}

		// Random generation of bullets
		for(int i=0; i<m_ShellNumbers; i++)
		{
			// Random direction
			Vector3 randomAim = m_Aim.transform.forward;
			randomAim.z += Random.Range(-m_FireRadius,m_FireRadius);
			randomAim.x += Random.Range(-m_FireRadius,m_FireRadius);
		
			Ray myAim = new Ray(m_Aim.transform.position, randomAim);
			RaycastHit gunHit;

			// Emitter of bullets
			if(m_BulletPrefab)
			{
				Quaternion bulletRot = Quaternion.FromToRotation(new Vector3(0,0,1f), randomAim);
				GameObject myBullet = (GameObject)Instantiate(m_BulletPrefab,myAim.GetPoint(0),bulletRot);

				// Launch bullet
				float velocity = Random.Range(1f,3f);
				float distance = Random.Range(9f,13f);
				StartCoroutine(bulletBehavior(myBullet,myAim,velocity,distance));
			}

			// We use raycasting, not the bullets themselves
			if (Physics.Raycast(myAim, out gunHit, m_FireRange,1 << LayerMask.NameToLayer("Enemies") << LayerMask.NameToLayer("Default")))
			{
				if (gunHit.collider.gameObject.tag == "Enemy")
				{
					gunHit.collider.gameObject.SendMessage("receiveDamage",SendMessageOptions.DontRequireReceiver);
				}
			}

		}

	}
	
	IEnumerator bulletBehavior(GameObject bullet, Ray direction, float distance, float velocity) 
	{
		// As long as the bullet has not travelled enough distance
		while(Vector3.Distance(direction.origin, bullet.transform.position) < distance)
		{
			bullet.transform.position += (direction.GetPoint(distance)-bullet.transform.position).normalized * velocity * Time.deltaTime;
			yield return null;
		}
		Destroy(bullet);
	}


	public	void	bonusPower()
	{
		m_ShellNumbers++;
	}

	public	void	malusPower()
	{
		m_ShellNumbers--;
		if(m_ShellNumbers<1)
			m_ShellNumbers = 1;
	}

	/* ------------------------------------------ VR interaction ---------------------------------- */
	
	/*
	 * Monitor VR inputs for grabbing/dropping
	 * */
	void monitorInputs ()
	{
		if (Time.time > timeBeforeNextIteration + 0.8f)
		{
			/* Swap behavior */
			if (m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton1()))
			{
				m_OwnerChild.SendMessage("takeControl",gameObject);
				releaseControl();
				timeBeforeNextIteration = Time.time;
			}
		}

		/* Shoot behavior */
		if (m_WandButtons.IsPressed(vrDeviceManager.GetInstance().GetWandButton0()))
		{
			if(Time.time > timeBeforeNextShot + m_FireRate) 
			{
				shoot ();
				timeBeforeNextShot = Time.time;
			}
		}
	}
	

}
