using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallFeedback : MonoBehaviour {

	public	ParticleSystem	m_particleFalls;
	public	bool			m_canBeDeployed = true;
	public	bool			m_overGround = true;
	public	List<Collider>	ml_obstacles;
	public	GameObject		m_target;
	public	GameObject		m_owner;


	// Use this for initialization
	void Start () {
		m_particleFalls = gameObject.GetComponent<ParticleSystem>();
		ml_obstacles = new List<Collider> ();
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if(m_target)
			raycastColliderOnGround();
	}

	public	void	assignTarget(GameObject newTarget)
	{
		m_target = newTarget;
		m_particleFalls.Play();
		clear ();
	}

	public	void	clearTarget()
	{
		m_target = null;
		m_particleFalls.Stop();
		clear ();
	}

	void clear()
	{
		ml_obstacles.Clear();
	}

	void	keepOnGround ()
	{
		// Change speed of particles depending on distance
		RaycastHit hit;
		m_overGround = ToyUtilities.RayCastToGround(m_target,out hit);
		if(m_particleFalls)
			m_particleFalls.startSpeed = Vector3.Distance(m_target.transform.position,hit.point);
		
		// Position under the hand
		Vector3 destination = new Vector3(m_target.transform.position.x,hit.point.y,m_target.transform.position.z);
		
		// Move in a physical manner
		rigidbody.MovePosition(destination);
	}

	Vector3	raycastEntireColliderOnGRound()
	{
		Vector3 rayOrigin =  new Vector3(m_target.transform.position.x,m_target.transform.position.y + m_target.collider.bounds.extents.y,m_target.transform.position.z);

		// Send raycast toward the ground 
		List<RaycastHit> results = ToyUtilities.BoxRayCastToGround(m_target.collider, rayOrigin, Vector3.down, -1);
		
		// Init
		m_canBeDeployed = true;
		
		// Verify that all objects collided are tabletops
		foreach(RaycastHit r in results)
		{
			if(!(r.collider.gameObject.layer == LayerMask.NameToLayer("Tabletop")))
			{
				m_canBeDeployed = false;
			}
		}
		
		Vector3 higherHit = results[0].point;
		foreach(RaycastHit r in results)
		{
			if(r.point.y > higherHit.y)
				higherHit = r.point;
		}

		return higherHit;

	}

	void	raycastColliderOnGround()
	{
		/*
		 * Note: raycasting from the sky can be problematic
		 * */
		Vector3 rayOrigin =  new Vector3(
			m_target.transform.position.x,
			m_target.transform.position.y+2f,
			m_target.transform.position.z
			);

		RaycastHit hit;
		ToyUtilities.RayCastToGround(m_target.collider,rayOrigin,out hit,-1);

		if(!(hit.collider.gameObject.layer == LayerMask.NameToLayer("Tabletop")))
		{
			m_canBeDeployed = false;
		}
		else
		{
			m_canBeDeployed = true;
		}

		Vector3 higherHit = hit.point;

		// Move particles
		if(m_particleFalls)
		{
			// Set color of particles
			if(m_canBeDeployed)
				m_particleFalls.startColor = Color.green;
			else
				m_particleFalls.startColor = Color.red;
			m_particleFalls.startSpeed = Mathf.Abs(higherHit.y - m_target.transform.position.y);
		}

		// Compute destination based on ryacast hit and clamp
		Vector3 destination = clampCoordinates();
		destination.y = higherHit.y;

		transform.position = destination;

		// Verify interpenetration
		// Note: does not work on everything
		// ToyUtilities.verifyInterpenetration(m_target, destination - m_target.transform.position);

		// Send this to player
		m_owner.SendMessage("canDrop",m_canBeDeployed);


	}
	
	Vector3 clampCoordinates ()
	{
		// Hack
		// Extents vary depending on object rotation !
		Quaternion oldRot = m_target.transform.rotation;
		m_target.transform.rotation = Quaternion.identity;

		// clamp motion in relation to extents
		Vector3 extents = m_target.GetComponent<Collider>().bounds.max - m_target.GetComponent<Collider>().bounds.center;

		// Retrieve old rotation
		m_target.transform.rotation = oldRot;

		float newX = Mathf.Round(m_target.transform.position.x / (extents.x*2)) * (extents.x*2);
		float newZ = Mathf.Round(m_target.transform.position.z / (extents.z*2)) * (extents.z*2);

		// Unused, but still it might be useful
		float newY = Mathf.Round(m_target.transform.position.y / (extents.y*2)) * (extents.y*2);

		return new Vector3(newX,newY,newZ);
	}
	
}
