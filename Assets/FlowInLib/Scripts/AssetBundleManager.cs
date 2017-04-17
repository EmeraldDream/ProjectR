using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace FlowInLib
{
    public class AssetBundleRef
    {
        public AssetBundle mAB = null;
        public AssetBundleCreateRequest mReq = null;
        private int mRefCount = 0;

        public void IncRef() { ++mRefCount; }
        public void DecRef() { --mRefCount; }
        public bool HasRef() { return mRefCount > 0; }
        public void StopAndUnload()
        {
            if (mReq != null)
                mReq = null;

            if (mAB != null)
            {
                mAB.Unload(false);
                mAB = null;
            }
        }
    }

    public class AssetBundleManager : MonoBehaviour
    {
        private static AssetBundleManager mInstance = null;
        public static AssetBundleManager Instance { get { return mInstance; } }

        private bool mInited = false;
        public bool Inited { get { return mInited; } }
        
        private AssetBundleManifest mABManifest = null;

        private Dictionary<string, AssetBundleRef> mRefDict = new Dictionary<string, AssetBundleRef>();

        void Awake()
        {
            mInstance = this;
        }

        public IEnumerator Init()
        {
            // 加载 AssetBundleManifest
            AssetBundleCreateRequest abReq = AssetBundle.LoadFromFileAsync(VersionManager.Instance.GetFilePath("AssetBundles", "AssetBundles"));
            yield return abReq;
            if (abReq == null || abReq.assetBundle == null) yield break;

            AssetBundleRequest assetReq = abReq.assetBundle.LoadAssetAsync("AssetBundleManifest");
            yield return assetReq;
            if (assetReq == null || assetReq.asset == null) yield break;

            mABManifest = assetReq.asset as AssetBundleManifest;
            mInited = true;
        }

        public AssetBundle GetAssetBundle(string name)
        {
            AssetBundleRef abRef = null;
            if (mRefDict.TryGetValue(name, out abRef))
                return abRef.mAB;

            return null;
        }

        public IEnumerator LoadAssetBundle(string name)
        {
            AssetBundleRef abRef = null;
            if (mRefDict.TryGetValue(name, out abRef))
            {
                abRef.IncRef();
                while (abRef.mReq != null && abRef.mAB == null) yield return null;
                yield break;
            }

            abRef = new AssetBundleRef();
            if (abRef == null) yield break;
            abRef.IncRef();
            mRefDict.Add(name, abRef);

            abRef.mReq = AssetBundle.LoadFromFileAsync(VersionManager.Instance.GetFilePath("AssetBundles", name));
            yield return abRef.mReq;

            // 创建失败或者被强行中断
            if (abRef.mReq == null) yield break;
            // 加载失败
            if (abRef.mReq.assetBundle == null)
            {
                LogManager.Debug(string.Format("{0}::{1} => can't load {2}", "AssetBundleManager", "LoadAssetBundle", name));
                yield break;
            }

            abRef.mAB = abRef.mReq.assetBundle;
            abRef.mReq = null;
        }

        public void UnloadAssetBundle(string name)
        {
            AssetBundleRef abRef = null;
            if (mRefDict.TryGetValue(name, out abRef))
            {
                abRef.DecRef();

                if (!abRef.HasRef())
                {
                    abRef.StopAndUnload();
                    mRefDict.Remove(name);
                }
            }
        }

        public void UnloadAllAssetBundles()
        {
            foreach (var item in mRefDict)
                item.Value.StopAndUnload();

            mRefDict.Clear();
        }

        public string[] GetAssetBundleDependencies(string name)
        {
            return mABManifest.GetAllDependencies(name);
        }
    }
}