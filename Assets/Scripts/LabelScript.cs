﻿using UnityEngine;
using System.Collections;

public class LabelScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

    //    transform.LookAt(transform.position + GameManager.instance.cameraRig.viewpoint.transform.rotation * Vector3.forward,
    //    GameManager.instance.cameraRig.viewpoint.transform.rotation * Vector3.up);

        transform.LookAt(GameManager.instance.cameraRig.viewpoint.transform);
    }
}
