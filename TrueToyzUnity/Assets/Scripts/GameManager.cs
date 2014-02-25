using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager> {

	// MiddleVR runtime generated nodes
	public 	GameObject 	vrRootNode; // Root node for all VR nodes
	public 	GameObject 	vrHandNode; // Node to which the hand is attached
	public 	GameObject 	vrHeadNode; // Head tracking

	// Game elements
	public	GameObject	playerToy;
	public	GameObject	playerAvatar;

	// Runtime elements
	public	int		enemyCount = 0;

	// Game rules
	public 	float 	swapScale = 5.0f;
	public	float	fallSpeed = 5.0f;
	public 	float 	distanceBeforeParachute = 0.1f;
	public	float	graspRadius = 0.35f;
	
	// Use this for initialization
	void Start () 
	{
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

	/*
	 * TODO: is there any other way to do that ?
	 * */
	void Update ()
	{
		enemyCount = Instance.enemyCount;
	}
	
	public void MoveRootTo (GameObject avatar)
	{
		vrRootNode.transform.parent = avatar.transform;
	}

	public void MoveRootTo (GameObject avatar, Vector3 offset)
	{
		vrRootNode.transform.parent = avatar.transform;
		vrRootNode.transform.localPosition = offset;
	}

	public void MoveRootTo (GameObject avatar, Vector3 offset, Quaternion rotation)
	{
		vrRootNode.transform.parent = avatar.transform;
		vrRootNode.transform.localPosition = offset;
		vrRootNode.transform.localRotation = rotation;
	}

	public void AttachNodeToHand (GameObject handChild)
	{
		// Instinctive answer
		handChild.transform.rotation = vrHandNode.transform.rotation;

		// Change hierarchy
		handChild.transform.parent = vrHandNode.transform;
		handChild.transform.localPosition = Vector3.zero;

		// Instinctive answer
		handChild.transform.localRotation = Quaternion.AngleAxis(90,new Vector3(0f,1f,0f));

	}

	public void AttachNodeToHand (GameObject handChild, Vector3 t_offset, Quaternion r_offset)
	{
		handChild.transform.parent = vrHandNode.transform;
		handChild.transform.localPosition = t_offset;
		handChild.transform.localRotation = r_offset;
	}

	public Vector3 GetHeadTrackingOffset ()
	{
		return vrHeadNode.transform.localPosition;
	}

	public void ResetScale ()
	{
		vrRootNode.transform.localScale = Vector3.one;
	}
}
