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
		{
			//keepOnGround();
			raycastColliderOnGround();
		}

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

	void	raycastColliderOnGround()
	{
		// Send raycasts
		List<RaycastHit> results = ToyUtilities.BoxRayCastToGround(m_target.collider, m_target.transform.position, Vector3.down, -1);

		// Need five raycast to work
		if(results.Count > 4)
		{

			// Compute mean and standard deviation
			float mean = 0;
			foreach(RaycastHit r in results)
				mean += r.point.y;
			mean /= results.Count;

			float std = 0;
			foreach(RaycastHit r in results)
				std += Mathf.Pow(r.point.y - mean,2);
			std = Mathf.Sqrt(std/results.Count);


			Debug.Log (std);

			if(std > 0.001f)
			{
				m_particleFalls.startColor = Color.red;
				m_canBeDeployed = false;
			}
			else
			{
				m_particleFalls.startColor = Color.green;
				m_canBeDeployed = true;
			}

			// Send this to player
			m_owner.SendMessage("canDrop",m_canBeDeployed);

			Vector3 higherHit = results[0].point;
			if(m_particleFalls)
					m_particleFalls.startSpeed = Vector3.Distance(m_target.transform.position,higherHit);
			
			// Position under the hand
			Vector3 destination = new Vector3(m_target.transform.position.x,higherHit.y,m_target.transform.position.z);

			transform.position = destination;

		}

	}

	void 	verify()
	{
		// If not over ground, then no need to verify
		if(m_overGround)
		{
			// Clean null
			ml_obstacles.RemoveAll(item => item == null);

			m_canBeDeployed = true;
			foreach(Collider potentialObstacle in ml_obstacles)
			{
				if(potentialObstacle.gameObject.layer == LayerMask.NameToLayer("Obstacle") ||
				   potentialObstacle.gameObject.layer == LayerMask.NameToLayer("Untouchable") 
				   )
					m_canBeDeployed = false;
				else
				{
					// Verify if there are tabletop elements at the SAME level
					if(Mathf.Abs(potentialObstacle.transform.position.y-transform.position.y) < 0.01f) 
					{
						Debug.Log (potentialObstacle.name);
						m_canBeDeployed = false;
					}
				}
			}
		}
		else
		{
			m_canBeDeployed = false;
		}

		if(m_particleFalls)
		{
			if(m_canBeDeployed)
			{
				m_particleFalls.startColor = Color.green;
			}
			else
			{
				m_particleFalls.startColor = Color.red;
			}
		}

		// Send this to player
		m_owner.SendMessage("canDrop",m_canBeDeployed);
	}

	/*
	void OnTriggerStay (Collider other)
	{
		if(!ml_obstacles.Contains(other))
			ml_obstacles.Add(other);
	}



	void OnTriggerEnter (Collider other)
	{
		if(!ml_obstacles.Contains(other))
			ml_obstacles.Add(other);

		verify();
	}

	void OnTriggerExit (Collider other)
	{
		if(ml_obstacles.Contains(other))
			ml_obstacles.Remove(other);

		verify();
	}
	*/

}
