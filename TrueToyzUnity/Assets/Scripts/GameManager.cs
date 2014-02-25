using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	// MiddleVR runtime generated nodes
	public 	static 	GameObject 	vrRootNode; // Root node for all VR nodes
	public 	static 	GameObject 	vrHandNode; // Node to which the hand is attached
	public 	static 	GameObject 	vrHeadNode; // Head tracking

	// Game elements
	public	static	GameObject	playerToy;
	public	static	GameObject	playerAvatar;

	// Runtime elements
	public	static	int		enemyCount;

	// Game rules
	public 	static 	float 	swapScale = 5.0f;
	public	static	float	fallSpeed = 5.0f;
	public 	static	float 	distanceBeforeParachute = 0.1f;
	public	static	float	graspRadius = 0.35f;
	
	// Use this for initialization
	void Start () 
	{
		enemyCount = 0;
		if(!playerAvatar)
			playerAvatar = GameObject.Find("Child");
		if(!playerToy)
			playerToy = GameObject.FindGameObjectWithTag("ChildToy");
		if(!vrRootNode)
			vrRootNode = GameObject.Find ("VRRootNode");
		if(!vrHandNode)
			vrHandNode = GameObject.Find ("HandNode");
		if(!vrHeadNode)
			vrHeadNode = GameObject.Find ("HeadTracking");
	}
	
	public static void MoveRootTo (GameObject avatar)
	{
		GameManager.vrRootNode.transform.parent = avatar.transform;
	}

	public static void MoveRootTo (GameObject avatar, Vector3 offset)
	{
		GameManager.vrRootNode.transform.parent = avatar.transform;
		GameManager.vrRootNode.transform.localPosition = offset;
	}

	public static void MoveRootTo (GameObject avatar, Vector3 offset, Quaternion rotation)
	{
		GameManager.vrRootNode.transform.parent = avatar.transform;
		GameManager.vrRootNode.transform.localPosition = offset;
		GameManager.vrRootNode.transform.localRotation = rotation;
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
		return GameManager.vrHeadNode.transform.localPosition;
	}

	public static void ResetScale ()
	{
		vrRootNode.transform.localScale = Vector3.one;
	}
}
