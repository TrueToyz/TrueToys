using UnityEngine;
using System.Collections;

public class EnnemyAI : MonoBehaviour {

	/* Hard coded */
	public static int ms_EnemyCount =0;

	public float chaseSpeed;
	public float chaseWaitTime;
	public float m_AttackRange = 5.0f;


	public int m_LifePoints; // Easy to kill

	private EnnemySight ennemySight;
	private LastPlayerSighting lastPlayerSighting;
	private GameObject player;
	private float chaseTimer;
	private Vector3 nextMovePosition;
	private bool isBlocked; //Means by a collision
	private int radius = 2; // Radius of anticipitation for collisions in patrolling case
	private bool m_IsFrozen = true;

	/* Behavior */
	public enum EnemyBehaviour{chasing,attacking,patrolling};
	private EnemyBehaviour m_EnemyBehaviour = EnemyBehaviour.patrolling;

	/* PArticle effects */
	public GameObject m_DeathPrefab;
	public GameObject m_InjuryPrefab;

	/*Audio*/
	private GameObject cameraVR;
	private AudioClip hurt1;
	private AudioClip hurt2;
	private AudioClip running;
	private AudioClip die;

	private Animator m_EnemyAnimator;

	void Start () {
		//Spawn
		//TODO

		ms_EnemyCount++;

		m_LifePoints = Random.Range(1, 5);
		chaseSpeed = Random.Range(0.3f, 0.5f); // Not to slow, not too fast :D
		chaseWaitTime = Random.Range(1.8f, 2.1f);

		m_EnemyAnimator = gameObject.GetComponent<Animator>();

		hurt1 = Resources.Load("Audio/hurt1") as AudioClip;
		hurt2 =  Resources.Load("Audio/hurt2") as AudioClip;
		running = Resources.Load("Audio/running") as AudioClip;
		die = Resources.Load("Audio/die") as AudioClip;
		
		cameraVR = GameObject.Find("CameraStereo0");
		//cameraVR.AddComponent<AudioListener>();
		cameraVR.AddComponent<AudioSource>();
	}

	void Awake () {
		
		player = GameObject.FindGameObjectWithTag("ChildToy");
		ennemySight = GetComponentInChildren<EnnemySight>();
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();

		ChildBehaviour.toyToChild += Freeze;
		ChildBehaviour.childToToy += Unfreeze;
	}

	void Update () {

		/* */
		if(!m_IsFrozen)
		{
			if(m_EnemyBehaviour != EnemyBehaviour.chasing)
			{
				Debug.Log ("Enter once here");
				if(Vector3.Distance(transform.position, player.transform.position) < m_AttackRange)
				{
					m_EnemyBehaviour = EnemyBehaviour.attacking;
					Attack ();
				}
				else if(ennemySight.playerInSight){
					m_EnemyBehaviour = EnemyBehaviour.chasing;
					m_EnemyAnimator.SetBool("IsRunning", true);
					
					/* MAnages routines */
					StartCoroutine(Chase (gameObject,player));
				}
			}

		}
	
	}

	/*
	 * Replaces old behavior
	 * */
	IEnumerator Chase (GameObject soldier, GameObject player)
	{
		soldier.transform.LookAt(player.transform.position);
		soldier.transform.Rotate(new Vector3(0f,1f,0f), 90);

		// Only keep the Z component
		while(Vector3.Distance(soldier.transform.position, player.transform.position) > m_AttackRange)
		{
			if(m_IsFrozen || !ennemySight.playerInSight)
				yield break;

			//Audio
			cameraVR.audio.Stop();
			cameraVR.audio.clip = running;
			cameraVR.audio.Play();

			// Translation
			soldier.transform.position = Vector3.Lerp(soldier.transform.position, player.transform.position, chaseSpeed * Time.deltaTime);

			// Rotation
			//Quaternion consigne = Quaternion.LookRotation(player.transform.position -soldier.transform.position);
			//soldier.transform.rotation = Quaternion.RotateTowards(soldier.transform.rotation,consigne,50);
			yield return null;
		}
		
		m_EnemyBehaviour = EnemyBehaviour.patrolling;
	}

	void Patrolling () {
		Debug.Log("Patrolling");
		//Update the futur position
		nextMovePosition = nextMovePosition + transform.forward * Time.deltaTime;
		Vector3 destination = nextMovePosition - transform.position ;

		//Test the collision of the next move
		RaycastHit hit;

		if (Physics.Raycast(transform.position, destination.normalized, out hit, radius)) {
			Debug.Log("COLLISION : CHANGE DIRECTION");
			//Compute a new destination
			float discFactor = 50;
			float i = Random.Range(1, discFactor);
			Vector3 circle = new Vector3 ( radius*Mathf.Cos(2*i*Mathf.PI / discFactor ), radius*Mathf.Sin(2*i*Mathf.PI / discFactor), 0);
			Vector3 randomPosition = transform.position + circle;
			nextMovePosition = randomPosition;

			destination = nextMovePosition - transform.position;
		}

		//Move the enemy
		transform.Translate(destination.normalized * chaseSpeed * Time.deltaTime);

	}

	
	/* --------------------------------------------- Fight behaviour ------------------------------ */
	
	void Attack ()
	{
		Debug.Log ("Attack !");
		m_EnemyAnimator.SetTrigger("Attack");
	}

	void ReceiveDamage ()
	{
		Debug.Log ("Oh, it hurts !");
		m_LifePoints--;
		
		Injured();
		
		/* Death Behaviour */
		if (m_LifePoints < 1)
		{
			Die();
		}
	}

	void Injured ()
	{
		m_EnemyAnimator.SetTrigger("TakeInjury");

		// Blood particles and sound
		if (m_InjuryPrefab)
		{
			Instantiate(m_InjuryPrefab, transform.position, transform.rotation);

			//Audio
			int random = Random.Range(1, 2);
			if (random == 1 ){
				cameraVR.audio.Stop();
				cameraVR.audio.clip = hurt1;
				cameraVR.audio.Play();
			}
			else if (random == 2) {
				cameraVR.audio.Stop();
				cameraVR.audio.clip = hurt2;
				cameraVR.audio.Play();
			}
		}
	}

	void Die ()
	{
		// Launch particles and sound
		if (m_DeathPrefab)
		{
			Vector3 newPosition =  transform.position;
			newPosition.y += 0.25f;
			Instantiate(m_DeathPrefab, newPosition, transform.rotation);

			cameraVR.audio.Stop();
			cameraVR.audio.clip = die;
			cameraVR.audio.Play();
		}

		ms_EnemyCount --;

		Destroy (gameObject);

	}
	/* -------------------------------------------- Pause during swapping ---------------------- */

	void Freeze () {
		m_IsFrozen = true;
		m_EnemyBehaviour = EnemyBehaviour.patrolling;
		if (m_EnemyAnimator)
		{
			m_EnemyAnimator.SetBool("IsRunning", false);
			m_EnemyAnimator.SetTrigger("Freeze");
		}
	}

	void Unfreeze () {
		m_IsFrozen = false;
	}

}
