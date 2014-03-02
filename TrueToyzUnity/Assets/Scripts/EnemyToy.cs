using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyToy : Toy {
	
	
	public 	float 	m_attackRange = 5.0f;
	public	float	m_attackSpeed = 1.0f;

	// AI variables
	private	float	m_chaseWaitTime;
	private float	m_chaseSpeed;
	private	int		m_lifePoints;
	private float 	m_chaseTimer;
	private Vector3 m_destination;
	private bool 	m_isBlocked; //Means by a collision
	private int 	m_radius = 2; // Radius of anticipitation for collisions in patrolling case

	// Sight
	public	ToySight	m_sight;

	/* Behavior */
	public 	enum 			EnemyBehaviour{chasing,attacking,patrolling};
	private EnemyBehaviour 	m_enemyBehaviour = EnemyBehaviour.patrolling;

	/* Effects */
	public 	GameObject 	m_deathPrefab;
	public 	GameObject 	m_injuryPrefab;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_actionSounds;
	private AudioSource 	m_enemyAudio;
	private Animator		m_enemyAnimator;

	void Start () 
	{
		m_sight = gameObject.GetComponentInChildren<ToySight>();
		m_enemyAnimator = gameObject.GetComponent<Animator>();
		m_enemyAudio = gameObject.GetComponent<AudioSource>();

		// Randomize characteristics
		m_lifePoints = Random.Range(1, 5);
		m_chaseSpeed = Random.Range(0.3f, 0.5f); // Not to slow, not too fast :D
		m_chaseWaitTime = Random.Range(1.8f, 2.1f);


		ml_actionSounds = new Dictionary<string, AudioClip>();
		ml_actionSounds["Hurt1"] = Resources.Load("Audio/scream1") as AudioClip;
		ml_actionSounds["Hurt2"] =  Resources.Load("Audio/scream2") as AudioClip;
		ml_actionSounds["Running"] = Resources.Load("Audio/running") as AudioClip;
		ml_actionSounds["Attack"] = Resources.Load("Audio/scream3") as AudioClip;
		ml_actionSounds["Die1"] = Resources.Load("Audio/explo3") as AudioClip;
		ml_actionSounds["Die2"] = Resources.Load("Audio/explo4") as AudioClip;
		ml_actionSounds["Die3"] = Resources.Load("Audio/explo5") as AudioClip;
	}

	void Awake () {
		ChildBehaviour.toyToChild += freeze;
		ChildBehaviour.childToToy += unfreeze;
	}

	// Note: Necessary to include any physicaly related behavior in fixed update
	void FixedUpdate () {

		if(!m_isFrozen)
		{
			if(m_enemyBehaviour == EnemyBehaviour.patrolling)
			{
				if(m_sight.gameObjectsOnSight.Contains(GameManager.Instance.playerToy))
				{
					m_enemyBehaviour = EnemyBehaviour.chasing;
					m_enemyAnimator.SetBool("IsRunning", true);
					m_enemyAudio.clip = ml_actionSounds["Running"];
					m_enemyAudio.Play();
				}
				else
				{
					transform.Rotate(0f,2f,0f);
				}
			}
			else
			{
				if(Vector3.Distance(transform.position, GameManager.Instance.playerToy.transform.position) < m_attackRange)
				{
					if(Time.time - m_chaseTimer > m_attackSpeed)
					{
						m_chaseTimer = 0f;
						m_enemyAnimator.SetTrigger("Attack");
						GameManager.Instance.playerToy.SendMessage("malusPower");
					}
					else
					{
						m_chaseTimer+= Time.deltaTime;
					}
				}
				else
				{
					transform.LookAt(GameManager.Instance.playerToy.transform.position);
					transform.Rotate(new Vector3(0f,1f,0f), 90); // Offset because of character wrong orientation

					// Move toward player
					Vector3 newPosition = GameManager.Instance.playerToy.transform.position - transform.position;

					// Take physics into account
					rigidbody.MovePosition(rigidbody.position +  m_chaseSpeed * Time.deltaTime * newPosition);

				}

				// If player disappears
				if(!m_sight.gameObjectsOnSight.Contains(GameManager.Instance.playerToy))
				{
					m_enemyBehaviour = EnemyBehaviour.patrolling;
					m_enemyAnimator.SetBool("IsRunning", false);
					m_enemyAudio.Pause();
				}
			}

		}
	
	}
	

	/* --------------------------------------------- Fight behaviour ------------------------------ */
	
	void receiveDamage ()
	{
		// TODO: solve this impossible behavior
		if(m_lifePoints <1)
		{
			return;
		}

		if (--m_lifePoints < 1)
			die();
		else
			injured();
	}

	void injured ()
	{
		m_enemyAnimator.SetTrigger("TakeInjury");

		// Blood particles and sound
		if (m_injuryPrefab)
		{
			Instantiate(m_injuryPrefab, transform.position, transform.rotation);

			//Audio
			int randHurt = Random.Range(1, 2);
			AudioSource.PlayClipAtPoint(ml_actionSounds["Hurt"+randHurt], transform.position);
		}
	}

	void die ()
	{
		// Launch particles and sound
		if (m_deathPrefab)
		{
			Vector3 newPosition =  transform.position;
			newPosition.y += 0.25f;
			GameObject death = Instantiate(m_deathPrefab, newPosition, transform.rotation) as GameObject;
			Destroy(death, death.GetComponent<ParticleSystem>().duration);

			int randDeath = Random.Range(1, 3);
			AudioSource.PlayClipAtPoint(ml_actionSounds["Die" + randDeath], transform.position);
		}

		/*
		 * Note: Very strange behaviour: if you destroy "gameObject", the script is still... existing ! but why ?
		 * */
		Destroy (this);
		Destroy (gameObject);
	}

	
	/* -------------------------------------------- Pause during swapping ---------------------- */

	public void freeze () {

		// Don't move !
		m_isFrozen = true;
		m_enemyBehaviour = EnemyBehaviour.patrolling;

		// Interrupt any parachute fall
		interrupt();

		if (m_enemyAnimator)
		{
			m_enemyAnimator.SetBool("IsRunning", false);
			m_enemyAnimator.SetTrigger("Freeze");
		}
	}

	public	void unfreeze () {
		m_isFrozen = false;
	}

}
