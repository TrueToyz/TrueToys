using UnityEngine;
using System.Collections;

public class Toy : MonoBehaviour {

	private 	GameObject 	m_Parachute;
	public 		GameObject 	m_ParachutePrefab;
	public		bool		m_interruptFlag = false;
	public 		bool 		m_isFrozen = false;
	public		bool		m_canBeTaken = false;

	public 	delegate void 	LandingCallback();
	public 	event 			LandingCallback hasLanded;
	public 	event 			LandingCallback hasBeenTaken;
	
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

		// For derived classes
		addSpecificCallbacks();

		// Ground
		Vector3 ToGround = new Vector3(soldier.transform.position.x, targetPos.y, soldier.transform.position.z);
		
		while(soldier != null && Vector3.Distance(soldier.transform.position, ToGround) > GameManager.Instance.distanceBeforeParachute)
		{
			if(m_interruptFlag)
				yield break;

			soldier.transform.position += (ToGround - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime;

			yield return null;
		}

		if(soldier == null)
			yield break;

		// Now the deceleration
		openParachute();

		while(soldier != null && (ToGround - soldier.transform.position).y < 0.01f && Vector3.Distance(ToGround, soldier.transform.position) > 0.01f)
		{

			// Grab again // destroy
			if(m_interruptFlag)
			{
				closeParachute();
				yield break;
			}

			soldier.transform.position += (ToGround - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime / GameManager.Instance.parachuteDampingCoef;
			yield return null;
		}

		if(soldier == null)
		{
			yield break;
		}

		// Correct y 
		soldier.transform.position = ToGround;
		closeParachute();

		m_isFrozen = false;
		if(hasLanded != null)
		{
			hasLanded();
			hasLanded = null;
		}
	}

	public void take()
	{
		if(hasBeenTaken != null)
			hasBeenTaken();

		interrupt();
	}

	public void	interrupt()
	{
		m_interruptFlag = true;
	}

	public void addCallback(LandingCallback cbk)
	{
		this.hasLanded += cbk;
	}

	/*For derived class only */
	public	virtual	void	addSpecificCallbacks() {}

}
