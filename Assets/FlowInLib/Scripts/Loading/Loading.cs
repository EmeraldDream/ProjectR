using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public abstract class Loading : MonoBehaviour
    {
        #region variable
        public delegate void delegateLoading();
        public event delegateLoading _LoadEventHandler;
        public event delegateLoading _AnimEventHandler;

        protected List<LoadingUnit> _unitList = new List<LoadingUnit>();
        private LoadingUnit _curUnit = null;
        private int _totalWeight;
        private int _completedWeight;
        private bool _isCompleted = false;
        #endregion

        private static List<Loading> _loadingList = new List<Loading>();
        public static void DispatchEvent(string strEvent, System.Object param, Type loadingType = null, Type unitType = null)
        {
            foreach (var loading in _loadingList)
            {
                if (loadingType != null && loading.GetType() != loadingType)
                    continue;

                foreach (var unit in loading._unitList)
                {
                    if (unitType != null && unit.GetType() != unitType)
                        continue;

                    unit.OnEvent(strEvent, param);
                }
            }
        }

        public virtual void Awake()
        {
            _loadingList.Add(this);
        }

        void OnDestroy()
        {
            _loadingList.Remove(this);
            //LogManager.LogDebug("Loading", string.Format("Load {0} over and destroy self", this.GetType().ToString()));
        }

        #region //需要在派生类中重写的成员函数
        protected abstract IEnumerator PrepareLoadingAnim();        // 准备过场动画资源
        protected abstract IEnumerator StartLoadingAnim();          // 更新过场动画显示
        protected abstract IEnumerator EndLoadingAnim();            // 显示结束过场动画
        protected abstract void InitUnitList();
        #endregion

        // Start包含了主要的加载流程逻辑
        IEnumerator Start()
        {
            // 准备Loading动画资源
            yield return StartCoroutine(PrepareLoadingAnim());

            // 开始Loading动画
            Coroutine coAnimStart = StartCoroutine(StartLoadingAnim());

            // 初始化加载单元列表
            InitUnitList();

            // 统计所有单元的总权重，用于计算总进度
            _totalWeight = 0;
            _completedWeight = 0;
            for (int i = 0; i < _unitList.Count; ++i)
            {
                LoadingUnit unit = _unitList[i];
                if (unit != null)
                    _totalWeight += unit.Weight;
            }

            // 开始加载
            while (_unitList.Count > 0)
            {
                _curUnit = _unitList[0];
                if (_curUnit != null)
                {
                    _curUnit.IsCompleted = false;
                    yield return StartCoroutine(_curUnit.DoWork());
                    _curUnit.IsCompleted = true;
                    _completedWeight += _curUnit.Weight;
                }
                _unitList.RemoveAt(0);
            }

            _curUnit = null;
            _isCompleted = true;

            // Load结束回调
            if (_LoadEventHandler != null)
                _LoadEventHandler();

            // 结束Loading动画
            yield return coAnimStart;
            yield return StartCoroutine(EndLoadingAnim());

            // 动画结束回调
            if (_AnimEventHandler != null)
                _AnimEventHandler();

            // 销毁相关UI
            GameRoot.UI.ClearViews(~(uint)UIView.EGroupMask.LOADING);

            // 销毁自己
            Destroy(this);
        }

        // 所有单元是否Loading完毕
        protected bool IsCompleted
        {
            get { return _isCompleted; }
        }

        // 所有单元的Loading进度，范围 0.0~1.0
        protected float TotalProgress
        {
            get
            {
                if (_totalWeight <= 0)
                    return 0;

                float weight = _completedWeight;
                if (_curUnit != null && !_curUnit.IsCompleted)
                    weight += (float)_curUnit.Weight * _curUnit.Progress;

                return weight / (float)_totalWeight;
            }
        }

        // 当前单元的Loading进度，范围 0.0~1.0
        protected float UnitProgress
        {
            get { return _curUnit == null ? 0 : _curUnit.Progress; }
        }
    }
}