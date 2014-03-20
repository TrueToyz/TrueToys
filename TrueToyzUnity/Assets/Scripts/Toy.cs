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
	 * Only take targetPots.y into account
	 * */
	public IEnumerator ParachuteFall (GameObject soldier, Vector3 targetPos)
	{
		m_interruptFlag = false;
		m_isFrozen = true;

		// For derived classes
		addSpecificCallbacks();

		// Scale ground at 1,1,1
		Vector3 invScale = new Vector3(
			1f / GameManager.Instance.level.transform.localScale.x,
			1f / GameManager.Instance.level.transform.localScale.y,
			1f / GameManager.Instance.level.transform.localScale.z
			);
		Vector3 ToGround = Vector3.Scale (targetPos,invScale);
		Debug.Log (ToGround);
		
		while(soldier != null && Vector3.Distance(soldier.transform.position, Vector3.Scale(ToGround,GameManager.Instance.level.transform.localScale)) > GameManager.Instance.distanceBeforeParachute)
		{
			if(m_interruptFlag)
				yield break;

			soldier.transform.position += (Vector3.Scale (ToGround,GameManager.Instance.level.transform.localScale) - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime;

			yield return null;
		}

		if(soldier == null)
			yield break;

		// Now the deceleration
		openParachute();

		while(soldier != null && (Vector3.Scale (ToGround,GameManager.Instance.level.transform.localScale) - soldier.transform.position).y < 0.01f && Vector3.Distance(Vector3.Scale (ToGround,GameManager.Instance.level.transform.localScale), soldier.transform.position) > 0.01f)
		{

			// Grab again // destroy
			if(m_interruptFlag)
			{
				closeParachute();
				yield break;
			}

			soldier.transform.position += (Vector3.Scale (ToGround,GameManager.Instance.level.transform.localScale) - soldier.transform.position) * GameManager.Instance.fallSpeed * Time.deltaTime / GameManager.Instance.parachuteDampingCoef;
			yield return null;
		}

		if(soldier == null)
		{
			yield break;
		}

		// Correct y 
		soldier.transform.position = Vector3.Scale (ToGround,GameManager.Instance.level.transform.localScale);
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
		
		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.down,out hit,mask,Color.gray))
		{
			m_support = hit.collider.gameObject;
			m_groundHit = hit.point;
			if(Vector3.Distance(collider.ClosestPointOnBounds(hit.point),hit.point) < m_minDistanceSupport)
				return true;
			else
				return false;
		}
		else{
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
