using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathingTable : MonoBehaviour {
	List<string> pois;

	// Use this for initialization
	void Start () {
		foreach (string s in pois) {
			Debug.Log (pois);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
