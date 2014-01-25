using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour {

	public Vector3 spawnPosition;
	public float spawnWaitTime = 2f;
	public int maxEnemies = 10;

	private GameObject enemy;
	private float spawnTimer;
	private GameObject[] enemies;

	void Awake () {
		enemy = GameObject.FindGameObjectWithTag("Enemy");
	}

	void Update () {
		//Spawn
		enemies = GameObject.FindGameObjectsWithTag("Enemy");

		spawnTimer += Time.deltaTime;
		
		if(spawnTimer >= spawnWaitTime && enemies.Length < 10){
			Debug.Log("New Enemy");
			Instantiate(enemy, spawnPosition, Quaternion.identity);
			spawnTimer = 0f;
		}
	}
}
