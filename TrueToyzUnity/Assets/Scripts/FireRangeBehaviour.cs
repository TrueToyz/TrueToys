using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireRangeBehaviour : MonoBehaviour {

	public List<GameObject> ml_EnemiesInRange;

	void Start ()
	{
		ml_EnemiesInRange = new List<GameObject>();
	}

	void OnTriggerEnter (Collider other)
	{
		if(other.gameObject.tag == "Enemy" && !ml_EnemiesInRange.Contains(other.gameObject))
		{
			Debug.Log ("One more");
			ml_EnemiesInRange.Add(other.gameObject);
		}
	}

	void OnTriggerStay (Collider other)
	{
		Debug.Log("Who is staying" + other.gameObject.name);
	}

	void OnTriggerExit (Collider other)
	{
		Debug.Log ("Exit :" + other.gameObject.name);
		if(other.gameObject.tag == "Enemy" && ml_EnemiesInRange.Contains(other.gameObject))
		{
			Debug.Log ("Less one");
			ml_EnemiesInRange.Remove(other.gameObject);
		}
	}

	void DamageAll ()
	{
		foreach (GameObject enemy in ml_EnemiesInRange)
		{
			enemy.SendMessage ("ReceiveDamage");
		}
	}
	
}
