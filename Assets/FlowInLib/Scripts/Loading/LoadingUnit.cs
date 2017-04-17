using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public abstract class LoadingUnit
    {
        private bool _isCompleted = false;

        public virtual int Weight
        {
            get { return 0; }
        }

        public virtual float Progress
        {
            get { return 0; }
        }

        public bool IsCompleted
        {
            get { return _isCompleted; }
            set { _isCompleted = value; }
        }

        public virtual IEnumerator DoWork()
        {
            yield break;
        }

        public virtual void OnEvent(string strEvent, System.Object param)
        {
        }
    }
}
