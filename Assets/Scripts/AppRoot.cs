using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FlowInLib;

namespace ProjectR
{
    [RequireComponent(typeof(ConfigManager))]
    // 控制生命周期为整个app的全局对象管理类，例如各种单件、网络层、配置文件读取等
    public class AppRoot : GameRoot
    {
        private bool _bWillQuit = false;
        
        // 计算FPS相关参数
        private static float _UpdateInterval = 1.0f;
        private static double _LastTime;
        private static int _FrameCount;
        private static float _FPS;

        // 全局临时数据
        private static Dictionary<string, object> _dataDict = new Dictionary<string, object>();
        public static object GetGlobalData(string key)
        {
            object data = null;
            _dataDict.TryGetValue(key, out data);
            return data;
        }
        public static void SetGlobalData(string key, object data)
        {
            if (_dataDict.ContainsKey(key))
                _dataDict[key] = data;
            else
                _dataDict.Add(key, data);
        }
        public static void ClearGlobalData()
        {
            _dataDict.Clear();
        }

        public static bool _bStandalone = false;    // 是否为单机模式

        [SerializeField]
        private bool _AutoEnterGame = true;

        #region unity event functions
        public override IEnumerator Start()
        {
            yield return base.Start();

            yield return StartCoroutine(SetupGameSetting());

            _LastTime = Time.realtimeSinceStartup;
            _FrameCount = 0;

            if (_AutoEnterGame)
                StartLoading<GameLevelLoading>();
        }

        public override IEnumerator Init()
        {
            yield return base.Init();

            yield return StartCoroutine(ConfigManager.Instance.Init());
            if (!ConfigManager.Instance.Inited) yield break;
        }

        void Update()
        {
            UpdateFPS();

            if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            {
                if (_bWillQuit)
                    Application.Quit();
                else
                {
                    _bWillQuit = true;
                    Invoke("CancelQuit", 2.0f);
                }
            }
        }

        void CancelQuit()
        {
            _bWillQuit = false;
        }
        #endregion

        #region interal functions
        private IEnumerator SetupGameSetting()
        {
            Application.targetFrameRate = 60;

            //Screen.orientation = ScreenOrientation.AutoRotation;
            //Screen.autorotateToLandscapeLeft = true;
            //Screen.autorotateToLandscapeRight = true;
            //Screen.autorotateToPortrait = false;
            //Screen.autorotateToPortraitUpsideDown = false;
            //Screen.sleepTimeout = SleepTimeout.NeverSleep;

            Caching.CleanCache();
            Caching.maximumAvailableDiskSpace = 1024 * 1024 * 50;
            yield break;
        }
        private void UpdateFPS()
        {
            ++_FrameCount;

            float TimeNow = Time.realtimeSinceStartup;
            if( TimeNow > _LastTime + _UpdateInterval )
            {
                _FPS = (float)(_FrameCount/(TimeNow - _LastTime));
                _FrameCount = 0;
                _LastTime = TimeNow;
            }
        }
        #endregion

        public static float fps
        {
            get
            {
                return _FPS;
            }            
        }

        #region wrapper of Unity.Time
        public static float timeScale
        {
            get
            {
                return Time.timeScale;
            }
            set
            {
                Time.timeScale = Mathf.Clamp(value, 0.0f, 100.0f);
            }
        }

        // 受timeScale影响的逻辑bIgnoreTimescale = false
        // 不受timeScale影响的逻辑bIgnoreTimescale = true，例如UI、背景音乐
        // 同一帧多次获取的值相同
        public static float GetDeltaTime(bool bIgnoreTimescale = false)
        {
            return bIgnoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
        }

        // 场景加载后开始计时
        // 同一帧多次获取的值相同
        public static float GetTimeSinceLevelLoad(bool bIgnoreTimescale = false)
        {
            return bIgnoreTimescale ? Time.unscaledTime : Time.time;
        }
        #endregion
    }
}