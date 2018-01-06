using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;  // Reference the Unity Analytics namespace

public class GameController : MonoBehaviour {
	public GameObject objDoor;
	public GameObject[] positionDoor = new GameObject[2];

	//public HudInteraction objHud;

	[Serializable]
	public class Waypoint {
		public GameObject obj;
		public string name;
	}

	public float moveSpeed = 4f;
	public float moveHeight = 1.5f;
	public GameObject objPlayer;
	public Waypoint[] waypoints;

	/// <summary>
	/// Capability to set navpoint from an external class
	/// </summary>
	protected Vector3 _navpoint;
	public Vector3 navpoint { 
		set {
			_navpoint = value;
		}
	}

	public Vector3 playerPosition { 
		get {
			return objPlayer.transform.position;
		}
	}

	public bool IsLocked {
		get {
			return (gameState==GameState.STATE_ENGAGED);
		}
	}

	public enum GameState { STATE_INVALID, STATE_STARTUP, STATE_ENGAGED, STATE_NORMAL }
	private GameState gameState = GameState.STATE_INVALID;
	//private GameState gameAutoProgress = GameState.STATE_INVALID;

	private float timeAutoAction = -1f;
	private GameObject autoDisable = null;
	private GameObject autoEnable = null;

	public GameObject FindWaypoint(string nameQuery) {
		foreach (Waypoint w in waypoints) {
			if (w.name == nameQuery)
				return w.obj;
		}
		return null;
	}

	// Use this for initialization
	protected void Start () {
		//disable all the waypoints because they contain other objects
		//FindWaypoint["start"].SetActive(false);
		dictTimeTracker = new Dictionary<string, float> ();

		//force start at the beginning
		navpoint = objPlayer.transform.position;
		/*iTween.CameraFadeFrom(iTween.Hash
			"amount", 0, "time", 2.0f
		);*/

		//configure sound
		soundMove = objPlayer.GetComponent<GvrAudioSource>();
	}
	
	/// --------------- walking/movement sound emulation  ----------------------
	private GvrAudioSource soundMove;
	private float soundStop = -1.0f;
	public void WalkStart(float fDuration, float fRate=1.0f) {
		if (soundMove) {
			if (soundStop < 0.0f) {		//only if not going, restart
				soundMove.Play();
			}
			soundStop = Time.fixedTime + fDuration;
		}
	}
	public void WalkStop() {
		if (soundMove) {
			soundMove.Pause();
		}
		soundStop = -1.0f;
	}


	/// --------------- methods for analytics tracking --------------------
	//method for tracking entry/egress for areas and actions
	protected Dictionary<string, float> dictTimeTracker;
	public GameObject raycastIndicator;

	public void AnalyticsTrigger(string strEvent) {
		//called when game mechanics don't know if it's enter or exit (e.g. a wall trigger)
		if (dictTimeTracker.ContainsKey(strEvent)) {
			AnalyticsExit(strEvent);
			return;
		}
		AnalyticsEnter(strEvent);
	}
	
	public void AnalyticsEnter(string strEvent) {
		dictTimeTracker[strEvent] = Time.fixedTime;
		Analytics.CustomEvent(strEvent, new Dictionary<string, object>{
			{"timeGame", Time.fixedTime}
		});
	}	

	public void AnalyticsExit(string strEvent, string keyProp=null, object valProp=null) {
		Dictionary<string, object> dictEvent = new Dictionary<string, object>();
		if (dictTimeTracker.ContainsKey(strEvent)) {
			dictEvent.Add("ellapsed", Time.fixedTime - dictTimeTracker[strEvent]);
			dictEvent.Add("timeGame", Time.fixedTime);
			dictTimeTracker.Remove(strEvent);
		}
		if (keyProp != null && valProp != null) {
			dictEvent.Add(keyProp, valProp);
		}
		Analytics.CustomEvent(strEvent, dictEvent);
	}	


	// Update is called once per frame
	protected void Update () {
		//initialize the game
		if (gameState == GameState.STATE_INVALID) {
			SetGameState (GameState.STATE_STARTUP);
			SetGameState (GameState.STATE_NORMAL);
		}

		//trigger stop of walking?
		if (soundStop > 0.0f && soundStop < Time.fixedTime) {
			WalkStop();
		}

		if (timeAutoAction > 0.0f && Time.time>timeAutoAction) {
			if (autoDisable) {
				autoDisable.SetActive (false);
				autoDisable = null;
			}
			if (autoEnable) {
				autoEnable.SetActive (true);
				autoEnable = null;
			}
			timeAutoAction = -1f;
		}
	}

