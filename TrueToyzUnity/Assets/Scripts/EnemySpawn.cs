using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour {

	static 	bool 			m_canSpawn = false;

	public 	GameObject[] 	ml_spawnObjects;
	public 	float 			m_spawnWaitTime = 2f;
	public 	int 			m_maxEnemies = 5;

	public 	float 			m_spawnRadius = 1f;

	public 	GameObject 		m_enemyPrefab;
	private float 			spawnTimer;
	private GameObject[] 	ml_enemies;


	void Start () 
	{

		// Find all spawn point
		ml_spawnObjects = GameObject.FindGameObjectsWithTag("SpawnPoint") as GameObject[];

		// Transition
		ChildBehaviour.childToToy += startSpawning;
		ChildBehaviour.toyToChild += stopSpawning;
	}

	void Update () {
		//Spawn
		if(m_canSpawn)
			{
			spawnTimer += Time.deltaTime;
			
			if(spawnTimer >= m_spawnWaitTime && GameManager.Instance.enemyCount < m_maxEnemies){

				GameManager.Instance.enemyCount ++;
				Debug.Log(GameManager.Instance.enemyCount );
				int index = Random.Range(0,ml_spawnObjects.Length-1);

				// Random position of spawn
				Vector3 spawnPos = ml_spawnObjects[index].transform.position;
				spawnPos.x += Random.Range(0f,m_spawnRadius);
				spawnPos.z += Random.Range(0f,m_spawnRadius);

				// Instantiate them
				GameObject enemy = (GameObject)Instantiate(m_enemyPrefab, spawnPos, Quaternion.identity);
				enemy.transform.parent = transform;
				enemy.transform.localScale = new Vector3(1f,1f,1f);

				// Make them fall !
				RaycastHit hit;
				ToyUtilities.RayCastToGround(enemy, out hit);

				// Launch PlayerToy parachute fall
				Toy toyScript = enemy.GetComponent<Toy>() as Toy;

				// Don't forget to specify the callbacks
				StartCoroutine(toyScript.ParachuteFall(enemy,hit.point)); 

				spawnTimer = 0f;
			}
		}
	}

	/* --------------------------------- Function for making them falling ------------------------------ */

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
