﻿using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.SceneManagement;

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

		//force start at the beginning
		navpoint = objPlayer.transform.position;
		/*iTween.CameraFadeFrom(iTween.Hash
			"amount", 0, "time", 2.0f
		);*/

		//load our different scenes
		//SceneManager.LoadScene("ScenePlayer", LoadSceneMode.Additive);
		//SceneManager.LoadScene("RoomWelcome", LoadSceneMode.Additive);

	}
	
	// Update is called once per frame
	protected void Update () {
		//initialize the game
		if (gameState == GameState.STATE_INVALID) {
			SetGameState (GameState.STATE_STARTUP);
			SetGameState (GameState.STATE_NORMAL);
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
		

	/// <summary>
	/// Moves player to next navigation point (via dash, not teleport)
	/// </summary>
	public void MovePlayer() {
		MovePlayer (_navpoint);
	}

	public void Clicked() {
		Debug.Log("CLICKED!");
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
		}
	}

}
