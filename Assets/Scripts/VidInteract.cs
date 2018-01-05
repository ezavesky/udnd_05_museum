using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class VidInteract : MonoBehaviour {
	/// <summary>
	/// waypoint for video experience
	/// </summary>
	public GameObject[] objWaypointEnter;
	public GameObject[] objWaypointExit;

	/// <summary>
	/// transform for position resume after experience
	/// </summary>
	protected Vector3 resumePosition;
	/// <summary>
	/// Last hit position
	/// </summary>
	protected Vector3 hitPosition;
	/// <summary>
	/// Centeral game controller
	/// </summary>
	protected GameController gameController;

	/// <summary>
	/// Holds all of the text simulator objects for running process
	/// </summary>
	protected Text[] textSimulator;
	protected bool simulatorActive;
	protected float distMaxGlobal;
	/// <summary>
	/// Collider/raycast object
	/// </summary>
	public GameObject raycastHolder;
	/// <summary>
	/// The simulator dome for intersection
	/// </summary>
	public GameObject objSimDome;
	/// <summary>
	/// Half life exponential decay for distance (e.g.  0.5 = avg half, 0.1 = fast)
	/// </summary>
	public float distHalfLife = 0.5f;
	public static float DECAY_MIN = 0.05f;
	public static float DECAY_MAX = 0.9f;
	/// <summary>
	/// Object for slider to set on start
	/// </summary>
	//public Slider objSlider;

	// Use this for initialization
	void Start () {
		GameObject objTag = GameObject.FindGameObjectsWithTag ("GameController") [0];
		gameController = objTag.GetComponent<GameController>();
		/*
		if (objSlider) {	// update slider if provided
			float valSlider = (distHalfLife - DECAY_MIN) / (DECAY_MAX - DECAY_MIN);
			objSlider.value = valSlider;
		}
		*/

		simulatorActive = false;
		GameObject[] objText = GameObject.FindGameObjectsWithTag ("streamSimulator");
		textSimulator = new Text[objText.Length];
		for (int i=0; i<objText.Length; i++) {
			textSimulator [i] = objText [i].GetComponent<Text> ();
		}

		// now find the global max dist among items
		distMaxGlobal = float.PositiveInfinity;
		for (int i=0; i<textSimulator.Length; i++) {
			float distMaxLocal = 0;
			for (int j=0; i<textSimulator.Length; i++) {
				float distCheck = Vector3.Distance (
					textSimulator [i].transform.position, 
					textSimulator [j].transform.position);
				if (distCheck > distMaxLocal) {	//max dist between points
					distMaxLocal = distCheck;
				}
			}
			if (distMaxLocal < distMaxGlobal) {  //min distance among all points overall
				distMaxGlobal = distMaxLocal;
			}
		}

		UpdateSimulator (Vector3.zero);
	}

	// Update is called once per frame
	void Update () {
		if (simulatorActive) {
			RaycastHit hit;
			Vector3 forwardDir = raycastHolder.transform.TransformDirection (Vector3.forward) * 100;
			//Debug.DrawRay (raycastHolder.transform.position, forwardDir, Color.green);
			if (Physics.Raycast (raycastHolder.transform.position, (forwardDir), out hit)) {
				//Debug.Log (hit.collider.gameObject);
				if (hit.collider.gameObject == objSimDome) {
					UpdateSimulator (hit.point);
				} 
			}
		}
	}

	/// <summary>
	/// Starts the video experience
	/// </summary>
	public void StartExperience() {
		resumePosition = gameController.playerPosition;
		gameController.MovePlayer (objWaypointEnter [objWaypointEnter.Length - 1].transform.position, objWaypointEnter);
		simulatorActive = true;
//		gameController.MovePlayer(
//		public void MovePlayer(Vector3 posNew, Quaternion rotNew, bool dash=true, 
//			float moveDelay=0.0f, Vector3[] posPath=null) {
//
		gameController.SetGameState (GameController.GameState.STATE_ENGAGED);
		gameController.AnalyticsEnter("Vid360");
	}

	/// <summary>
	/// Stop the video experience
	/// </summary>
	public void StopExperience() {
		simulatorActive = false;
		UpdateSimulator (Vector3.zero);
		gameController.SetGameState (GameController.GameState.STATE_NORMAL);
		gameController.MovePlayer (resumePosition, objWaypointExit);
		gameController.AnalyticsExit("Vid360");
	}

	public void UpdateEncodeStrength(float value) {		
		//Debug.Log ("New Compression: " + value);
		//clamp incoming value and interpert for half-life
		distHalfLife = Math.Min(Mathf.Max(DECAY_MIN, 1-value), DECAY_MAX);
		UpdateSimulator (hitPosition);		//call to update
	}
		

	/// <summary>
	/// Update objects in simulator with global message
	/// </summary>
	/// <param name="pointQuery">Reference distance point, null for deactivate.</param>
	protected void UpdateSimulator(Vector3 posTest) {
		if (textSimulator==null) {
			return;
		}			
		float[] distPrecompute = new float[textSimulator.Length];
		float distMin = float.PositiveInfinity;
		float distMax = float.NegativeInfinity;
		hitPosition = posTest;
		for (int i=0; i<textSimulator.Length; i++) {
			if (!simulatorActive) {
				textSimulator [i].text = "Simulator Inactive";
			} else {
				float distHit = Vector3.Distance (textSimulator [i].transform.position, posTest);
				float distExp = (distHit / distMaxGlobal);
				distPrecompute [i] = distExp;
				if (distExp < distMin) {
					distMin = distExp;
				}
				if (distExp > distMax) {
					distMax = distExp;
				}
				//Debug.Log (String.Format("[{0}] Dist: {1}, max {2}", i, distHit, distMaxGlobal));
			}
		}

		if (simulatorActive) {
			float decayTime = distHalfLife / Mathf.Log (2);
			for (int i = 0; i < textSimulator.Length; i++) {
				float distNorm = (distPrecompute [i] - distMin) / (distMax - distMin);
				float distDecay = Mathf.Pow (2.0f, -distNorm / decayTime);
				float percentTotal = Mathf.Round (distDecay * 100.0f);
				textSimulator [i].text = percentTotal + "%";
				//Debug.Log (String.Format ("[{0}] Dist: {1}, norm {2}, decay {5}, min {3}, max {4}", i, distPrecompute [i], distNorm, distMin, distMax, distDecay));
			}
		}
	}


}
