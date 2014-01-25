using UnityEngine;
using System.Collections;

public class EnnemySight : MonoBehaviour {

	public float fieldOfViewAngle = 180f ;
	public bool playerInSight;
	public Vector3 personalLastSighting;

	private SphereCollider col;
	private GameObject player;
	private LastPlayerSighting lastPlayerSighting;
	private Vector3 previousSighting;

	void Awake () {
		col = GetComponent<SphereCollider>();
		if(!col)
		{
			gameObject.AddComponent<SphereCollider>();
			col = GetComponent<SphereCollider>();
			col.radius = 0.5f;
			col.isTrigger = true;
		}

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


	void OnTriggerEnter (Collider other) {
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

	float CalculatePathLength (Vector3 targetPosition)
	{
		// Create a path and set it based on a target position.
		NavMeshPath path = new NavMeshPath();
		if(nav.enabled)
			nav.CalculatePath(targetPosition, path);
		
		// Create an array of points which is the length of the number of corners in the path + 2.
		Vector3[] allWayPoints = new Vector3[path.corners.Length + 2];
		
		// The first point is the enemy's position.
		allWayPoints[0] = transform.position;
		
		// The last point is the target position.
		allWayPoints[allWayPoints.Length - 1] = targetPosition;
		
		// The points inbetween are the corners of the path.
		for(int i = 0; i < path.corners.Length; i++)
		{
			allWayPoints[i + 1] = path.corners[i];
		}
		
		// Create a float to store the path length that is by default 0.
		float pathLength = 0;
		
		// Increment the path length by an amount equal to the distance between each waypoint and the next.
		for(int i = 0; i < allWayPoints.Length - 1; i++)
		{
			pathLength += Vector3.Distance(allWayPoints[i], allWayPoints[i + 1]);
		}
		
		return pathLength;
	}

}
