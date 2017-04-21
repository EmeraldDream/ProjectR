using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour {

    private Camera m_camera = null;

	// Use this for initialization
	void Start () {
        m_camera = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        m_camera.transform.position = gameObject.transform.position - 
            gameObject.transform.forward * 5 + gameObject.transform.up * 3;
        m_camera.transform.LookAt(gameObject.transform.position, Vector3.up);
	}
}
