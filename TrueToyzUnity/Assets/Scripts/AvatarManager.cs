using UnityEngine;
using System.Collections;

public class AvatarManager : MonoBehaviour {

	public static GameObject vrRootNode;

	// Use this for initialization
	void Start () {
		if(!vrRootNode)
			vrRootNode = GameObject.Find ("VRRootNode");
	}
	
	public void MoveRootTo (GameObject avatar)
	{
		vrRootNode.transform.parent = avatar.transform;
	}
}
