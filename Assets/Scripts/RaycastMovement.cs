using UnityEngine;
using System.Collections;
using DG.Tweening;

public class RaycastMovement : MonoBehaviour {
	public GameObject raycastHolder;
	public GameObject player;
	public GameObject raycastIndicator;

	public float height = 2;
	public bool teleport = false;

	public float maxMoveDistance = 10;

	private bool moving = false;

	RaycastHit hit;
	float theDistance;

	// Use this for initialization
	void Start () {

	}

	// Simple Macro for click detection
	bool Clicked() {
		//return Input.GetMouseButtonDown (0);
		return (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Began);
	}

	// Update is called once per frame
	void Update () {

		Vector3 forwardDir = raycastHolder.transform.TransformDirection (Vector3.forward) * 100;
		//Debug.DrawRay (raycastHolder.transform.position, forwardDir, Color.green);

		if (Physics.Raycast (raycastHolder.transform.position, (forwardDir), out hit)) {
			if (hit.collider.gameObject.tag == "movementCapable") {
				ManageIndicator ();
				if (hit.distance <= maxMoveDistance) { //If we are close enough
					//If the indicator isn't active already make it active.
					if (raycastIndicator.activeSelf == false) {
						raycastIndicator.SetActive (true);
					}
				} else {
					if (raycastIndicator.activeSelf == true) {
						raycastIndicator.SetActive (false);
					}
				}
			}
		}
	}

	public void TriggerMove() {
		if (teleport) {
			teleportMove (hit.point);
		} else {
			DashMove (hit.point);
		}
	}


	public void ManageIndicator() {
		if (!teleport) {
			if (moving != true) {
				raycastIndicator.transform.position = hit.point;
			}
			if(Vector3.Distance(raycastIndicator.transform.position, player.transform.position) <= 2.5) {
				moving = false;
			}

		} else {
			raycastIndicator.transform.position = hit.point;
		}
	}
	public void DashMove(Vector3 location) {
		moving = true;
//		Debug.Log ("Position:" + location);
//		Debug.Log ("Previous:" + player.transform);
		player.transform.DOMove (new Vector3 (location.x, location.y + height, location.z), 0.2f).SetEase (Ease.Linear);
	}
	public void teleportMove(Vector3 location) {
		player.transform.position = new Vector3 (location.x, location.y + height, location.z);
	}
}
