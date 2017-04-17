using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public class ConfigManager : MonoBehaviour
    {
        private static ConfigManager mInstance = null;
        public static ConfigManager Instance { get { return mInstance; } }

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
    }
}