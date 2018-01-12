using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContentInteract: MonoBehaviour {
	public GameObject objTabletPanel;
	public float maxTabletDisplacement = 35;
	public float offscreenDisplacement = 500;
	public float minLookAction = 5;		//at least 5 degree look diff to move tablet
	protected Quaternion lookLast;
	protected const float DURATION_TABLET_SHOW = 1f;
	protected const float ROTATE_SCALAR = 3.0f;

	private bool simulatorActive = false;

	/// <summary>
	/// Centeral game controller
	/// </summary>
	protected GameController gameController;

	// Use this for initialization
	void Start () {
		GameObject objTag = GameObject.FindGameObjectsWithTag ("GameController") [0];
		gameController = objTag.GetComponent<GameController>();
		
		lookLast = Camera.main.transform.rotation;	//grab initial camera view
		StopExperience();
			
		//StartExperience();		//trigger for testing
	}
	
	// Update is called once per frame
	void Update () {
		if (simulatorActive) {
			Vector3 eulerCam = Camera.main.transform.rotation.eulerAngles;
			Vector3 eulerLast = lookLast.eulerAngles;
			Vector3 direction = eulerCam-eulerLast;
			float fAngleRot = Mathf.Atan2(direction.x * Mathf.Deg2Rad, direction.y * Mathf.Deg2Rad) * Mathf.Rad2Deg;
			float fAngleMagnitude = Quaternion.Angle(Camera.main.transform.rotation, lookLast);
			if (objTabletPanel && fAngleMagnitude > minLookAction) {
				//Debug.Log("rotAngle: "+fAngleRot+", rotMag: "+fAngleMagnitude);
				float fRadDiff = Mathf.Deg2Rad * fAngleRot;
				RectTransform rt = objTabletPanel.GetComponent<RectTransform>();
				//move much faster in Y (vertical) position adjustment
				Vector3 posNew = new Vector3(-(float)Mathf.Cos(fRadDiff)*fAngleMagnitude*ROTATE_SCALAR, 
											 (float)Mathf.Sin(fRadDiff)*fAngleMagnitude*ROTATE_SCALAR*10, 0.0f);
				posNew += rt.anchoredPosition3D;	//add to prior location
				posNew.x = Mathf.Max(-maxTabletDisplacement*2, Mathf.Min(0, posNew.x));	//clamp
				posNew.y = Mathf.Max(-maxTabletDisplacement*2, Mathf.Min(0, posNew.y));
				LeanTween.move(objTabletPanel.GetComponent<RectTransform>(), posNew, 0.5f)
					.setEase( LeanTweenType.easeOutQuad );
			}
			lookLast = Camera.main.transform.rotation;
		}
	}


	/// <summary>
	/// Starts the content experience
	/// </summary>
	public void StartExperience() {
		if (simulatorActive) {
			return;
		}
		simulatorActive = true;
		//gameController.SetGameState (GameController.GameState.STATE_ENGAGED);
		lookLast = Camera.main.transform.rotation;	//grab initial camera view
		gameController.AnalyticsEnter("Content");
		if (objTabletPanel) {
			RectTransform rt = objTabletPanel.GetComponent<RectTransform>();
			Vector3 posNew = new Vector3(-offscreenDisplacement, -offscreenDisplacement, 0f);
			rt.anchoredPosition3D += posNew;
			//objTabletPanel.SetActive(true);
			LeanTween.move(rt, new Vector3(-maxTabletDisplacement, -maxTabletDisplacement, 0f), DURATION_TABLET_SHOW)
				.setEase( LeanTweenType.easeOutQuad ).setDelay(0.5f);
		}

	}

	/// <summary>
	/// Stop the content experience
	/// </summary>
	public void StopExperience() {
		if (objTabletPanel) {
			RectTransform rt = objTabletPanel.GetComponent<RectTransform>();
			Vector3 posNew = new Vector3(-offscreenDisplacement, -offscreenDisplacement, 0f);
			if (!simulatorActive) {	//not active now, move instantly
				rt.anchoredPosition3D = posNew;
				DisablePanel();
			}
			else {
				LeanTween.move(rt, posNew, DURATION_TABLET_SHOW)
					.setEase( LeanTweenType.easeInQuad ).setDelay(0.5f).setOnComplete(DisablePanel);
			}
		}
		simulatorActive = false;
		if (!simulatorActive) {
			return;
		}
		//gameController.MovePlayer (resumePosition, objWaypointExit);
		gameController.AnalyticsExit("Content");
	}

	public void ToggleExperience() {
		if (simulatorActive) {
			StopExperience();
		}
		else {
			StartExperience();
		}
	}

	protected void DisablePanel() {
		//do something else?	
	}

}
