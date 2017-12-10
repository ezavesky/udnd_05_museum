using UnityEngine;
using System.Collections;
using DG.Tweening;

public class RaycastMovement : MonoBehaviour {
	public GameObject raycastHolder;
	public GameObject raycastIndicator;
	/// <summary>
	/// Centeral game controller
	/// </summary>
	protected GameController gameController;

	//public float maxMoveDistance = 10;

	RaycastHit hit;
	float theDistance;

	void Start() {
		GameObject objTag = GameObject.FindGameObjectsWithTag ("GameController") [0];
		gameController = objTag.GetComponent<GameController>();
	}

	// Simple Macro for click detection
	bool Clicked() {
		//return Input.GetMouseButtonDown (0);
		return (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began);
	}

	// Update is called once per frame
	void Update () {
		if (gameController.IsLocked) {
			return;
		}

		Vector3 forwardDir = raycastHolder.transform.TransformDirection (Vector3.forward) * 100;
		//Debug.DrawRay (raycastHolder.transform.position, forwardDir, Color.green);

		if (Physics.Raycast (raycastHolder.transform.position, (forwardDir), out hit)) {
			//Debug.Log (hit.collider.gameObject);
			if (hit.collider.gameObject.tag == "movementCapable") {
				ManageIndicator ();
				if (true) { // no max dist -- if (hit.distance <= maxMoveDistance) { //If we are close enough
					//If the indicator isn't active already make it active.
					if (raycastIndicator.activeSelf == false) {
						raycastIndicator.SetActive (true);
					}
					//update hit
					//Debug.Log(hit.point);
					gameObject.GetComponent<GameController> ().navpoint = hit.point;
				} /* else {
					if (raycastIndicator.activeSelf == true) {
						raycastIndicator.SetActive (false);
					}
				} */
			} else {
				ManageIndicator (false);
			}
		}
	}

	public void ManageIndicator(bool enabled=true) {
		raycastIndicator.SetActive (enabled);
		if (!enabled) {
			return;
		}
		raycastIndicator.transform.position = hit.point;
		/*
		if (!teleport) {
			if (moving != true) {
				raycastIndicator.transform.position = hit.point;
			}
			if(Vector3.Distance(raycastIndicator.transform.position, player.transform.position) <= 2.5) {
				moving = false;
			}

		} else {
			raycastIndicator.transform.position = hit.point;
		}*/
	}

}
