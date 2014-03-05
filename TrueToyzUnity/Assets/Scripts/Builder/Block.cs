using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : Toy {

	public	static	float	m_minDistanceSupport = 0.03f;

	// Numbers of gravity checks to be done
	public	int	m_gravityChecks 	= 0;
	public	int	m_timeImmobile 		= 0;
	public	int	m_timeToBeKinematic = 4;

	// Successive positions to monitor speed
	public	Queue<Vector3>	m_lastPositions = new Queue<Vector3>();

	// Neighbors
	public List<Collider>	m_friendlyColliders = new List<Collider>();

	// Raycast sent toward the ground
	private	Vector3	m_support;	

	void Start()
	{
		m_canBeTaken = true;
		//hasBeenTaken += propagatePhysicalAwakening;
	}

	void	FixedUpdate()
	{
		if(!m_isFrozen)
			verifySupport();

		// Verify if object can be taken
		m_canBeTaken = !hasObjectOverIt();
	}

	public	override	void	addSpecificCallbacks()
	{
		// addCallback(becomeDynamic);
	}

	// Necessary to know if the block can be taken
	public	bool	hasObjectOverIt()
	{
		//We cast a ray touching only tabletop elements and other toy layers (such as untouchable including the player toy)
		int mask = (1 << LayerMask.NameToLayer("Untouchable")) | (1 << LayerMask.NameToLayer("Tabletop"));
		RaycastHit hit;
		Vector3 rayOrigin = transform.position;

		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.up,out hit,mask,Color.gray))
		{
			return true;
		}
		return false;
	}

	public	bool	hasSupport()
	{
		// We cast a ray from the higher point in the object toward the ground
		Vector3 rayOrigin =  new Vector3(transform.position.x,transform.position.y + collider.bounds.extents.y, transform.position.z);
		RaycastHit hit;
		int mask = 1 << LayerMask.NameToLayer("Tabletop");

		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.down,out hit,mask,Color.gray))
		{
			m_support = hit.point;
			if(Vector3.Distance(collider.ClosestPointOnBounds(hit.point),hit.point) < m_minDistanceSupport)
				return true;
			else
				return false;
		}
		else{
			return false;
		}
	}

	/*
	public	bool	isMoving()
	{
		if(!rigidbody.isKinematic)
		{
			m_lastPositions.Enqueue(transform.position);
			if(m_lastPositions.Count > 10)
			{
				Vector3 motion = m_lastPositions.Dequeue() - transform.position;
				if(motion.magnitude < 0.03f)
					return false;
			}
			return rigidbody.velocity.magnitude > 0.001f;
		}

		return false;
	}
	*/


	// Manual contro lof fall
	public	void	verifySupport()
	{
		if(!this.hasSupport())
		{
			fall ();
		}
	}

	/*
	 * Check if gravity must be turned on again
	 * */
	/*
	public	void	verifyGravity ()
	{
		// Can be awakened
		if(rigidbody.isKinematic)
		{
			if(m_gravityChecks > 0)
			{
				m_gravityChecks--;

				// Verify support
				if(!this.hasSupport())
				{
					m_gravityChecks = 0;

					// Reestablish gravity
					becomeDynamic();

					// Propagate the physicalization
					propagatePhysicalAwakening();
				}
			}
		}
		// Physical behavior
		else
		{
			if(!this.isMoving())
			{
				m_timeImmobile++;
				if(m_timeImmobile == m_timeToBeKinematic)
				{
					// Reset attributes
					m_timeImmobile = 0;
					m_lastPositions.Clear();
					m_gravityChecks = 0;

					// Object will stay immobile a long time !
					rigidbody.isKinematic = true;
				}
			}
			else
			{
				m_timeImmobile = 0;
				foreach(Collider c in m_friendlyColliders)
				{
					if(!c.Equals(null))
					{
						c.SendMessage("checkForGravity",SendMessageOptions.DontRequireReceiver);
					}
				}
				m_friendlyColliders.Clear();
			}

		}

	}
	*/

	public	void	fall()
	{
		// Launch PlayerToy parachute fall
		Toy toyScript = GetComponent<Toy>() as Toy;
		
		// Don't forget to specify the callbacks
		StartCoroutine(toyScript.ParachuteFall(gameObject,m_support)); 
	}

	/*
	public	void	propagatePhysicalAwakening()
	{
		Collider[] neighbors = Physics.OverlapSphere(transform.position, 0.3f, 1 << LayerMask.NameToLayer("Tabletop"));
		foreach(Collider c in neighbors)
		{
			if(!c.Equals(collider))
			{
				Debug.Log(c.name);
				c.SendMessage("checkForGravity",SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public	void	becomeDynamic ()
	{
		rigidbody.isKinematic = false;
		rigidbody.WakeUp();
	}

	public	void	checkForGravity()
	{
		m_gravityChecks = 50;
	}

	public	void	makeKinematic ()
	{
		rigidbody.isKinematic = true;
		m_isFrozen = true;
	}
	*/


	/* -------------------------------------------------- Collision checks ---------------------------- */

	/*
	public	void	OnCollisionEnter(Collision c)
	{
		if(c.gameObject.layer == gameObject.layer)
			m_friendlyColliders.Add(c.collider);

		// Too strong a collision can move the others
		if(rigidbody.isKinematic && c.relativeVelocity.sqrMagnitude > 20)
		{
			becomeDynamic();
			rigidbody.velocity = c.relativeVelocity / 2;
		}

	}

	public	void	OnCollisionExit(Collision c)
	{
		if(m_friendlyColliders.Contains(c.collider))
			m_friendlyColliders.Remove(c.collider);
	}
	*/
}
