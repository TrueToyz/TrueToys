using UnityEngine;
using System.Collections;

public class EnnemyAI : MonoBehaviour {

	public float chaseSpeed = 0.05f;
	public float chaseWaitTime = 2f;
	public float m_AttackRange = 0.05f;
	public Vector3 spawnPosition;
	public float spawnWaitTime = 2f;

	public int m_LifePoints = 2; // Easy to kill

	private EnnemySight ennemySight;
	private LastPlayerSighting lastPlayerSighting;
	private GameObject player;
	private GameObject enemy;
	private float chaseTimer;
	private float spawnTimer;
	private bool isFrozen = true;

	void Awake () {

		enemy = GameObject.FindGameObjectWithTag("Enemy");
		player = GameObject.FindGameObjectWithTag("ChildToy");
		ennemySight = GetComponentInChildren<EnnemySight>();
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();

		ChildBehaviour.toyToChild += Freeze;
		ChildBehaviour.childToToy += Unfreeze;
	}

	void Start () {
		//Spawn
		spawnTimer += Time.deltaTime;
		
		if(spawnTimer >= spawnWaitTime){
			Instantiate(enemy, spawnPosition, Quaternion.identity);
			spawnTimer = 0f;
		}
	}

	void Update () {
		if (ennemySight.playerInSight && !isFrozen) {
			Chasing ();
		}
	}

	void Chasing () {
	
		Vector3 sightingDeltaPos = ennemySight.personalLastSighting - transform.position;

		// If the the last personal sighting of the player is not close...
		if(sightingDeltaPos.sqrMagnitude > m_AttackRange) {
			Debug.Log("Chasing !");

			Vector3 destination = player.transform.position - transform.position ;
			transform.Translate(destination.normalized * chaseSpeed * Time.deltaTime);

			// Timer
			chaseTimer += Time.deltaTime;

			if(chaseTimer >= chaseWaitTime)
			{
				Debug.Log(" ------------ TIME OUT : quit chasing -------------- ");
				lastPlayerSighting.position = lastPlayerSighting.resetPosition;
				ennemySight.personalLastSighting = lastPlayerSighting.resetPosition;
				chaseTimer = 0f;
			}
		}


	}

	/* --------------------------------------------- Fight behaviour ------------------------------ */

	void ReceiveDamage ()
	{
		Debug.Log ("Oh, it hurts !");
		m_LifePoints--;

		if (m_LifePoints < 2)
		{
			Injured();
		}
		/* Death Behaviour */
		else if (m_LifePoints < 1)
		{
			Die();
		}
	}

	void Injured ()
	{
		// Blood particles and sound
	}

	void Die ()
	{
		// Launch particles and sound

		Destroy (gameObject);

	}
	/* -------------------------------------------- Pause during swapping ---------------------- */

	void Freeze () {
		isFrozen = true;
	}

	void Unfreeze () {
		isFrozen = false;
	}

}
