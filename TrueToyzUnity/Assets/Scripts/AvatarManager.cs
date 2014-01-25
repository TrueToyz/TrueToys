using UnityEngine;
using System.Collections;

public class AvatarManager : MonoBehaviour {

	public static GameObject vrRootNode; // Root node for all VR nodes
	public static GameObject vrHandNode; // Node to which the hand is attached

	// Use this for initialization
	void Start () {
		if(!vrRootNode)
			vrRootNode = GameObject.Find ("VRRootNode");
		if(!vrHandNode)
			vrHandNode = GameObject.Find ("HandNode");
	}
	
	public static void MoveRootTo (GameObject avatar)
	{
		AvatarManager.vrRootNode.transform.parent = avatar.transform;
	}

	public static void AttachNodeToHand (GameObject hand)
	{
		hand.transform.parent = vrHandNode.transform;
	}
}
