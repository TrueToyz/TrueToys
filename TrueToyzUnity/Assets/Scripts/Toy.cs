using UnityEngine;
using System.Collections;

public class Toy : MonoBehaviour {

	private 	GameObject 	m_Parachute;
	public 		GameObject 	m_ParachutePrefab;
	public		bool		m_interruptFlag = false;
	public 		bool 		m_isFrozen = false;

	public 	delegate void 	LandingCallback();
	public 	event 			LandingCallback hasLanded;
	
	public void openParachute ()
	{
		if (m_ParachutePrefab)
		{
			m_Parachute = Instantiate(m_ParachutePrefab,Vector3.zero, Quaternion.identity) as GameObject;
			m_Parachute.transform.parent = transform;
			m_Parachute.transform.localPosition = new Vector3(0f,0.05f,0f);
			m_Parachute.transform.localRotation = Quaternion.Euler(-90f,0f,0f);
			m_Parachute.transform.localScale = m_ParachutePrefab.transform.localScale;
		}
	}
	
	public void closeParachute ()
	{
		if (m_Parachute)
			Destroy(m_Parachute);
	}

	public IEnumerator ParachuteFall (GameObject soldier, Vector3 targetPos)
	{
		m_interruptFlag = false;
		m_isFrozen = true;

		// Ground
		Vector3 ToGround = new Vector3(soldier.transform.position.x, targetPos.y, soldier.transform.position.z);
		
		while(soldier != null && Vector3.Distance(soldier.transform.position, ToGround) > GameManager.Instance.distanceBeforeParachute)
		{
			if(m_interruptFlag)
				yield break;

			soldier.transform.position = Vector3.Lerp(soldier.transform.position, ToGround, GameManager.Instance.fallSpeed * Time.deltaTime);
			yield return null;
		}

		if(soldier == null)
			yield break;

		// Now the deceleration
		openParachute();
		
		while(soldier != null && Vector3.Distance(soldier.transform.position, ToGround) > 0.01f)
		{
			if(m_interruptFlag)
			{
				closeParachute();
				yield break;
			}


			soldier.transform.position = Vector3.Lerp(soldier.transform.position, ToGround, GameManager.Instance.fallSpeed * Time.deltaTime / GameManager.Instance.parachuteDampingCoef);
			yield return null;
		}

		if(soldier == null)
			yield break;
		closeParachute();

		m_isFrozen = false;
		if(hasLanded != null)
		{
			hasLanded();
			hasLanded = null;
		}
	}

	public void	interrupt()
	{
		m_interruptFlag = true;
	}

	public void addCallback(LandingCallback cbk)
	{
		this.hasLanded += cbk;
	}


}
