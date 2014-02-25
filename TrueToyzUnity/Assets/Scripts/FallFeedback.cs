﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallFeedback : MonoBehaviour {

	public	ParticleSystem	m_particleFalls;
	public	bool			m_canBeDeployed = true;
	public	List<Collider>	ml_obstacles;
	public	GameObject		m_target;
	public	GameObject		m_owner;

	// Use this for initialization
	void Start () {
		m_particleFalls = gameObject.GetComponent<ParticleSystem>();
		ml_obstacles = new List<Collider> ();
	}
	
	// Update is called once per frame
	void Update () {

		// Change speed of particles depending on distance
		RaycastHit hit;
		ToyUtilities.RayCastToGround(m_target,out hit);
		m_particleFalls.startSpeed = Vector3.Distance(m_target.transform.position,hit.point);

		// Position under the hand
		transform.position = new Vector3(m_target.transform.position.x,hit.point.y,m_target.transform.position.z);

	}

	void verify()
	{
		// Clean null
		ml_obstacles.RemoveAll(item => item == null);

		m_canBeDeployed = true;
		foreach(Collider potentialObstacle in ml_obstacles)
		{
			if(potentialObstacle.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
				m_canBeDeployed = false;
		}

		if(m_canBeDeployed)
		{
			m_particleFalls.startColor = Color.green;
		}
		else
		{
			m_particleFalls.startColor = Color.red;
		}

		// Send this to player
		m_owner.SendMessage("canDrop",m_canBeDeployed);
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
}
