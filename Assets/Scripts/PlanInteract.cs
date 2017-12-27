using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class PlanInteract : MonoBehaviour {
	/// <summary>
	/// transform for position resume after experience
	/// </summary>
	protected Vector3 resumePosition;
	/// <summary>
	/// Centeral game controller
	/// </summary>
	protected GameController gameController;

	/// <summary>
	/// waypoint for eneter and exit experience
	/// </summary>
	public GameObject[] objWaypointEnter;
	public GameObject[] objWaypointExit;

	/// <summary>
	/// Container of homes to have virtual network traffic for
	/// </summary>
	public string tagHomes;

	public int numGameActive = 2;
	public int numGameTotal = 8;
	protected const float VAL_BAD = -1.5f;
	protected const float VAL_GOOD = 1.5f;

	/// <summary>
	/// Prefab storage for network example...
	/// </summary>
	public GameObject objPrefab;

	public Text textCountdown;
	protected bool gameRunning;
	protected float timeStart;

	protected List<PlanNode> listGame;
	protected Gradient gradientGame;

	void Start()
	{
		GameObject objTag = GameObject.FindGameObjectsWithTag ("GameController") [0];
		gameController = objTag.GetComponent<GameController>();
		gameRunning = false;

		gradientGame = new Gradient();		//create gradient [-1.5,-0.5]=red, [-0.5,0.5]=white, [0.5,1.5]=blue
		GradientColorKey[] gck;
		GradientAlphaKey[] gak;
		gck = new GradientColorKey[3];
		gck [0].color = new Color (	//red
			byte.Parse ("E5", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("00", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("0F", System.Globalization.NumberStyles.HexNumber)/255.0f);
		gck[0].time = 0f;
		gck [1].color = new Color (
			byte.Parse ("FF", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("FF", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("FF", System.Globalization.NumberStyles.HexNumber)/255.0f);
		gck[1].time = 0.5f;
		gck [2].color = new Color (	//blue
			byte.Parse ("00", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("BF", System.Globalization.NumberStyles.HexNumber)/255.0f,
			byte.Parse ("B2", System.Globalization.NumberStyles.HexNumber)/255.0f);
		gck[2].time = 1f;
		gak = new GradientAlphaKey[2];
		Debug.Log (gck[0].color);
		Debug.Log (gck[1].color);
		Debug.Log (gck[2].color);
		gak[0].alpha = 1.0F;
		gak[0].time = 0f;
		gak[1].alpha = 1.0F;
		gak[1].time = 1;
		gradientGame.SetKeys(gck, gak);
		listGame = new List<PlanNode> ();
	}

	// Update is called once per frame
	void Update () {
		if (gameRunning) {
			float timeDelta = Time.fixedTime - timeStart;
			textCountdown.text = String.Format("Time {0:00.}:{1:00.}",timeDelta/60, timeDelta%60);
		}
	}

	/// <summary>
	/// Starts the planing experience
	/// </summary>
	public void StartExperience() {
		resumePosition = gameController.playerPosition;
		gameController.MovePlayer (objWaypointEnter [objWaypointEnter.Length - 1].transform.position, objWaypointEnter);
		timeStart = Time.fixedTime;
		gameRunning = true;
		gameController.SetGameState (GameController.GameState.STATE_ENGAGED);
		RestartExperience ();
	}

	/// <summary>
	/// Stop the experience
	/// </summary>
	public void StopExperience() {
		gameRunning = false;
		gameController.SetGameState (GameController.GameState.STATE_NORMAL);
		gameController.MovePlayer (resumePosition, objWaypointExit);
	}

	/// <summary>
	/// Stop the experience
	/// </summary>
	public void RestartExperience() {
		//delete items from game
		foreach  (PlanNode item in listGame) {
			Destroy (item.gameObject);
		}
		listGame.Clear ();

		//need to repopulate it?
		timeStart = Time.fixedTime;
		gameRunning = true;
	
		//create required objects
		//turn on or off different objects
		//allow interaction of devices to turn on or off network impact

		GameObject[] objHome = GameObject.FindGameObjectsWithTag (tagHomes);
		List<int> listIdx = Enumerable.Range(0, objHome.Length).ToList();
		int numGood = numGameActive;
		int numBad = numGameActive;
		for (int i = 0; i < numGameTotal; i++) {
			int idxRand = (int)Math.Round((double)UnityEngine.Random.Range (0, listIdx.Count-1));
			GameObject objNew = Instantiate<GameObject> (objPrefab);
			Transform transParent = objHome[listIdx[idxRand]].transform;
			objNew.transform.SetParent (transParent, false);
			PlanNode nodeNew = objNew.GetComponent<PlanNode> ();
			nodeNew.force = 0.0f;
			nodeNew.load = 0.0f;
			float randState = UnityEngine.Random.Range (0f,1f);
			if (numGood > 0) {
				nodeNew.load = nodeNew.force = randState + (VAL_GOOD-1);
				numGood--;
				nodeNew.activeLight = true;
			} else if (numBad > 0) {
				numBad--;
				nodeNew.load = nodeNew.force = randState + VAL_BAD;
				nodeNew.activeLight = true;
			} else {
				randState = 0.0f;
			}

			//add event trigger data to the new ndoe
			nodeNew.interactTarget = this.gameObject;

			//prepare node with lighting
			Debug.Log ("Created new Game NODE: "+listIdx[idxRand]+", parent:"+idxRand+", force:"+nodeNew.force);
			RelightNode (nodeNew);
			listGame.Add (nodeNew);
			listIdx.RemoveAt (idxRand);
		}

		//TODO: other items...

	}

	/// <summary>
	/// Method to relight a specific node and optionally propagate to others
	/// </summary>
	/// <param name="updateOthers">If set to <c>true</c> update others.</param>
	protected void RelightNode(PlanNode node, bool updateOthers=false) {
		Debug.Log ("Relighting NODE: force:"+node.force+", value:"+node.load+", state:"+node.activeLight+", parent:"+node.gameObject.transform.parent.gameObject.name);

		//fetch the light object of our game object and set its color based on node value
		Light lightNode = node.interactLight;
		lightNode.color = gradientGame.Evaluate ((node.load - VAL_BAD)/(VAL_GOOD - VAL_BAD));
		lightNode.enabled = node.activeLight;
		Debug.Log (lightNode.color);
		if (updateOthers) {
			Debug.Log ("PlanInteract::RelightNode: Sorry, this functionality is not complete yet...");
		}
	}

	/// <summary>
	/// Method to toggle a single node (usually by user click)
	/// </summary>
	/// <param name="obj">Object.</param>
	public void ToggleNode(GameObject obj) {
		PlanNode foundNode = listGame.FirstOrDefault (p => p.gameObject == obj);
		if (foundNode == null) {
			return;
		}
		if (foundNode.force != 0.0f && foundNode.activeLight) {
			//error, can't turn off a force node
			Debug.Log("Can't turn on or off a force node!");
			return;
		}
		foundNode.activeLight = !foundNode.activeLight;	//otherwise, toggle node state
		RelightNode (foundNode, true);
	}

}