	public void SetGameState(string value)
	{
		switch (value) {
		case "start":
			SetGameState (GameState.STATE_STARTUP);
			break;
		case "locked":
			SetGameState (GameState.STATE_ENGAGED);
			break;
		case "normal":
			SetGameState (GameState.STATE_NORMAL);
			break;
		default:
			break;
		}
	}

	public void SetGameState(GameState value)
	{
		if (gameState == value)
			return;
		gameState = value;
		float moveDelay = 0.0f;

		GameObject objActive = null;
		GameObject objLocal = null;
		switch (gameState) {
		default:
		case GameState.STATE_INVALID:
			return;
		case GameState.STATE_STARTUP:
			Camera.main.DOColor (Color.black, 2).From ();
			objLocal = FindWaypoint ("start");
			objLocal.SetActive (true);
			Quaternion targetRotation = objLocal.transform.rotation;
			targetRotation.x = targetRotation.z = 0;
			MovePlayer (objLocal.transform.position, targetRotation, null, false);
			//gameAutoProgress = GameState.STATE_PUZZLE1;
			//objHud.ActivateHUD ("Welcome to the Puzzle, version 1.");
			break;
		case GameState.STATE_ENGAGED:
			break;
		case GameState.STATE_NORMAL:
			break;
		}
		Debug.Log ("GameController: Entering new state: " + gameState);

		//allow position and rotation to move...
		if (objActive) {
			MovePlayer (objActive.transform.position, objActive.transform.rotation, null, true, moveDelay);
		}
	}
		

	/// ---------------- player movement capabilities --------------------

	/// <summary>
	/// Moves player to next navigation point (via dash, not teleport)
	/// </summary>
	public void MovePlayer() {
		if (raycastIndicator.activeSelf) {
			MovePlayer (_navpoint);
		}
	}

	public void MovePlayer(Vector3 posNew, GameObject[] objPath=null, bool dash=true, float moveDelay=0.0f) {
		MovePlayer (posNew, objPlayer.transform.rotation, objPath, dash, moveDelay);
	}

	public void MovePlayer(Vector3 posNew, Quaternion rotNew, GameObject[] objPath=null, 
							bool dash=true, float moveDelay=0.0f) {
		if (this.IsLocked) {
			Debug.Log ("Warning, move requested but game state is locked!");
			return;
		}
		Vector3 positionGo = posNew;
		if (posNew == objPlayer.transform.position) {	// if point is not new, do nothing
			return;
		}
			
		//Debug.Log ("speed: " + moveSpeed + ", dist: " + moveDuration);
		positionGo.y += moveHeight;
		_navpoint = positionGo;
		if (!dash) {
			objPlayer.transform.position = positionGo;
			objPlayer.transform.rotation = rotNew;
			WalkStop();
			return;
		}

		//disabled for now -- method for navigating along path with tween
		if (false) {  /* posPath != null) {
			//convert from objects into positions
			Vector3[] posPath = new Vector3[objPath.Length];
			for (int i=0; i<objPath.Length; i++) {
				posPath[i] = objPath[i].transform.position;
			}

			// update the position
			float moveDuration = Vector3.Distance (objPlayer.transform.position, posPath[0]) / moveSpeed;
			for (int i = 1; i < posPath.Length; i++) {
				moveDuration += Vector3.Distance (posPath[i-1], posPath[i]) / moveSpeed;
			}
			moveDuration += Vector3.Distance (posPath[posPath.Length-1], positionGo) / moveSpeed;

			//start actual tween
			Tween t = objPlayer.transform.DOPath(waypoints, moveDuration, PathType.CatmullRom)
				.SetOptions(true)
				.SetLookAt(0.001f);
			// Then set the ease to Linear 
			t.SetEase(Ease.Linear);
			if (moveDelay > 0f) {
				t.SetDelay (moveDelay);
			}*/
		} else {
			// compute duration by speed
			float moveDuration = Vector3.Distance (objPlayer.transform.position, positionGo) / moveSpeed;
			Sequence mySequence = DOTween.Sequence ();
			mySequence.Append (objPlayer.transform.DOMove (positionGo, moveDuration).SetEase (Ease.Linear));
			if (moveDelay > 0f) {
				mySequence.PrependInterval (moveDelay);
				//TweenSettingsExtensions.Prepend(mySequence, moveDelay);
			}
			WalkStart(moveDuration);
		}
	}

}
