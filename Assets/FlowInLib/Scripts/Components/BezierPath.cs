using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public class BezierPath : MonoBehaviour
    {
        public List<Vector3> mPoints = new List<Vector3>();

        // Use this for initialization
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void SetPoints(List<Vector3> points)
        {
            mPoints.Clear();
            mPoints.AddRange(points);
        }

        public Vector3 GetPoint(float t)
        {
            int n = mPoints.Count - 1;
            if (n < 0)
                return Vector3.zero;
            else if (n == 0)
                return mPoints[0];
            else if (n == 1)
                return mPoints[0] + (mPoints[1] - mPoints[0]) * t;

            Vector3 res = Vector3.zero;
            for (int i = 0; i <= n; ++i)
            {
                res += ComputeCombination(n, i) * mPoints[i] * Mathf.Pow(1 - t, n - i) * Mathf.Pow(t, i);
            }
            return res;
        }

        public int ComputeCombination(int n, int m)
        {
            if (m * 2 > n)
                m = n - m;

            if (m == 0)
                return 1;

            return ComputeFactorial(n) / (ComputeFactorial(n - m) * ComputeFactorial(m));
        }

        public int ComputeFactorial(int x)
        {
            int res = 1;
            for (int i = 2; i <= x; ++i)
                res *= i;
            return res;
        }
    }
}