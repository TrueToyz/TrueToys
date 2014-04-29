﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyToy : Toy {
	
	
	public 	float 	m_attackRange = 0.8f;
	public	float	m_attackSpeed = 1.0f;
	private float 	m_attackTimer;

	// Sight
	public	ToySight	m_sight;
	private	GameObject	m_target;
	
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
		
		ml_actionSounds = new Dictionary<string, AudioClip>();
		ml_actionSounds["Hurt1"] = Resources.Load("Audio/scream1") as AudioClip;
		ml_actionSounds["Hurt2"] =  Resources.Load("Audio/scream2") as AudioClip;
		ml_actionSounds["Running"] = Resources.Load("Audio/running") as AudioClip;
		ml_actionSounds["Attack"] = Resources.Load("Audio/scream3") as AudioClip;
		ml_actionSounds["Die1"] = Resources.Load("Audio/explo3") as AudioClip;
		ml_actionSounds["Die2"] = Resources.Load("Audio/explo4") as AudioClip;
		ml_actionSounds["Die3"] = Resources.Load("Audio/explo5") as AudioClip;


		// Special pro-feature
		hasLanded += landing;
	}
	
	void Awake () {
		ChildBehaviour.toyToChild += freeze;
		ChildBehaviour.childToToy += unfreeze;
	}

	bool	SeeTarget ()
	{

		GameObject target = null;

		// Player is seen
		// Update: Do not target the player
		/*
		if(m_sight.gameObjectsOnSight.Contains(GameManager.Instance.playerToy))
		{
			target = GameManager.Instance.playerToy;
		}
		*/

		// Else, the enemy attacks building blocks
		foreach(GameObject potentialTarget in m_sight.gameObjectsOnSight)
		{
			if(potentialTarget != null && potentialTarget.tag == "BuildingBlock")
			{
				if(target != null)
				{
					if(Vector3.Distance(transform.position, potentialTarget.transform.position) < Vector3.Distance(transform.position, target.transform.position))
					{
						target = potentialTarget;
					}
				}
				else
				{
					target = potentialTarget;
				}
			}
		}

		if(target != null)
		{
			m_target = target;
			return true;
		}
		else
		{
			m_target = null;
			return false;
		}
	}
	
	// 
	void FixedUpdate () {

		//
		if(!m_isFrozen)
		{
			if(m_enemyBehaviour == EnemyBehaviour.patrolling)
			{
				if(SeeTarget())
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
				if(m_target == null || !SeeTarget())
				{
					m_enemyBehaviour = EnemyBehaviour.patrolling;
					m_enemyAnimator.SetBool("IsRunning", false);
					m_enemyAudio.Pause();
				}
				else if(Vector3.Distance(transform.position, m_target.transform.position) < m_attackRange)
				{
					if(m_attackTimer > m_attackSpeed)
					{
						m_attackTimer = 0f;
						m_enemyAnimator.SetTrigger("Attack");

						// Note: if object is destroyed, it becomes a problem
						m_target.SendMessage("receiveDamage",SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						m_attackTimer+= Time.deltaTime;
					}
				}
				else
				{
					transform.LookAt(m_target.transform.position);

					// Offset because of character wrong orientation
					// TODO: might need a cleaner modification of the mesh/prefab
					transform.Rotate(new Vector3(0f,1f,0f), 90); 

					// Special navmesh agent
					NavMeshAgent navigation = GetComponent<NavMeshAgent>();
					if(navigation)
					{
						navigation.SetDestination(m_target.transform.position);
					}
				}

			}
			
		}
		
	}
	
	/*
	 * Note: Navmesh agent MUST BE created over a navmesh.
	 * Or you could change its position to adhere to the navmesh
	 * */
	public	void	landing()
	{
		NavMeshHit closestHit;
		if(NavMesh.SamplePosition(transform.position,out closestHit, 1000,1))
		{
			transform.position = closestHit.position;

			// Create nav Mesh agent now that it can be done
			gameObject.AddComponent<NavMeshAgent>();
			NavMeshAgent navigator = GetComponent<NavMeshAgent>();

			// Hardcoded navmesh configuration
			navigator.radius = 0.05f;
			navigator.speed = 1.5f;
			navigator.acceleration = 2.5f;
			navigator.height = 0.12f;
			navigator.stoppingDistance = m_attackRange;
		}
	}
	
	/* --------------------------------------------- Fight behaviour ------------------------------ */
	
	public override	void receiveDamage ()
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

		// Notify the level
		m_owner.SendMessage("toyIsDestroyed",gameObject);

		// Bonus block
		GameManager.Instance.blockSpawn.SendMessage("increase");

		/*
		 * Note: Very strange behaviour: if you destroy "gameObject", the script is still... existing ! but why ?
		 * */
		Destroy (this);
		Destroy (gameObject);
	}
	
	
	/* -------------------------------------------- Pause during swapping ---------------------- */
	
	void freeze () {
		m_isFrozen = true;
		m_enemyBehaviour = EnemyBehaviour.patrolling;

		// Freeze animation
		if (m_enemyAnimator)
		{
			m_enemyAnimator.SetBool("IsRunning", false);
			m_enemyAnimator.SetTrigger("Freeze");
		}

		// Freeze navmesh
		// Note: Why do I need to verify this ?
		if(this != null && gameObject != null)
		{
			NavMeshAgent navigation = GetComponent<NavMeshAgent>();
			if(navigation)
			{
				navigation.enabled = false;
			}
		}
	}
	
	void unfreeze () {
		m_isFrozen = false;

		// Freeze navmesh
		if(this != null && gameObject != null)
		{
			NavMeshAgent navigation = GetComponent<NavMeshAgent>();
			if(navigation)
			{
				navigation.enabled = true;
			}
		}
	}
	
}
