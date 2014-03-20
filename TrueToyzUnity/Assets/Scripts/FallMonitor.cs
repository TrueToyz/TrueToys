using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallMonitor : MonoBehaviour {

	public	ParticleSystem	m_particleFalls;

	// Information about the ground
	public	bool			m_canBeDeployed = true;
	public	bool			m_overGround = true;
	private	GameObject		m_touchedObject;
	private	GameObject		m_obstacle;
	
	public	GameObject		m_target;
	public	GameObject		m_owner;

	public	bool			m_showTemplate;
	public	GameObject		m_templatePrefab;
	public	GameObject		m_templateInstance;


	// Use this for initialization
	void Awake () {
		m_particleFalls = gameObject.GetComponent<ParticleSystem>();

		// We build a new copy of the template used to displace objects in the world
		if(m_templatePrefab)
		{
			m_templateInstance = Instantiate(m_templatePrefab,transform.position,transform.rotation) as GameObject;
			m_templateInstance.transform.parent = transform;

			// Do we show it ?
			if(!m_showTemplate)
			{
				m_templateInstance.GetComponentInChildren<Renderer>().enabled = false;
			}
		}
	}

	void Update ()
	{
		if(m_target)
			raycastColliderOnGround();
	}

	public	void	assignTarget(GameObject newTarget)
	{
		m_target = newTarget;
		m_particleFalls.Play();
	}

	public	void	clearTarget()
	{
		m_target = null;
		m_particleFalls.Stop();
	}

	/* DEPRECATED */
	Vector3	raycastEntireColliderOnGround()
	{
		Vector3 rayOrigin =  new Vector3(m_target.transform.position.x,m_target.transform.position.y + m_templateInstance.collider.bounds.extents.y,m_target.transform.position.z);

		// Send raycast toward the ground 
		List<RaycastHit> results = ToyUtilities.BoxRayCastToGround(m_templateInstance.collider, rayOrigin, Vector3.down, -1);
		
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

	/*
	 * Generates a ray toward the ground from the collider of the template instance
	 * */
	void	raycastColliderOnGround()
	{
		/*
		 * Note: raycasting from the sky can be problematic
		 * */
		Vector3 rayOrigin =  new Vector3(
			m_target.transform.position.x,
			m_target.transform.position.y, // From the hand
			m_target.transform.position.z
			);

		RaycastHit hit;

		// Create a mask that ignores untouchables
		int mask = ~(1 << LayerMask.NameToLayer("Untouchable"));

		// This raycast will momentarily desactivate the collider
		if(ToyUtilities.RayCastToGround(m_templateInstance.collider,rayOrigin,out hit,mask))
		{
			Debug.DrawRay(rayOrigin,Vector3.down,Color.red);

			// Store the object touched for debug
			m_touchedObject = hit.collider.gameObject;

			if(!(hit.collider.gameObject.layer == LayerMask.NameToLayer("Tabletop")))
			{
				m_canBeDeployed = false;
			}
			else
			{
				m_canBeDeployed = true;
			}

			Vector3 groundHit = hit.point;

			// Now launches reflect raycast
			float distance = 1f;
			Vector3 reflectOrigin = new Vector3(
				groundHit.x,
				groundHit.y - 0.05f, // just a little offset to be inside the object
				groundHit.z
				);
			m_touchedObject.collider.enabled = false;
			if(ToyUtilities.RayCastToward(m_templateInstance.collider,reflectOrigin,Vector3.up,out hit,mask,Color.blue,distance))
			{
				m_obstacle = hit.collider.gameObject;
				m_canBeDeployed = false;
			}
			m_touchedObject.collider.enabled = true; 

			// Move particles
			if(m_particleFalls)
			{
				// Set color of particles
				if(m_canBeDeployed)
					m_particleFalls.startColor = Color.green;
				else
					m_particleFalls.startColor = Color.red;
				m_particleFalls.startSpeed = Mathf.Abs(groundHit.y - m_target.transform.position.y);
			}

			// Compute destination based on ryacast hit and clamp
			Vector3 destination = clampCoordinates();
			destination.y = groundHit.y;

			transform.position = destination;

			// Verify interpenetration
			// Note: does not work on everything
			// ToyUtilities.verifyInterpenetration(m_target, destination - m_target.transform.position);

			// Send this to player
			m_owner.SendMessage("canDrop",m_canBeDeployed);
		}
	}
	
	Vector3 clampCoordinates ()
	{
		// Hack
		// Extents vary depending on object rotation !
		Quaternion oldRot = m_target.transform.rotation;
		m_target.transform.rotation = Quaternion.identity;

		// clamp motion in relation to extents
		Vector3 extents = m_templateInstance.GetComponent<Collider>().bounds.max - m_templateInstance.GetComponent<Collider>().bounds.center;

		// Retrieve old rotation
		m_target.transform.rotation = oldRot;

		float newX = Mathf.Round(m_target.transform.position.x / (extents.x*2)) * (extents.x*2);
		float newZ = Mathf.Round(m_target.transform.position.z / (extents.z*2)) * (extents.z*2);

		// Unused, but still it might be useful
		float newY = Mathf.Round(m_target.transform.position.y / (extents.y*2)) * (extents.y*2);

		return new Vector3(newX,newY,newZ);
	}
	
}
