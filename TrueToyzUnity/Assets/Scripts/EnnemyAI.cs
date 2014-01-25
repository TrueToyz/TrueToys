using UnityEngine;
using System.Collections;

public class EnnemyAI : MonoBehaviour {

	public float chaseSpeed = 0.05f;
	public float chaseWaitTime = 2f;
	public float m_AttackRange = 0.05f;

	private EnnemySight ennemySight;
	private LastPlayerSighting lastPlayerSighting;
	private GameObject player;
	private float chaseTimer;
	private bool isFrozen = true;
	
	void Awake () {

		player = GameObject.FindGameObjectWithTag("ChildToy");
		ennemySight = GetComponent<EnnemySight>();
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();

		ChildBehaviour.toyToChild += Freeze;
		ChildBehaviour.childToToy += Unfreeze;
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

	void Freeze () {
		isFrozen = true;
	}

	void Unfreeze () {
		isFrozen = false;
	}

}
