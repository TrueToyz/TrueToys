using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawn : MonoBehaviour {

	static 	bool 			m_canSpawn = false;
	public 	GameObject[] 	ml_spawnObjects;

	public 	float 			m_spawnWaitTime = 2f;
	public 	int 			m_maxEnemies = 5;
	public 	float 			m_spawnRadius = 5f;
	public 	GameObject 		m_enemyPrefab;

	public	GameObject		m_spawnIndicator;
	private float 			spawnTimer;
	private	int				m_enemyCount;

	/* Generated enemies */
	private List<GameObject> 	ml_enemies;

	/* Stack of to-be-born enemies */
	private	List<Vector3>	ml_stackSpawns;

	void Start () 
	{
		ml_enemies = new List<GameObject>();
		ml_stackSpawns = new List<Vector3>();

		// Find all spawn point
		ml_spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint") as GameObject[];

		// Transition
		ChildBehaviour.childToToy += startSpawning;
		ChildBehaviour.toyToChild += stopSpawning;
	}

	void Update () {

		if(m_canSpawn)
		{
			spawnTimer += Time.deltaTime;
			if(spawnTimer >= m_spawnWaitTime && ml_enemies.Count + ml_stackSpawns.Count < m_maxEnemies)
			{
				// Randomize spawn point
				int index = Random.Range(0,ml_spawnObjects.Length-1);

				Vector3 newPosition = ml_spawnObjects[index].transform.position;
				newPosition.x += Random.Range(0f,m_spawnRadius);
				newPosition.z += Random.Range(0f,m_spawnRadius);

				// Stack this spawn
				ml_stackSpawns.Add(newPosition);
			}

			if(ml_stackSpawns.Count > 0) 
				m_spawnIndicator.transform.position = ml_stackSpawns[0];
		}
	}

	void spawn(Vector3 position)
	{

		// Instantiate them
		GameObject enemy = (GameObject)Instantiate(m_enemyPrefab, position, Quaternion.identity);
		enemy.transform.parent = transform;
		enemy.transform.localScale = new Vector3(1f,1f,1f);
	
		ml_enemies.Add (enemy);

		// Make them fall !
		RaycastHit hit;
		ToyUtilities.RayCastToGround(enemy, out hit);
		
		// Launch PlayerToy parachute fall
		Toy toyScript = enemy.GetComponent<Toy>() as Toy;
		
		// Don't forget to specify the callbacks
		StartCoroutine(toyScript.ParachuteFall(enemy,hit.point)); 
		
		spawnTimer = 0f;
	}

	void die(GameObject toy)
	{
		if(ml_enemies.Contains(toy))
			ml_enemies.Remove(toy);
	}

	/* --------------------------------- Function for making them falling ------------------------------ */

	void	canDrop (bool bValue)
	{
		if(ml_stackSpawns.Count > 0)
		{
			if(bValue)
			{
				spawn(ml_stackSpawns[0]);
				ml_stackSpawns.RemoveAt(0);
			}
			else
			{
				ml_stackSpawns.RemoveAt(0);
			}
		}
	}

	/* ----------------------------------- Callbacks for when the world swaps ----------------------- */

	void startSpawning ()
	{
		m_canSpawn = true;
	}

	void stopSpawning ()
	{
		m_canSpawn = false;
	}
}
