using UnityEngine;
using System.Collections;

public class MenuController : MonoBehaviour {

	public int radius = 10;

	private GameObject quitButton;
	private GameObject playButton;
	private GameObject camera;

	void Awake () {
		camera = GameObject.FindGameObjectWithTag("MainCamera");
		quitButton = GameObject.FindGameObjectWithTag("Quit");
		playButton = GameObject.FindGameObjectWithTag("Play");
	}

	void Update () {

		// Keyboard motion of the toy
		if (Input.GetKey(KeyCode.UpArrow))
			camera.transform.Translate(camera.transform.up * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.DownArrow))
			camera.transform.Translate(-camera.transform.up * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.RightArrow))
			camera.transform.Translate(camera.transform.right * Time.deltaTime * 3.5f);
		else if(Input.GetKey(KeyCode.LeftArrow))
			camera.transform.Translate(-camera.transform.right * Time.deltaTime * 3.5f);

		RaycastHit hit;

		if (Physics.Raycast(camera.transform.position, transform.forward.normalized, out hit, radius)){

			Debug.Log("raycast");

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
