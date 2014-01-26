using UnityEngine;
using System.Collections;

public class EnemySpawn : MonoBehaviour {

	static bool m_CanSpawn = false;

	public GameObject[] ml_SpawnObjects;
	public float spawnWaitTime = 2f;
	public int maxEnemies = 5;

	public float m_SpawnRadius = 1f;
	public float m_FallSpeed = 4f;
	public float m_DistanceBeforeParachute = 0.1f;

	public GameObject m_EnemyPrefab;
	private float spawnTimer;
	private GameObject[] enemies;

	/*Audio*/
	private GameObject cameraVR;
	private AudioClip fall;

	void Awake () {
		ChildBehaviour.childToToy += StartSpawning;
		ChildBehaviour.toyToChild += StopSpawning;
		fall =  Resources.Load("Audio/fall") as AudioClip;
		
		cameraVR = GameObject.Find("CameraStereo0");
		cameraVR.AddComponent<AudioSource>();
	}

	void Update () {
		//Spawn
		if(m_CanSpawn)
			{
			spawnTimer += Time.deltaTime;
			
			if(spawnTimer >= spawnWaitTime && EnnemyAI.ms_EnemyCount < maxEnemies){
				Debug.Log("New Enemy");
				foreach(GameObject spawnPoint in ml_SpawnObjects)
				{
					// Random position of spawn
					float offset_x = Random.Range(0f,m_SpawnRadius);
					float offset_z = Random.Range(0f,m_SpawnRadius);
					Vector3 spawnPos = spawnPoint.transform.position;
					spawnPos.x += offset_x;
					spawnPos.z += offset_z;

					// Instantiate them
					GameObject enemy = (GameObject)Instantiate(m_EnemyPrefab, spawnPos, Quaternion.identity);
					enemy.transform.parent = transform;
					enemy.transform.localScale = new Vector3(1f,1f,1f);

					// Make them fall !
					Vector3 groundPos = ToyUtilities.RayCastToGround(enemy);
					StartCoroutine(FallSoldier(enemy,groundPos));
				}

				spawnTimer = 0f;
			}
		}
	}

	/* --------------------------------- Function for making them falling ------------------------------ */


	IEnumerator FallSoldier (GameObject soldier, Vector3 targetPos)
	{
		/* This fall can only be made in kinematic*/
		soldier.rigidbody.isKinematic = true;

		// Only keep the Z component
		Vector3 newTarget = new Vector3(soldier.transform.position.x, targetPos.y, soldier.transform.position.z);
		
		while(soldier != null && Vector3.Distance(soldier.transform.position, newTarget) > m_DistanceBeforeParachute)
		{
			if(!m_CanSpawn)
			{
				Destroy(soldier);
				EnnemyAI.ms_EnemyCount --;
				yield break;
				cameraVR.audio.Stop();
				cameraVR.audio.clip = fall;
				cameraVR.audio.Play();

				GameObject enemy = (GameObject)Instantiate(m_EnemyPrefab, spawnPoint.transform.position, Quaternion.identity);
				enemy.transform.parent = transform;
			}

			soldier.transform.position = Vector3.Lerp(soldier.transform.position, newTarget, m_FallSpeed * Time.deltaTime);
			yield return null;
		}
		
		Debug.Log("Parachute !");
		yield return new WaitForSeconds(0.5f);
		Debug.Log("Fall has ended.");
		
		// the toy must returns to normal state
		soldier.rigidbody.isKinematic = false;

		soldier.SendMessage("Unfreeze");
		
	}

	/* ----------------------------------- Callbacks for when the world swaps ----------------------- */

	void StartSpawning ()
	{
		m_CanSpawn = true;
	}

	void StopSpawning ()
	{
		m_CanSpawn = false;
	}
}
