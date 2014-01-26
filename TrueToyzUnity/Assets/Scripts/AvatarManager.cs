using UnityEngine;
using System.Collections;

public class AvatarManager : MonoBehaviour {

	public static GameObject vrRootNode; // Root node for all VR nodes
	public static GameObject vrHandNode; // Node to which the hand is attached
	public static GameObject vrHeadNode; // Head tracking

	public static float swapScale = 5.0f;
	
	// Use this for initialization
	void Start () {
		if(!vrRootNode)
			vrRootNode = GameObject.Find ("VRRootNode");
		if(!vrHandNode)
			vrHandNode = GameObject.Find ("HandNode");
		if(!vrHeadNode)
			vrHeadNode = GameObject.Find ("HeadTracking");
	}
	
	public static void MoveRootTo (GameObject avatar)
	{
		Debug.Log ("Change Root toward " + avatar.name);
		AvatarManager.vrRootNode.transform.parent = avatar.transform;
	}

	public static void MoveRootTo (GameObject avatar, Vector3 offset)
	{
		Debug.Log ("Change Root toward " + avatar.name);
		AvatarManager.vrRootNode.transform.parent = avatar.transform;
		AvatarManager.vrRootNode.transform.localPosition = offset;
	}

	public static void MoveRootTo (GameObject avatar, Vector3 offset, Quaternion rotation)
	{
		Debug.Log ("Change Root toward " + avatar.name);
		AvatarManager.vrRootNode.transform.parent = avatar.transform;
		AvatarManager.vrRootNode.transform.localPosition = offset;
		AvatarManager.vrRootNode.transform.localRotation = rotation;
	}

	public static void AttachNodeToHand (GameObject handChild)
	{
		// Instinctive answer
		handChild.transform.rotation = vrHandNode.transform.rotation;

		// Change hierarchy
		handChild.transform.parent = vrHandNode.transform;
		handChild.transform.localPosition = Vector3.zero;

		// Instinctive answer
		handChild.transform.localRotation = Quaternion.AngleAxis(90,new Vector3(0f,1f,0f));

	}

	public static void AttachNodeToHand (GameObject handChild, Vector3 t_offset, Quaternion r_offset)
	{
		handChild.transform.parent = vrHandNode.transform;
		handChild.transform.localPosition = t_offset;
		handChild.transform.localRotation = r_offset;
	}

	public static Vector3 GetHeadTrackingOffset ()
	{
		return AvatarManager.vrHeadNode.transform.localPosition;
	}

	public static void ResetScale ()
	{
		vrRootNode.transform.localScale = Vector3.one;
	}
}
