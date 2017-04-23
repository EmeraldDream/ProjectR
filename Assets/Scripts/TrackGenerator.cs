using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackGenerator : MonoBehaviour
{
    public GameObject[] TrackPieceArray;
    public float TotalTrackLength = 0.0f;

    // Use this for initialization
    void Start ()
    {
        Vector3 curPos = Vector3.zero;
        Quaternion quaternion = Quaternion.identity;
        int count = 0;
        while (TotalTrackLength>=0)
        {
            int value = 0;
            if (++count>4)
            {
                value = Random.Range(0, 2);
                count = 0;
            }
            else
            {
                value = 2;
            }
            
            GameObject curTrack = Instantiate(TrackPieceArray[value], curPos, quaternion);

            TotalTrackLength -= 16;

            Transform curMat = curTrack.transform.Find("out");
            curPos = curMat.position;
            quaternion = curMat.rotation;
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
	}
}
