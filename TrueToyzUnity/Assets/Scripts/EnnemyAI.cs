using UnityEngine;
using System.Collections;

public class EnnemyAI : MonoBehaviour {

	public float chaseSpeed = 0.05f;
	public float chaseWaitTime = 2f;
	public float m_AttackRange = 0.05f;


	public int m_LifePoints = 2; // Easy to kill

	private EnnemySight ennemySight;
	private LastPlayerSighting lastPlayerSighting;
	private GameObject player;
	private float chaseTimer;
	private Vector3 nextMovePosition;
	private bool isBlocked; //Means by a collision
	private int radius = 2; // Radius of anticipitation for collisions in patrolling case
	private bool isFrozen = true;

	void Awake () {
		
		player = GameObject.FindGameObjectWithTag("ChildToy");
		ennemySight = GetComponentInChildren<EnnemySight>();
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();

		ChildBehaviour.toyToChild += Freeze;
		ChildBehaviour.childToToy += Unfreeze;
	}

	void Update () {

		if (ennemySight.playerInSight && !isFrozen) {
			Chasing ();
		}

		else if (!ennemySight.playerInSight && !isFrozen ){
			Patrolling ();
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
