using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollider : MonoBehaviour {
	protected GameController gameController;

	void Start () {
		GameObject objTag = GameObject.FindGameObjectsWithTag ("GameController") [0];
		gameController = objTag.GetComponent<GameController>();
	}

	void OnTriggerEnter(Collider other) {
		//NOTE: we could restrict by the other object having a specific tag, but for now we accept all
		
        Debug.Log("PlayerTrigger: " + other.gameObject.name);
		gameController.AnalyticsTrigger(other.gameObject.name);

		//trigger volume toggle for rooms as entered..
		VolumeToggle volToggle = other.gameObject.GetComponent<VolumeToggle>();
		if (volToggle != null) {
			volToggle.Toggle();
		}
    }
	/* 
    void OnCollisionEnter(Collision collision) {
        Debug.Log("OnCollisionEnter");
        Debug.Log(collision.collider.gameObject);
    }
 
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Debug.Log("OnControllerColliderHit");
 
    }
	*/
}
