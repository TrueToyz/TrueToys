using UnityEngine;
using System.Collections;

public class EnnemySight : MonoBehaviour {

	public float fieldOfViewAngle = 110f ;
	public bool playerInSight;
	public Vector3 personalLastSighting;

	private SphereCollider col;
	private GameObject player;
	private LastPlayerSighting lastPlayerSighting;
	private Vector3 previousSighting;

	void Awake () {
		col = GetComponent<SphereCollider>();
		player = GameObject.FindGameObjectWithTag("ChildToy");
		lastPlayerSighting = GameObject.FindGameObjectWithTag("GameController").GetComponent<LastPlayerSighting>();

		// Set the personal sighting and the previous sighting to the reset position.
		personalLastSighting = lastPlayerSighting.resetPosition;
		previousSighting = lastPlayerSighting.resetPosition;
	}

	void Update () {

		if(lastPlayerSighting.position != previousSighting)
			personalLastSighting = lastPlayerSighting.position;

		previousSighting = lastPlayerSighting.position;
	}

	void OnTriggerStay ( Collider other) {
		// If the player enter the trigger zone
		if (other.gameObject == player ){

			playerInSight = false;

			Vector3 direction = other.transform.position - transform.position;
			float angle = Vector3.Angle(direction, transform.forward);

			//If the angle between forward and where the player is, is less than half the angle of view...
			if (angle <  fieldOfViewAngle / 0.5f ) {

				RaycastHit hit;

				//... and if a raycast toward the player hits something
				if (Physics.Raycast(transform.position, direction.normalized, out hit, col.radius)) {

					if ( hit.collider.gameObject == player ) {

						playerInSight = true;
						lastPlayerSighting.position = player.transform.position;
						//TODO : personnalLastPlayerPos
					}
				}
			}
		}
	}

	void OnTriggerExit (Collider other ) {
		// If the player leaves the trigger zone
		if (other.gameObject == player ) {
			playerInSight = false;
		}
	
	}



}
