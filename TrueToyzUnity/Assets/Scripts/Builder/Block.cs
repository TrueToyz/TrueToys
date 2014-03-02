using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Block : Toy {

	// Numbers of gravity checks to be done
	public	int	m_gravityChecks 	= 0;
	public	int	m_timeImmobile 		= 0;
	public	int	m_timeToBeKinematic = 4;

	// Successive positions to monitor speed
	public	Queue<Vector3>	m_lastPositions = new Queue<Vector3>();

	// Neighbors
	public List<Collider>	m_friendlyColliders = new List<Collider>();

	// Raycast sent toward the ground
	RaycastHit groundHit = new RaycastHit();
	RaycastHit skyHit = new RaycastHit();

	void Start()
	{
		m_canBeTaken = true;
		hasBeenTaken += propagatePhysicalAwakening;
	}

	void	FixedUpdate()
	{
		if(!m_isFrozen)
			verifyGravity();
		m_canBeTaken = !hasObjectOverIt();
	}

	public	override	void	addSpecificCallbacks()
	{
		// addCallback(becomeDynamic);
	}

	// Necessary to know if the block can be taken
	public	bool	hasObjectOverIt()
	{
		// Disable collider temporarily 
		Collider thisCollider = this.GetComponent<Collider>();

		// Extents cannot be accessed after diabling collider
		Vector3 extents = thisCollider.bounds.extents;
		thisCollider.enabled = false;

		// Construct ray
		Vector3 rayOrigin = transform.position;

		// Only tabletop elements or player toy
		int mask_table = 1 << LayerMask.NameToLayer("Tabletop") ;
		int mask_child = 1 << LayerMask.NameToLayer("Untouchable") ;
		int mask = mask_table | mask_child;

		List<RaycastHit> results = ToyUtilities.BoxRayCastToGround(thisCollider, transform.position, Vector3.up, mask);

		if(results.Count > 0)
		{
			return true;
		}

		return false;
	}

	public bool	rayCast(Vector3 rayOrigin, int mask)
	{
		Ray myRay = new Ray(rayOrigin, Vector3.up);
		Debug.DrawLine (rayOrigin, rayOrigin+Vector3.up, Color.green);

		return Physics.Raycast (myRay, out skyHit, 0.3f, mask);

	}

	// Necessary to know if the block will fall or not
	public	bool	hasSupport()
	{
		if (Physics.Raycast (transform.position, Vector3.down, out groundHit, 2, 1 << LayerMask.NameToLayer("Tabletop"))) {
			Debug.DrawLine (transform.position, groundHit.point, Color.cyan);

			// Verify if support is directly under the block
			if(Vector3.Distance(collider.ClosestPointOnBounds(groundHit.point), groundHit.point) < 0.03){
				return true;
				
			}
		}
		return false;

	}

	/*
	 * TODO: verify behavior
	 * */
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

	/*
	 * Check if gravity must be turned on again
	 * */
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


	/* -------------------------------------------------- Collision checks ---------------------------- */

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
}
