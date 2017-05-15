using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    Transform playerTransform;

    TrackWalker walker;

    // Use this for initialization
    void Start ()
    {
        playerTransform = GetComponent<Transform>();
        walker = (GameObject.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator).GetWalker();
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(walker != null)
        {
            walker.GetTransfrom(LaneType.Lane0, 0.2f, ref playerTransform);
        }
        else
        {
            walker = (GameObject.FindObjectOfType(typeof(TrackGenerator)) as TrackGenerator).GetWalker();
        }
    }
}
