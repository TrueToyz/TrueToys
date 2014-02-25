using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RotateBlades : MonoBehaviour {

	private bool m_IsActive;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_ActionSounds;
	private AudioSource m_PlaneAudio;

	void Start ()
	{
		ml_ActionSounds = new Dictionary<string, AudioClip>();
		ml_ActionSounds["Fly"] = Resources.Load("Audio/airplane") as AudioClip;
		
		m_PlaneAudio = gameObject.GetComponent<AudioSource>();
		if(!m_PlaneAudio)
		{
			gameObject.AddComponent<AudioSource>();
			m_PlaneAudio = gameObject.GetComponent<AudioSource>();
		}
		
		m_PlaneAudio.clip = ml_ActionSounds["Fly"];
		m_PlaneAudio.loop = true;
		m_PlaneAudio.Stop();
	}
	
	// Update is called once per frame
	void Update () {
		if(m_IsActive)
			transform.Rotate(Vector3.up * Time.deltaTime * 100);
	}

	void activate ()
	{
		m_IsActive = true;
		if(m_PlaneAudio)
			m_PlaneAudio.Play();
	}

	void desactivate ()
	{
		m_IsActive = false;
		if(m_PlaneAudio)
			m_PlaneAudio.Stop();
	}
}
