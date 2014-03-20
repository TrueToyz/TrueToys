using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToySpawn : MonoBehaviour {

	public 	bool 			m_canSpawn = false;
	public	bool			m_isFrozen = false;
	public 	GameObject[] 	ml_spawnObjects;

	public 	float 			m_spawnWaitTime = 2f;
	public 	float 			m_spawnRadius = 5f;

	// toys
	public 	int 			m_maxToys = 5;
	public 	GameObject 		m_toyPrefab;

	public	GameObject		m_spawnIndicator;
	private float 			spawnTimer;

	/* Generated enemies */
	private List<GameObject> 	ml_toys;
	public	List<Vector3>		ml_stackSpawns;

	void Start () 
	{
		ml_toys = new List<GameObject>();
		ml_stackSpawns = new List<Vector3>();
		
		// Transition
		ChildBehaviour.childToToy += startSpawning;
		ChildBehaviour.toyToChild += stopSpawning;
	}

	void Update () {

		if(!m_isFrozen)
		{
			spawnTimer += Time.deltaTime;
			if(spawnTimer >= m_spawnWaitTime &&  ml_toys.Count + ml_stackSpawns.Count < m_maxToys)
			{
				// Randomize spawn point
				int index = Random.Range(0,ml_spawnObjects.Length-1);

				Vector3 newPosition = ml_spawnObjects[index].transform.position;
				newPosition.x += Random.Range(-m_spawnRadius,m_spawnRadius);
				newPosition.z += Random.Range(-m_spawnRadius,m_spawnRadius);

				spawnTimer = 0f;

				// Stack this spawn
				ml_stackSpawns.Add(newPosition);

				// Repoint to the first
				m_spawnIndicator.transform.position = ml_stackSpawns[0];
			}

			else if(spawnTimer >= m_spawnWaitTime && ml_stackSpawns.Count > 1) 
			{
				m_spawnIndicator.transform.position = ml_stackSpawns[1];
				ml_stackSpawns.RemoveAt(0);
				spawnTimer = 0f;
			}
		}
	}

	void spawn(Vector3 position)
	{

		// Instantiate them
		GameObject toy = (GameObject)Instantiate(m_toyPrefab, position, Quaternion.identity);
		toy.transform.parent = transform;
		toy.transform.localScale = new Vector3(1f,1f,1f);
	
		ml_toys.Add (toy);

		// Make them fall !
		RaycastHit hit;
		ToyUtilities.RayCastToGround(toy, out hit);
		
		// Specify to which object the toy must be linked with
		Toy toyScript = toy.GetComponent<Toy>() as Toy;
		toyScript.m_owner = gameObject;

		// Launch PlayerToy parachute fall
		// Don't forget to specify the callbacks
		StartCoroutine(toyScript.ParachuteFall(toy,hit.point)); 
		
		spawnTimer = 0f;
	}

	void toyIsDestroyed(GameObject toy)
	{
		if(ml_toys.Contains(toy))
			ml_toys.Remove(toy);
	}

	/* --------------------------------- Function for making them falling ------------------------------ */

	void	canDrop (bool bValue)
	{
		if(!m_isFrozen && ml_stackSpawns.Count > 0)
		{
			if(bValue)
			{
				spawn(ml_stackSpawns[0]);
				ml_stackSpawns.RemoveAt(0);
			}
			else
			{
				spawnTimer = m_spawnWaitTime;
			}
		}
	}

	/* ----------------------------------- Callbacks for when the world swaps ----------------------- */

	void startSpawning ()
	{
		m_isFrozen = false;
	}

	void stopSpawning ()
	{
		m_isFrozen = true;
		ml_stackSpawns.Clear();
	}

	public	void	decrease ()
	{
		m_maxToys--;
	}

	public	void	increase ()
	{
		m_maxToys++;
	}
}
