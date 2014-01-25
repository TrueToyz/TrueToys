using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	public int radius = 10;
	public float quitWaitTime = 2f;

	private float quitTimer;
	private GameObject quitButton;

	void Awake () {
		quitButton = GameObject.FindGameObjectWithTag("Quit");
	}

	void Update () {

		RaycastHit hit;

		if (Physics.Raycast(camera.transform.position, transform.forward.normalized, out hit, radius)){
		
			if ( hit.collider.gameObject == quitButton) {

				//Timer
				quitTimer += Time.deltaTime;

				if(quitTimer >= quitWaitTime) {
					//Quit the game
					Application.Quit();
					quitTimer = 0f;
				}


			}

		}
	}
}
