using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToySight : MonoBehaviour {

	public	List<GameObject> gameObjectsOnSight;

	void OnTriggerEnter (Collider other)
	{
		if(!gameObjectsOnSight.Contains(other.gameObject))
			gameObjectsOnSight.Add(other.gameObject);
	}

	void OnTriggerExit(Collider other)
	{
		if(gameObjectsOnSight.Contains(other.gameObject))
			gameObjectsOnSight.Remove(other.gameObject);
	}
}
