using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TrackSegment
{
    public Spline[] SplineLane;

    public TrackSegment(Spline[] spline)
    {
        SplineLane = spline;
    }
};