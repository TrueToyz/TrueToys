using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Torchelight : MonoBehaviour {
	
	public GameObject TorchLight;
	public GameObject MainFlame;
	public GameObject BaseFlame;
	public GameObject Etincelles;
	public GameObject Fumee;
	public float MaxLightIntensity;
	public float IntensityLight;

	private bool m_IsActive = false;

	/*Audio*/
	private Dictionary<string,AudioClip> ml_ActionSounds;
	private AudioSource m_TorchAudio;

	void Start () {
		TorchLight.light.intensity=IntensityLight;
		MainFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*20f;
		BaseFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*15f;	
		Etincelles.GetComponent<ParticleSystem>().emissionRate=IntensityLight*7f;
		Fumee.GetComponent<ParticleSystem>().emissionRate=IntensityLight*12f;

		ml_ActionSounds = new Dictionary<string, AudioClip>();
		ml_ActionSounds["Fire"] = Resources.Load("Audio/torch") as AudioClip;
		
		m_TorchAudio = gameObject.GetComponent<AudioSource>();
		if(!m_TorchAudio)
		{
			gameObject.AddComponent<AudioSource>();
			m_TorchAudio = gameObject.GetComponent<AudioSource>();
		}

		m_TorchAudio.clip = ml_ActionSounds["Fire"];
		m_TorchAudio.loop = true;
		m_TorchAudio.Stop();
	}
	

	void Update () {
		if(m_IsActive)
		{
			if (IntensityLight<0) IntensityLight=0;
			if (IntensityLight>MaxLightIntensity) IntensityLight=MaxLightIntensity;		

			TorchLight.light.intensity=IntensityLight/2f+Mathf.Lerp(IntensityLight-0.1f,IntensityLight+0.1f,Mathf.Cos(Time.time*30));

			TorchLight.light.color=new Color(Mathf.Min(IntensityLight/1.5f,1f),Mathf.Min(IntensityLight/2f,1f),0f);
			MainFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*20f;
			BaseFlame.GetComponent<ParticleSystem>().emissionRate=IntensityLight*15f;
			Etincelles.GetComponent<ParticleSystem>().emissionRate=IntensityLight*7f;
			Fumee.GetComponent<ParticleSystem>().emissionRate=IntensityLight*12f;	
		}
	}

	void desactivate ()
	{
		m_IsActive = false;
		if(m_TorchAudio)
			m_TorchAudio.Stop();
		
		MainFlame.GetComponent<ParticleSystem>().Pause();
		BaseFlame.GetComponent<ParticleSystem>().Pause();
		Etincelles.GetComponent<ParticleSystem>().Pause();
		Fumee.GetComponent<ParticleSystem>().Pause();

		MainFlame.SetActive(false);
		BaseFlame.SetActive(false);
		Etincelles.SetActive(false);
		Fumee.SetActive(false);
	}

	void activate () {
		m_IsActive = true;
		if(m_TorchAudio)
			m_TorchAudio.Play();

		MainFlame.GetComponent<ParticleSystem>().Play();
		BaseFlame.GetComponent<ParticleSystem>().Play();
		Etincelles.GetComponent<ParticleSystem>().Play();
		Fumee.GetComponent<ParticleSystem>().Play();

		MainFlame.SetActive(true);
		BaseFlame.SetActive(true);
		Etincelles.SetActive(true);
		Fumee.SetActive(true);
	}
}
