using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ToyUtilities {

	public static bool RayCastToGround (GameObject obj)
	{
		Ray toGround = new Ray(obj.transform.position, Vector3.down);
		return Physics.Raycast(toGround,1000,1 << LayerMask.NameToLayer("Tabletop"));
	}

	public static bool RayCastToGround (GameObject obj, out RaycastHit hit)
	{
		Ray toGround = new Ray(obj.transform.position, Vector3.down);
		return Physics.Raycast(toGround,out hit,1000,1 << LayerMask.NameToLayer("Tabletop"));
	}

	public	static	bool	RayCastToward(Vector3 rayOrigin, Vector3 direction, out RaycastHit hit, int mask)
	{
		Ray myRay = new Ray(rayOrigin, direction);
		Debug.DrawLine (rayOrigin, rayOrigin+direction, Color.red);
		return Physics.Raycast (myRay, out hit, 100f, mask);
	}

	public	static	List<RaycastHit>	BoxRayCastToGround(Collider c, Vector3 rayOrigin, Vector3 direction, int mask)
	{
		// Extents cannot be accessed after diabling collider
		Vector3 extents = c.bounds.extents;
		c.enabled = false;

		// Results
		List<RaycastHit> results = new List<RaycastHit>();

		// Raycast from all extents
		Vector3[] rays = {
			new Vector3(rayOrigin.x,rayOrigin.y,rayOrigin.z),
			new Vector3(rayOrigin.x + extents.x,rayOrigin.y,rayOrigin.z + extents.z),
			new Vector3(rayOrigin.x + extents.x,rayOrigin.y,rayOrigin.z - extents.z),
			new Vector3(rayOrigin.x - extents.x,rayOrigin.y,rayOrigin.z + extents.z),
			new Vector3(rayOrigin.x - extents.x,rayOrigin.y,rayOrigin.z - extents.z)
		};
		
		foreach(Vector3 r in rays)
		{
			RaycastHit hit;
			if(RayCastToward(r,direction,out hit,mask))
			{
				results.Add(hit);
			}
		}
		
		// Else
		c.enabled = true;

		return results;
	}
}
