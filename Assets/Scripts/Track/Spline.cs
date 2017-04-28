using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class Spline : MonoBehaviour
{
    [SerializeField]
    private List<SplineKnot> m_Knots;

    public float Length { get; set; }

    private Vector3 m_FirstSplineCP;
    private Vector3 m_LastSplineCP;

    public void Start()
    {
        if (m_Knots == null || m_Knots.Count() < 3)
        {
            CacheLinearKnots(m_Knots);
        }
        else
        {
            CacheCatmullRomKnots(m_Knots);
        }

        m_FirstSplineCP = GetAdditionalControlPoint(ControlPointType.Start);
        m_LastSplineCP  = GetAdditionalControlPoint(ControlPointType.End);
    }

    /// <summary>
    /// Get the Pos Before Spline Head Or After Spline Tail
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Vector3 GetAdditionalControlPoint(ControlPointType type)
    {
        Vector3 controlPoint = new Vector3();
        
        if (m_Knots == null || m_Knots.Count < 3)
            return controlPoint;
        
        if (type == ControlPointType.Start)
            controlPoint = GetAdditionalControlPoint(m_Knots[0].transform, m_Knots[1].transform);
        else
            controlPoint = GetAdditionalControlPoint(m_Knots.Last().transform, m_Knots[m_Knots.Count - 2].transform);

        return controlPoint;
    }

    /// <summary>
    /// Get the Pos Along Ray From Dest To Source
    /// </summary>
    /// <param name="source"></param>
    /// <param name="dest"></param>
    /// <returns></returns>
    private Vector3 GetAdditionalControlPoint(Transform source, Transform dest)
    { 
        Vector3 additionalPoint = dest.position - source.position;
        additionalPoint.x = -additionalPoint.x;
        additionalPoint.y = -additionalPoint.y;
        additionalPoint.z = -additionalPoint.z;

        // Add on where we started from and return it
        additionalPoint += source.position;
        return additionalPoint;
    }

    /// <summary>
    /// Get ControlPoint Base on knotIndex
    /// </summary>
    /// <param name="knotIndex"></param>
    /// <param name="controlPointsOut"></param>
    public void GetControlPointsFor(int knotIndex, ref ControlPoint controlPointsOut)
    {
        Assert.IsTrue(knotIndex >= 0 && knotIndex < m_Knots.Count - 1,
                    "knot index " + knotIndex + " is invalid, must be >=0 && < " + (m_Knots.Count - 1));

        controlPointsOut.Control0 = knotIndex == 0 ? m_FirstSplineCP : m_Knots[knotIndex - 1].transform.position;
        controlPointsOut.Control1 = m_Knots[knotIndex].transform.position;
        controlPointsOut.Control2 = m_Knots[knotIndex + 1].transform.position;
        controlPointsOut.Control3 = knotIndex < (m_Knots.Count - 2)
            ? m_Knots[knotIndex + 2].transform.position
            : m_LastSplineCP;
    }

    private void DrawLinearSpline(IList<SplineKnot> knots)
    {
        for (int i = 0; i < knots.Count() - 1; ++i)
        {
            SplineKnot thisKnot = knots[i] as SplineKnot;
            SplineKnot nextKnot = knots[i + 1] as SplineKnot;

            Vector3 thisKnotPosition = thisKnot.transform.position;
            Vector3 nextKnotPosition = nextKnot.transform.position;

            // Just draw a line between them
            Gizmos.DrawLine(thisKnotPosition, nextKnotPosition);
        }
    }

    private void DrawCatmullRomSpline(IList<SplineKnot> knots)
    {
        if (knots.Count() < 3)
            return;

        Vector3 lineStart = knots[0].transform.position;

        for (int i = 0; i < knots.Count() - 1; ++i)
        {
            var controlPoints = new ControlPoint();
            GetControlPointsFor(i, ref controlPoints);
            
            float time = 0.0f;
            float timeStep = 1.0f / (float)(10);
            while (time <= 1.0f)
            {
                Vector3 lineEnd = CalculatePoint(ref controlPoints, time);
                
                Gizmos.DrawLine(lineStart, lineEnd);
                lineStart = lineEnd;

                float stepThisTime = timeStep;
                if (1.0f - time < stepThisTime && 1.0f - time > 0.0001f)
                    stepThisTime = 1.0f - time;
                time += stepThisTime;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (m_Knots == null)
        {
            return;
        }
        
        Gizmos.color = Color.white;

        if (m_Knots.Count() < 3)
            DrawLinearSpline(m_Knots);
        else
            DrawCatmullRomSpline(m_Knots);
    }

    /// <summary>
    /// Calc Spline Lerp
    /// </summary>
    /// <param name="controlPoints"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector3 CalculatePoint(ref ControlPoint controlPoints, float t)
    {
        float sqT = t * t;
        float triT = sqT * t;
        return 0.5f * ((2.0f * controlPoints[1])
                        + (-controlPoints[0] + controlPoints[2]) * t
                        + (2.0f * controlPoints[0] - 5.0f * controlPoints[1] + 4.0f * controlPoints[2] - controlPoints[3]) * sqT
                        + (-controlPoints[0] + 3.0f * controlPoints[1] - 3.0f * controlPoints[2] + controlPoints[3]) * triT
            );
    }

    /// <summary>
    /// Calc Spline Detail From Linear
    /// </summary>
    /// <param name="knots"></param>
    private void CacheLinearKnots(IList<SplineKnot> knots)
    {
        if (knots == null)
        {
            return;
        }

        float totalLength = 0.0f;

        for (int knotIndex = 0; knotIndex < knots.Count() - 1; ++knotIndex)
        {
            Vector3 toNextKnot = (knots[knotIndex + 1].transform.position - knots[knotIndex].transform.position);

            knots[knotIndex].KnotDistanceTravelled = knots[knotIndex].WorldDistanceToNextKnot = toNextKnot.magnitude;

            totalLength += knots[knotIndex].KnotDistanceTravelled;
        }

        Length = totalLength;
    }

    /// <summary>
    /// Calc Spline Detail From CatmullRom
    /// </summary>
    /// <param name="knots"></param>
    private void CacheCatmullRomKnots(IList<SplineKnot> knots)
    {
        float totalLength = 0.0f;

        for (int knotIndex = 0; knotIndex < knots.Count() - 1; ++knotIndex)
        {
            var controlPoints = new ControlPoint();
            GetControlPointsFor(knotIndex, ref controlPoints);

            Vector3 toNextKnot = (controlPoints[2] - controlPoints[1]);
            float linearDistToNextKnot = toNextKnot.magnitude;

            float approxLength = linearDistToNextKnot * 2; // overestimate because of curve
            float traversalStep = 1.0f / approxLength;

            float knotDistanceTravelled = 0.0f;

            float traversalSoFar = 0.0f;
            Vector3 prevPosition = controlPoints[1];
            while (traversalSoFar < 1.0f)
            {
                traversalSoFar = Mathf.Min(1.0f, traversalSoFar + traversalStep);

                Vector3 newPosition = CalculatePoint(ref controlPoints, traversalSoFar);

                float approxMovement = (newPosition - prevPosition).magnitude;
                knotDistanceTravelled += approxMovement;
                prevPosition = newPosition;
            }

            knots[knotIndex].KnotDistanceTravelled  = knotDistanceTravelled;
            knots[knotIndex].WorldDistanceToNextKnot= linearDistToNextKnot;
            totalLength += knotDistanceTravelled;
        }

        Length = totalLength;
    }
}
