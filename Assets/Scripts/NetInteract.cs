using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class NetInteract : MonoBehaviour {
	public float moveMax = -0.5f;
	public float scaleMax = 1.19f;
	public float duration = 1f;
	public Text objTextDesc;
	public Text objTextTitle;
	public GameObject objAudio;

	[Serializable]
	public class NetPoint {
		public GameObject objSlice;
		public GameObject objImage;
		public string name;
	}
	protected class NetData {
		public Vector3 startScale;
		public Vector3 startPos;
	}
	public NetPoint[] netPoints;
	protected NetData[] netData;
	protected int netSelected = -1;

	void Start() {
		//iterate through k nown objects and grab initial information
		netData = new NetData[netPoints.Length];
		for (int i=0; i < netPoints.Length; i++) {
			netData [i] = new NetData ();
			netData [i].startScale = new Vector3 (
				netPoints [i].objSlice.transform.localScale.x,
				netPoints [i].objSlice.transform.localScale.y,
				netPoints [i].objSlice.transform.localScale.z);
			netData[i].startPos = netPoints [i].objSlice.transform.localPosition;
		}
		if (netPoints.Length > 0) {
			ZoomIn (netPoints [0].objSlice);
		}
	}

	protected int FindTarget(GameObject objTarget, bool sliceObj=true) {
		for (int i = 0; i < netPoints.Length; i++) {
			if (sliceObj) {
				if (netPoints [i].objSlice == objTarget)
					return i;
			} else {
				if (netPoints [i].objImage == objTarget)
					return i;
			}
		}
		return -1;
	}

	public void ZoomIn (GameObject objTarget) {
		int idx = FindTarget (objTarget);
		if (idx == -1) {
			return;
		}
		if (netSelected != -1) {
			ZoomOut (netPoints [netSelected].objSlice);
		} else if (netSelected == idx) {
			return;
		}

		Vector3 posLocal = new Vector3(netData[idx].startPos.x, netData[idx].startPos.y+moveMax, netData[idx].startPos.z);
		Vector3 posGlobal = objTarget.transform.parent.TransformPoint(posLocal);
		netSelected = idx;
		LTDescr ltDescM = LeanTween.move(objTarget, posGlobal, duration).setEase( LeanTweenType.easeInQuad );
		LTDescr ltDescS = LeanTween.scale(objTarget, netData[idx].startScale * scaleMax, duration).setEase( LeanTweenType.easeInQuad );
		ZoomClickNonRecurse (idx);
		//Debug.Log ("ZOOM IN: " + idx + ", target: " + netPoints[idx].name);
	}
	
	public void ZoomOut (GameObject objTarget) {
		int idx = FindTarget (objTarget);
		if (idx == -1) {
			return;
		}
		Vector3 posGlobal = objTarget.transform.parent.TransformPoint(netData[idx].startPos);
		netSelected = idx;
		LTDescr ltDescM = LeanTween.move(objTarget, posGlobal, duration).setEase( LeanTweenType.easeOutQuad );
		LTDescr ltDescS = LeanTween.scale(objTarget, netData[idx].startScale, duration).setEase( LeanTweenType.easeOutQuad );
		//Debug.Log ("ZOOM OUT: " + idx + ", target: " + netPoints[idx].name);
	}

	public void ClickSlice (GameObject objTarget) {
		int idx = FindTarget (objTarget, true);
		if (idx == -1) {
			return;
		}
		ZoomIn (netPoints[idx].objSlice);
	}

	public void ClickImage (GameObject objTarget) {
		int idx = FindTarget (objTarget, false);
		if (idx == -1) {
			return;
		}
		ZoomIn (netPoints[idx].objSlice);
	}

	protected void ZoomClickNonRecurse (int idx) {
		switch (netPoints [idx].name) {
		default:
			objTextTitle.text = "Easter Egg!";
			objTextDesc.text = "Thanks! You are a true hacker as you've managed to outsmart the game itself!";
			break;
		case "brute":
			objTextTitle.text = "Brute Force Attacks – 20%";
			objTextDesc.text = "Brute force attacks are akin to kicking down the front door of a network. " +
				"Rather than attempting to trick a user into downloading malware, the attacker tries to " +
				"discover the password for a system or service through trial and error.\n\n" +
				"These network attacks can be time consuming, so attackers typically use software to automate " +
				"the task of typing hundreds of passwords.";
			break;
		case "browser":
			objTextTitle.text = "Browser Attacks – 20%";
			objTextDesc.text = "Browser-based network attacks tied for the second-most common type. They attempt "+
				"to breach a machine through a web browser, one of the most common ways people use the internet. " +
				"Browser attacks often start at legitimate, but vulnerable, websites. Attackers breach the site and " +
				"infect it with malware.\n\n"+
				"When new visitors arrive (via web browser), the infected site attempts to force malware onto "+
				"their systems by exploiting vulnerabilities in their browsers.";
			break;
		case "ddos":
			objTextTitle.text = "Denial-of-Service (DDoS) Attacks – 15%";
			objTextDesc.text = "Denial-of-service attacks, also known as distributed denial-of-service attacks (DDoS), "+
				"are third on the list on the list of network security attacks, and they continue to grow stronger "+
				"every year.\n\nDDoS attacks attempt to overwhelm a resource – such as websites, game servers, "+
				"or DNS servers – with floods of traffic. Typically the goal is to slow or crash the system.\n\n"+
				"One in three businesses (33%) experienced a DDoS attack in 2017, according to a Kaspersky Labs "+
				"survey of 5,200 people from businesses in 29 countries. ";
			break;
		case "worm":
			objTextTitle.text = "Worm Attacks – 13%";
			objTextDesc.text = "Malware typically requires user interaction to start infection. For example, "+
				"the person may have to download a malicious email attachment, visit an infected website, or plug "+
				"an infected thumb drive into a machine.\n\n"+
				"Worm attacks spread on their own. They are self-propagating "+
				"malware that does not require user interaction. Typically, they exploit system vulnerabilities to "+
				"spread across local networks and beyond.\n\n"+
				"WannaCry ransomware, which infected more than 300,000 computers in a few days, used worm "+
				"techniques to attack networks and machines.";
			break;
		case "malware":
			objTextTitle.text = "Malware Attacks – 10%";
			objTextDesc.text = "Malware is, of course, malicious software – applications that have been created "+
				"to harm, hijack, or spy on the infect system.\n\n"+
				"It’s not clear why “worm attacks” were not included in this category – as they are typically "+
				"associated with malware. Perhaps it was to emphasize the prevalence of work attacks during Q2 2017.\n\n"+
				"Regardless, malware is widespread and well known. Three common ways it spreads include:"+
				"phishing emails, malicious websites, and malvertising.";
			break;
		case "scan":
			objTextTitle.text = "Scan Attacks – 4%";
			objTextDesc.text = "Rather than outright network attacks, scans are pre-attack reconnaissance. Attackers "+
				"use widely available scanning tools to probe public-facing systems to better understand the services, "+
				"systems, and security in place.\n\n"+
				"Port scanner – A simple tool used to determine a system’s open ports. Several types exist, with some "+
				"intended to prevent detection by the scanned target.\n\n"+
				"Vulnerability scanner – Collects information on a target and compares it to known security "+
				"vulnerabilities. The result is a list of known vulnerabilities on the system and their severity.";
			break;
		case "web":
			objTextTitle.text = "Web Attacks – 4%";
			objTextDesc.text = "Public-facing services – such as web applications and databases – are also targeted "+
				"for network security attacks.\n\n"+
				"The most common web application attacks in Q2 2017, according to Positive Research:"+
				"Cross-Site Scripting (XSS) – 39.1%, SQL Injection (SQLi) – 24.9%, and Path Traversal – 6.6%.";
				
			break;
		case "other":
			objTextTitle.text = "Other Attacks – 14%";
			objTextDesc.text = "We can only speculate on the network attack types bundled into “other”. "+
				"That said, here are some of the usual suspects: \n\n"+
				"Physical Attacks – Attempts to destroy or steal network architecture or systems in an "+
				"old-school, hands-on way. Stolen laptops are a common example.\n\n"+
				"Insider Attacks – Not every network attack is performed by an outsider. Angry employees, "+
				"criminal third-party contractors, and bumbling staff members are just a few potential actors. "+
				"They can steal and abuse access credentials, misuse customer data, or accidentally "+
				"leak sensitive information.\n\n"+
				"Advanced Persistent Threats – The most advanced network attacks are performed by elite teams "+
				"of hackers who adapt and tailor techniques to the target environment. Their goal is usually to "+
				"steal data over an extended period by hiding and “persisting”.";
			break;
		}
		if (objAudio) {
			objAudio.GetComponent<GvrAudioSource>().Play ();
		}

		//Debug.Log ("CLICK: " + idx + ", target: " + netPoints[idx].name);
	}

}
