using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlanNode : MonoBehaviour {
	public float load = 0.0f;
	public float force = 0.0f;
	public bool activeLight = false;

	public Light interactLight;
	public GameObject interactTarget;

	public void Toggle()
	{
		if (interactTarget == null) {
			Debug.Log ("OnPointerClick called, but no target");
			return;
		}
		interactTarget.GetComponent<PlanInteract>().ToggleNode (this.gameObject);
	}
}
