using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public enum LaneType
{
    Lane0,
    Lane1,
    Lane2
}

public class TrackWalker
{
    private List<TrackSegment> m_TrackSegmentArray;

    private int m_SegmentIndex;

    private int m_BeginKnot;

    private float m_DistanceTravelledFromBeginKnot;

    public TrackWalker(List<TrackSegment> array)
    {
        m_TrackSegmentArray = array;

        m_SegmentIndex = 0;

        m_BeginKnot = 0;
    }

    public void GetTransfrom(LaneType type, float distance, ref Transform mat)
    {
        if(m_SegmentIndex < m_TrackSegmentArray.Count)
        {
            TrackSegment segment = null;
            int knotsCount = 0;
            var controlPoints = new ControlPoint();
            var controlUps = new ControlPoint();
            var lane = 0;
            float factor = 0;
            switch (type)
            {
                case LaneType.Lane0: lane = 0; break;
                case LaneType.Lane1: lane = 1; break;
                case LaneType.Lane2: lane = 2; break;
            }

            while (m_SegmentIndex < m_TrackSegmentArray.Count)
            {
                segment = m_TrackSegmentArray[m_SegmentIndex];

                knotsCount = segment.SplineLane[lane].GetKnotsCount();

                segment.SplineLane[lane].GetControlPointsFor(m_BeginKnot, ref controlPoints);
                segment.SplineLane[lane].GetControlUpsFor(m_BeginKnot, ref controlUps);

                Vector3 toNextKnot = (controlPoints[2] - controlPoints[1]);
                float linearDistToNextKnot = toNextKnot.magnitude;

                m_DistanceTravelledFromBeginKnot += distance;

                if(m_DistanceTravelledFromBeginKnot >= linearDistToNextKnot)
                {
                    m_DistanceTravelledFromBeginKnot -= linearDistToNextKnot;
                    m_BeginKnot++;

                    if (m_BeginKnot == knotsCount-1)
                    {
                        m_BeginKnot = 0;
                        m_SegmentIndex++;
                    }
                }
                else
                {
                    factor = m_DistanceTravelledFromBeginKnot / linearDistToNextKnot;

                    if (m_SegmentIndex < m_TrackSegmentArray.Count)
                    {
                        Vector3 newPosition = Spline.CalculatePoint(ref controlPoints, factor);
                        Vector3 forward = Spline.CalculateTangent(ref controlPoints, factor);
                        Vector3 up = Spline.CalculatePoint(ref controlUps, factor);
                        mat.position = newPosition;
                        mat.rotation = Quaternion.LookRotation(forward, up);
                    }

                    break;
                }
            }
        }
    }
};