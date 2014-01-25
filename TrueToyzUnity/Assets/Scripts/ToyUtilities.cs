using UnityEngine;
using System.Collections;

public class ToyUtilities {

	/*
	 * Raycast toward the ground to know where to place the toy
	 * */
	public static Vector3 RayCastToGround (GameObject obj)
	{
		Ray myRay = new Ray(obj.transform.position, Vector3.down);
		RaycastHit hit;
		
		if(Physics.Raycast(myRay, out hit, 1000))
		{
			if(hit.collider.tag == "Ground")
			{
				return hit.point;
			}
		}
		
		/* Surely a bad idea for a behavior */
		return Vector3.zero;
		
	}
}
