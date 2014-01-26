using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour {

	public GameObject[] ml_SpawnObjects;
	public float spawnWaitTime = 2f;
	public int maxEnemies = 5;

	public GameObject m_EnemyPrefab;
	private float spawnTimer;
	private GameObject[] enemies;

	void Awake () {
	}

	void Update () {
		//Spawn
		enemies = GameObject.FindGameObjectsWithTag("Enemy");

		spawnTimer += Time.deltaTime;
		
		if(spawnTimer >= spawnWaitTime && enemies.Length < maxEnemies){
			Debug.Log("New Enemy");
			foreach(GameObject spawnPoint in ml_SpawnObjects)
			{
				GameObject enemy = (GameObject)Instantiate(m_EnemyPrefab, spawnPoint.transform.position, Quaternion.identity);
				enemy.transform.parent = transform;
			}

			spawnTimer = 0f;
		}
	}
}
