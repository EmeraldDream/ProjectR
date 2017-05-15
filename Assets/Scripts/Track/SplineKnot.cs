using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineKnot : MonoBehaviour
{
    public float KnotDistanceToNextKnot;
    public float WorldDistanceToNextKnot;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Vector3 position = transform.position;
        Gizmos.DrawIcon(position, "splineknot.gif", false);
    }
}
