using UnityEngine;
using System.Collections;

public class Toy : MonoBehaviour {

	private 	GameObject 	m_Parachute;
	public 		GameObject 	m_ParachutePrefab;

	public void openParachute ()
	{
		if (m_ParachutePrefab)
		{
			m_Parachute = Instantiate(m_ParachutePrefab,Vector3.zero, Quaternion.identity) as GameObject;
			m_Parachute.transform.parent = transform;
			m_Parachute.transform.localPosition = new Vector3(0f,0.05f,0f);
			m_Parachute.transform.localRotation = Quaternion.Euler(-90f,0f,0f);
			
			Destroy(m_Parachute,2f);
		}
	}
	
	public void closeParachute ()
	{
		if (m_Parachute)
			Destroy(m_Parachute);
	}
	
	

}
