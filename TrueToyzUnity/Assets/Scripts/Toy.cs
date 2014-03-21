using UnityEngine;
using System.Collections;

public class Toy : MonoBehaviour {

	// Spawn
	public		GameObject	m_owner;
	private		GameObject	m_support;
	private		GameObject	m_overObject;
	
	public		bool		m_interruptFlag = false;
	public 		bool 		m_isFrozen = false;

	// Lifetime
	public		int			m_lifePoints;

	// Physical behavior
	public	static	float	m_minDistanceSupport = 0.03f;
	private 	GameObject 	m_Parachute;
	private		Vector3		m_groundHit;
	private		Vector3		m_scaledGroundHit; // Debug purpose
	public 		GameObject 	m_ParachutePrefab;
	public		bool		m_canBeTaken = false;
	public		bool		m_canServeAsSupport = false;

	public 	delegate void 	LandingCallback();
	public 	event 			LandingCallback hasLanded;
	public 	event 			LandingCallback hasBeenTaken;

	public virtual	void receiveDamage ()
	{
		m_lifePoints--;
	}
	
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

	/*
	 * We consider ToGround to be already normalized
	 * */
	public IEnumerator ParachuteFall (GameObject soldier, Vector3 ToGround)
	{
		m_interruptFlag = false;
		m_isFrozen = true;

		// For derived classes
		addSpecificCallbacks();

		while(soldier != null && Vector3.Distance(soldier.transform.position,ToyUtilities.ProjectToWorld(ToGround)) > ToyUtilities.ProjectToWorld(GameManager.Instance.distanceBeforeParachute))
		{
			if(m_interruptFlag)
				yield break;

			soldier.transform.position += (ToyUtilities.ProjectToWorld(ToGround) - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime;

			yield return null;
		}

		if(soldier == null)
			yield break;

		// Now the deceleration
		openParachute();


		while(soldier != null && (ToyUtilities.ProjectToWorld(ToGround) - soldier.transform.position).y <  ToyUtilities.ProjectToWorld(m_minDistanceSupport) && Vector3.Distance(ToyUtilities.ProjectToWorld(ToGround), soldier.transform.position) >  ToyUtilities.ProjectToWorld(m_minDistanceSupport))
		{

			// Grab again // destroy
			if(m_interruptFlag)
			{
				closeParachute();
				yield break;
			}

			soldier.transform.position += (ToyUtilities.ProjectToWorld(ToGround) - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime / GameManager.Instance.parachuteDampingCoef;
			yield return null;
		}

		if(soldier == null)
		{
			yield break;
		}

		// Correct y 
		soldier.transform.position = ToyUtilities.ProjectToWorld(ToGround);
		closeParachute();

		if(hasLanded != null)
		{
			hasLanded();
			hasLanded = null;
		}

		m_isFrozen = false;
	}

	public virtual	void take()
	{
		if(hasBeenTaken != null)
			hasBeenTaken();

		// All actual behaviros must be shut down !
		interrupt();
		m_isFrozen = true;

		// Collider is deactivated
		if(collider)
			collider.enabled = false;
		else
			gameObject.GetComponentInChildren<Collider>().enabled = false;

	}

	public	virtual	void	drop()
	{
		// Collider is deactivated
		if(collider)
			collider.enabled = true;
		else
			gameObject.GetComponentInChildren<Collider>().enabled = true;
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

	/* -------------------------------- Physical behavior ---------------------- */
	
	public	virtual	void	Update()
	{
		if(!m_isFrozen)
		{
			verifySupport();
			m_canBeTaken = !hasObjectOverIt();
		}
	}
	
	// We cast a ray touching only tabletop elements and other toy layers (such as untouchable including the player toy)
	public	bool	hasObjectOverIt()
	{
		int mask = (1 << LayerMask.NameToLayer("Character")) | (1 << LayerMask.NameToLayer("Tabletop"))| (1 << LayerMask.NameToLayer("Ennemies"));
		RaycastHit hit;
		Vector3 rayOrigin = transform.position;
		
		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.up,out hit,mask,Color.gray,1f))
		{
			m_overObject = hit.collider.gameObject;
			return true;
		}
		return false;
	}
	
	// We cast a ray from the higher point in the object toward the ground
	public	bool	hasSupport()
	{
		Vector3 rayOrigin =  new Vector3(transform.position.x,transform.position.y + collider.bounds.extents.y, transform.position.z);
		RaycastHit hit;
		int mask = 1 << LayerMask.NameToLayer("Tabletop");
		
		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.down,out hit,mask,Color.green))
		{
			// Supposed support
			m_support = hit.collider.gameObject;
			m_groundHit = ToyUtilities.NormalizeToWorld(hit.point);

			if(Vector3.Distance(collider.ClosestPointOnBounds(hit.point),hit.point) < ToyUtilities.ProjectToWorld(m_minDistanceSupport))
				return true;
			else
				return false;
		}
		else
		{
			Debug.Log ("Weird");
			return false;
		}
	}
	
	public	void	verifySupport()
	{
		if(!this.hasSupport())
		{
			fall ();
		}
	}
	
	public	void	fall()
	{
		// Launch PlayerToy parachute fall
		Toy toyScript = GetComponent<Toy>() as Toy;
		
		// Don't forget to specify the callbacks
		if(m_support == null)
		{
			if(m_owner)
				m_owner.SendMessage("toyIsDestroyed",gameObject);
			Destroy(gameObject);
		}
		else
		{
			StartCoroutine(toyScript.ParachuteFall(gameObject,m_groundHit)); 
		}
	}

}
