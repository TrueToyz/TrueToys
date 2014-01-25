using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	public int radius = 10;

	private GameObject quitButton;
	private GameObject playButton;

	void Awake () {
		quitButton = GameObject.FindGameObjectWithTag("Quit");
		playButton = GameObject.FindGameObjectWithTag("Play");
	}

	void Update () {
		RaycastHit hit;

		if (Physics.Raycast(AvatarManager.vrHeadNode.transform.position, transform.forward, out hit, radius)){

			if ( hit.collider.gameObject == playButton) {

				//Load the main scene
				Debug.Log ("PLAY ! " );
				Application.LoadLevel("Sandbox");
			}
			else if ( hit.collider.gameObject == quitButton) {

				//Quit the game
				Application.Quit();
			}

		}
	}
}
