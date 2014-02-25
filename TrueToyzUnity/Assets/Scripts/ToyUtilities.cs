using UnityEngine;
using System.Collections;

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

}
