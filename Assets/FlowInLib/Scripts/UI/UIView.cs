using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowInLib
{
    public class UIView : MonoBehaviour
    {
        [SerializeField] private string _path = "";

        public enum EGroupMask
        {
            NONE = 0,
            LOADING = 1 << 0,
            STARTUP = 1 << 1,
            LOGIN = 1 << 2,
            LOBBY = 1 << 3,
            ROOM = 1 << 4,
            PLAYING = 1 << 5,
            ALL = -1 // 0xffffffff
        }
        [SerializeField] private EGroupMask[] _groupMaskConfig = null;
        private uint _groupMask = 0;

        void Awake()
        {
            if (_groupMaskConfig != null)
            {
                for (int i = 0; i < _groupMaskConfig.Length; ++i)
                    _groupMask |= (uint)_groupMaskConfig[0];
            }

            UIController UI = GameRoot.UI;
            if (UI == null)
                return;

            if (!UI.AttachViewToPath(Path, this))
            {
                LogManager.Warn(string.Format("[UIView::Init]Attach {0} fail", gameObject.name));
                Destroy(gameObject);
                return;
            }
        }

        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public uint GroupMask
        {
            get { return _groupMask; }
        }

        public string ViewName
        {
            get { return gameObject.name.Replace("(Clone)", ""); }
        }
    }
}
