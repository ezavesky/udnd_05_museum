using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeToggle : MonoBehaviour {
	public GameObject[] muteObjects;
	protected float[] volumeObjects;
	
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
	
	// toggle all objects to max or no volume, based on their currnet state
	public void Toggle () {
		for (int i=0; i<muteObjects.Length; i++) {
			GvrAudioSource audSrc = muteObjects[i].GetComponent<GvrAudioSource>();
			if (audSrc != null) {
				//TODO: add tween'd smoothing for audio going to and from levels
				if (audSrc.mute) {
					audSrc.mute = false;
				}
				else {
					audSrc.mute = true;
				}
			}
		}
	}
}
