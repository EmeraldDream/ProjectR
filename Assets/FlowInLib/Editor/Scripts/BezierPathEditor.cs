using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    [CustomEditor(typeof(BezierPath))]
    public class BezierPathEditor : Editor
    {
        void OnSceneGUI()
        {
            BezierPath b = target as BezierPath;

            if (b.mPoints.Count <= 0)
                return;

            //b.mPoints.Clear();
            //b.mPoints.Add(new Vector3(0, 0, 0));
            //b.mPoints.Add(new Vector3(1, 0, 1));
            //b.mPoints.Add(new Vector3(1, 2, 2));
            //b.mPoints.Add(new Vector3(3, 2, -2));

            Handles.color = Color.green;
            for (int i = 0; i < b.mPoints.Count; ++i)
            {
                Handles.SphereHandleCap(0, b.mPoints[i], Quaternion.identity, 0.5f, EventType.Repaint);
            }

            Handles.color = Color.red;
            int num = 10;
            float tunit = 1.0f / (float)num;
            for (int i = 0; i < num; ++i)
            {
                Handles.SphereHandleCap(0, b.GetPoint(i * tunit), Quaternion.identity, 0.1f, EventType.Repaint);
            }
        }
    }
}