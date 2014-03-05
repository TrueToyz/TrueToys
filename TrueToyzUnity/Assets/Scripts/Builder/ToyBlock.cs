using UnityEngine;
using System.Collections;

public class ToyBlock : Toy {

	public	static	float	m_minDistanceSupport = 0.03f;
	private			Vector3	m_support;	

	void Start()
	{
		m_canBeTaken = true;
	}

	void	FixedUpdate()
	{
		if(!m_isFrozen)
			verifySupport();
		m_canBeTaken = !hasObjectOverIt();
	}

	// We cast a ray touching only tabletop elements and other toy layers (such as untouchable including the player toy)
	public	bool	hasObjectOverIt()
	{
		int mask = (1 << LayerMask.NameToLayer("Untouchable")) | (1 << LayerMask.NameToLayer("Tabletop"));
		RaycastHit hit;
		Vector3 rayOrigin = transform.position;
		
		if(ToyUtilities.RayCastToward(collider,rayOrigin,Vector3.up,out hit,mask,Color.gray))
		{
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
			m_support = hit.point;
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
		StartCoroutine(toyScript.ParachuteFall(gameObject,m_support)); 
	}

	public override	void take()
	{
		base.take();
		m_isFrozen = true;
	}

}
