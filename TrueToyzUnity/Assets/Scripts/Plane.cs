using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Plane : TransitionEffect {

	// Plane
	private	GameObject	m_plane;
	private	GameObject	m_blades;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_ActionSounds;
	private AudioSource m_PlaneAudio;

	void Start ()
	{
		// Find plane parts
		m_plane = transform.FindChild("Plane").gameObject;
		m_blades = m_plane.transform.FindChild("blades").gameObject;

		// Fly sound
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
		if(m_isActive)
		{
			transform.Rotate(-Vector3.up * Time.deltaTime * 25);
			m_blades.transform.Rotate(Vector3.up * Time.deltaTime * 100);
		}
	}

	public	override	void activate ()
	{
		base.activate();
		if(m_PlaneAudio)
			m_PlaneAudio.Play();
	}

	public	override	void desactivate ()
	{
		base.desactivate();
		if(m_PlaneAudio)
			m_PlaneAudio.Stop();
	}
}
