using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour {

	public Vector3 spawnPosition;
	public float spawnWaitTime = 2f;

	private GameObject enemy;
	private float spawnTimer;

	void Awake () {
		enemy = GameObject.FindGameObjectWithTag("Enemy");
	}

	void Update () {
		//Spawn
		spawnTimer += Time.deltaTime;
		
		if(spawnTimer >= spawnWaitTime){
			Debug.Log("New Enemy");
			Instantiate(enemy, spawnPosition, Quaternion.identity);
			spawnTimer = 0f;
		}
	}
}
