using UnityEngine;
using System.Collections;

namespace FlowInLib
{
    [RequireComponent(typeof(LogManager))]
    [RequireComponent(typeof(VersionManager))]
    [RequireComponent(typeof(AssetBundleManager))]
    [RequireComponent(typeof(AssetManager))]
    public class GameRoot : MonoBehaviour
    {
        private static GameRoot mInstance = null;
        public static GameRoot Instance { get { return mInstance; } }

        private bool mInited = false;
        public bool Inited { get { return mInited; } }

        // UI Controller
        [SerializeField]
        protected UIController mUIController;
        public static UIController UI
        {
            get { return Instance ? Instance.mUIController : null; }
        }

        void Awake()
        {
            mInstance = this;
            GameObject.DontDestroyOnLoad(gameObject);
        }

        virtual public IEnumerator Start()
        {
            yield return StartCoroutine(Init());
        }

        virtual public IEnumerator Init()
        {
            // 初始化各种单例组件
            yield return StartCoroutine(LogManager.Instance.Init());
            if (!LogManager.Instance.Inited) yield break;

            yield return StartCoroutine(VersionManager.Instance.Init());
            if (!VersionManager.Instance.Inited) yield break;

            yield return StartCoroutine(AssetBundleManager.Instance.Init());
            if (!AssetBundleManager.Instance.Inited) yield break;

            yield return StartCoroutine(AssetManager.Instance.Init());
            if (!AssetManager.Instance.Inited) yield break;

            mInited = true;
        }

        #region 过场动画
        public static bool StartLoading<T>(Loading.delegateLoading cbLoad = null, Loading.delegateLoading cbAnim = null) where T : Loading
        {
            if (Instance == null)
            {
                LogManager.Error("[AppRoot::StartLoading]AppRoot is not initiated yet!");
                return false;
            }

            T loadingCom = Instance.gameObject.GetComponent<T>();
            if (loadingCom != null)
                return true;

            loadingCom = Instance.gameObject.AddComponent<T>();
            if (loadingCom == null)
            {
                LogManager.Error("[AppRoot::StartLoading]Failed to create loading [" + typeof(T).Name + "]");
                return false;
            }
            loadingCom._LoadEventHandler += cbLoad;
            loadingCom._AnimEventHandler += cbAnim;
            return true;
        }
        #endregion
    }
}