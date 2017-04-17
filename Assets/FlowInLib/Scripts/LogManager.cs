using UnityEngine;
using System.Collections;

namespace FlowInLib
{
    public class LogManager : MonoBehaviour
    {
        private static LogManager mInstance = null;
        public static LogManager Instance { get { return mInstance; } }

        private bool mInited = false;
        public bool Inited { get { return mInited; } }

        void Awake()
        {
            mInstance = this;
        }

        public IEnumerator Init()
        {
            mInited = true;
            yield break;
        }

        public static void Debug(string content)
        {
            UnityEngine.Debug.Log(string.Format("[{0}:{1:F3}][Debug]{2}", Time.frameCount, Time.realtimeSinceStartup, content));
        }

        public static void Warn(string content)
        {
            UnityEngine.Debug.Log(string.Format("[{0}:{1:F3}][Warn]{2}", Time.frameCount, Time.realtimeSinceStartup, content));
        }

        public static void Error(string content)
        {
            UnityEngine.Debug.Log(string.Format("[{0}:{1:F3}][Error]{2}", Time.frameCount, Time.realtimeSinceStartup, content));
        }
    }
}