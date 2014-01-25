using UnityEngine;
using System.Collections;

public class SniperBehaviour : MonoBehaviour {

	private EnnemySight ennemySight;
	private LastPlayerSighting lastPlayerSighting;
	private GameObject player;

	void Awake () {
		
		player = GameObject.FindGameObjectWithTag("ChildHand");
		ennemySight = GetComponent<EnnemySight>();
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();
	}

	void Update () {
		if (ennemySight.playerInSight) {
			Shooting ();
		}
	}

	void Shooting () {

		Vector3 sightingDeltaPos = ennemySight.personalLastSighting - transform.position;
		Debug.Log("plop");
		// If the the last personal sighting of the player is not close...
		if(sightingDeltaPos.sqrMagnitude > 4f) {
			Debug.Log("SHOOT !");

		}
	}

}
