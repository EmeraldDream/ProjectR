using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowInLib
{
    public class UIController : MonoBehaviour
    {
        private Dictionary<string, UIView> _viewMap = new Dictionary<string,UIView>();

        void Awake()
        {
        }

        void OnDestroy()
        {
            ClearViews((uint)UIView.EGroupMask.NONE);
        }

        public UIView GetView(string name)
        {
            UIView view;
            if (_viewMap.TryGetValue(name, out view))
                return view;
            return null;
        }

        // 将某个UI View添加到指定场景路径下，返回值表示是否成功
        // 路径字符串从"UIRoot"开始并连接各层级的UI对象名，层级之间用'/'隔开。例如："UIRoot/TheParentView"
        public bool AttachViewToPath(string path, UIView view)
        {
            if (view == null)
                return false;

            string[] names = path.Split('/');
            if (names.Length <= 0 || names[0] != "UIRoot")
                return false;

            UIController UI = GameRoot.UI;
            Transform curNode = UI ? UI.transform : null;
            if (curNode == null)
                return false;

            for (int i = 1; i < names.Length; ++i)
            {
                Transform childNode = curNode.FindChild(names[i]);
                if (childNode == null)
                    return false;
                curNode = childNode;
            }

            string viewName = view.ViewName;
            if (!_viewMap.ContainsKey(viewName))
                _viewMap.Add(viewName, view);

            view.transform.SetParent(curNode, false);
            view.Path = path;
            UIRect rect = view.GetComponent<UIRect>();
            if (rect != null)
                rect.SetAnchor(curNode);
            return true;
        }

        public void ClearView(string name)
        {
            UIView view = GetView(name);
            if (view != null)
            {
                _viewMap.Remove(name);
                Destroy(view.gameObject);
            }
        }

        public void ClearViews(uint keepMask = 0)
        {
            List<UIView> uselessViews = new List<UIView>();
            List<string> nullViews = new List<string>();
            foreach (KeyValuePair<string, UIView> pair in _viewMap)
            {
                if (pair.Value == null)
                {
                    nullViews.Add(pair.Key);
                    continue;
                }

                if ((pair.Value.GroupMask & keepMask) == 0)
                    uselessViews.Add(pair.Value);
            }

            foreach (UIView view in uselessViews)
            {
                _viewMap.Remove(view.ViewName);
                Destroy(view.gameObject);
            }

            foreach (string viewName in nullViews)
                _viewMap.Remove(viewName);
        }
    }
}
