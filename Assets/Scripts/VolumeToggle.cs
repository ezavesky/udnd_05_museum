using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VolumeToggle : MonoBehaviour {
	public GameObject[] muteObjects;
	public UnityEvent toggleEvent = null;
	protected float[] volumeObjects;
	private const float TRANSITION_TIME = 1.0f;
	
	// Use this for initialization
	void Start () {
		//loop through all known objects, grab their volume and mute
		volumeObjects = new float[muteObjects.Length];
		for (int i=0; i<muteObjects.Length; i++) {
			GvrAudioSource audSrc = muteObjects[i].GetComponent<GvrAudioSource>();
			volumeObjects[i] = 0.0f;
			if (audSrc != null) {
				volumeObjects[i] = audSrc.volume;
				audSrc.mute = true;
			}
		}
	}

	//helper callback for volume walk
	private void updateVolume(float fVal, object objRaw) {
		GameObject objTarget = (GameObject)objRaw;
		GvrAudioSource audSrc = objTarget.GetComponent<GvrAudioSource>();
		if (audSrc != null) {
			audSrc.volume = fVal;
			if (fVal == 0.0f) {
				audSrc.mute = true;
			}
		}
	}

	// toggle all objects to max or no volume, based on their currnet state
	public void Toggle () {
		for (int i=0; i<muteObjects.Length; i++) {
			GvrAudioSource audSrc = muteObjects[i].GetComponent<GvrAudioSource>();
			if (audSrc != null) {
				float fValTo, fValFrom;
				fValTo = fValFrom = volumeObjects[i];
				if (audSrc.mute) {
					audSrc.mute = false;
					fValFrom = 0;
				}
				else {
					//audSrc.mute = true; -- handled in callback function
					fValTo = 0;
				}
				//tween between a few avalues
				LeanTween.value(muteObjects[i], callOnUpdate:updateVolume, from:fValFrom, to:fValTo, time:VolumeToggle.TRANSITION_TIME);
			}
		}
		if (toggleEvent != null) {
			toggleEvent.Invoke();
		}
	}
}
