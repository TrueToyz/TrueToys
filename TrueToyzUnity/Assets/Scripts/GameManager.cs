using UnityEngine;
using System.Collections;

public class GameManager : Singleton<GameManager> {

	/*
	 * TODO:  what an horrible behavior from this singleton class !
	 * Members from instance does not have the same value than these listed over here
	 * Advice: always use the Instance
	 * But in the future, I shall change that
	 * */

	// MiddleVR runtime generated nodes
	public 	GameObject 	vrRootNode; // Root node for all VR nodes
	public 	GameObject 	vrHandNode; // Node to which the hand is attached
	public 	GameObject 	vrHeadNode; // Head tracking

	// Game elements
	public	GameObject	playerToy;
	public	GameObject	playerAvatar;
	public	GameObject	level;

	public	GameObject	enemySpawn;
	public	GameObject	blockSpawn;

	// Game rules
	public 	float 	swapScale = 5.0f;
	public	float	fallSpeed = 5.0f;
	public	float	parachuteDampingCoef = 2.0f;
	public 	float 	distanceBeforeParachute = 9f;
	public	float	graspRadius = 0.35f;
	public	float	moveScale	=	1.0f;

	// Use this for initialization
	void Start () 
	{
		Instance.distanceBeforeParachute = distanceBeforeParachute;
		Instance.graspRadius = graspRadius;
		Instance.parachuteDampingCoef = parachuteDampingCoef;
		Instance.fallSpeed = fallSpeed;
		Instance.swapScale = swapScale;
		Instance.moveScale = moveScale;

		// Assign what was given on the inspector
		Instance.enemySpawn = enemySpawn;
		Instance.blockSpawn = blockSpawn;
		Instance.level = level;

		if(!level)
			level = GameObject.Find("Level");
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
	
	public void ResetScale ()
	{
		vrRootNode.transform.localScale = Vector3.one;
	}
	
}
