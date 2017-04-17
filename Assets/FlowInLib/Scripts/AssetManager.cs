using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public class AssetRef
    {
        public UnityEngine.Object mAsset = null;
        public bool mLoaded = false;
    }

    public class AssetManager : MonoBehaviour
    {
        private static AssetManager mInstance = null;
        public static AssetManager Instance { get { return mInstance; } }

        private bool mInited = false;
        public bool Inited { get { return mInited; } }

        private Dictionary<string, string> mAsset2BundleDict = new Dictionary<string, string>();
        private Dictionary<string, AssetRef> mRefDict = new Dictionary<string, AssetRef>();

        void Awake()
        {
            mInstance = this;
        }

        public IEnumerator Init()
        {
            // 加载 AssetManifest
            AssetBundleCreateRequest abReq = AssetBundle.LoadFromFileAsync(VersionManager.Instance.GetFilePath("AssetBundles", "assetmanifest"));
            yield return abReq;
            if (abReq == null || abReq.assetBundle == null) yield break;

            AssetBundleRequest assetReq = abReq.assetBundle.LoadAssetAsync("AssetManifest.asset");
            yield return assetReq;
            if (assetReq == null || assetReq.asset == null) yield break;

            AssetManifest am = assetReq.asset as AssetManifest;
            if (am != null)
            {
                for (int i = 0; i < am.mAssetNameList.Count; ++i)
                    mAsset2BundleDict.Add(am.mAssetNameList[i], am.mAssetBundleNameList[am.mAssetBundleIndexList[i]]);
            }

            mInited = true;
        }

        public void AutoHandleAsset(string name, System.Type type = null, bool bInst = false, Action<UnityEngine.Object> callback = null)
        {
            StartCoroutine(DoAutoHandleAsset(name, type, bInst, callback));
        }

        public void AutoHandleAssets(string[] names, System.Type[] types = null, bool bInst = false, Action<UnityEngine.Object[]> callback = null)
        {
            StartCoroutine(DoAutoHandleAssets(names, types, bInst, callback));
        }

        public UnityEngine.Object GetAsset(string name)
        {
            AssetRef assetRef = null;
            if (mRefDict.TryGetValue(name, out assetRef))
                return assetRef.mAsset;

            return null;
        }

        public IEnumerator LoadAsset(string name, System.Type type = null)
        {
            AssetRef assetRef = null;
            if (mRefDict.TryGetValue(name, out assetRef))
            {
                while (!assetRef.mLoaded && assetRef.mAsset == null)
                {
                    yield return null;
                }
                yield break;
            }

            string bundleName;
            if (!mAsset2BundleDict.TryGetValue(name, out bundleName))
            {
                LogManager.Debug("AssetManager::LoadAsset => can't find bundle name for asset " + name);
                yield break;
            }

            assetRef = new AssetRef();
            if (assetRef == null) yield break;
            mRefDict.Add(name, assetRef);

            string[] dependencies = AssetBundleManager.Instance.GetAssetBundleDependencies(bundleName);

            Coroutine[] crtList = new Coroutine[dependencies.Length + 1];
            crtList[crtList.Length - 1] = StartCoroutine(AssetBundleManager.Instance.LoadAssetBundle(bundleName));
            for (int i = 0; i < dependencies.Length; ++i)
                crtList[i] = StartCoroutine(AssetBundleManager.Instance.LoadAssetBundle(dependencies[i]));

            int waitIndex = 0;
            while (waitIndex < crtList.Length)
            {
                yield return crtList[waitIndex];
                ++waitIndex;
            }
            
            AssetBundle ab = AssetBundleManager.Instance.GetAssetBundle(bundleName);
            if (ab != null)
            {
                AssetBundleRequest req = ab.LoadAssetAsync(name, type == null ? typeof(GameObject) : type);
                if (req != null)
                {
                    yield return req;
                    assetRef.mAsset = req.asset;
                }
            }
            assetRef.mLoaded = true;

            AssetBundleManager.Instance.UnloadAssetBundle(bundleName);
            for (int i = 0; i < dependencies.Length; ++i)
                AssetBundleManager.Instance.UnloadAssetBundle(dependencies[i]);
        }

        public void UnloadAllAssets()
        {
            mRefDict.Clear();
        }

        private IEnumerator DoAutoHandleAsset(string name, System.Type type = null, bool bInst = false, Action<UnityEngine.Object> callback = null)
        {
            UnityEngine.Object asset = GetAsset(name);
            if (asset == null)
                yield return StartCoroutine(LoadAsset(name, type));

            asset = GetAsset(name);
            if (asset == null)
                yield break;

            if (bInst)
                Instantiate(asset);

            if (callback != null)
                callback(asset);
        }

        private IEnumerator DoAutoHandleAssets(string[] names, System.Type[] types = null, bool bInst = false, Action<UnityEngine.Object[]> callback = null)
        {
            List<Coroutine> crtList = new List<Coroutine>();

            UnityEngine.Object[] assets = new UnityEngine.Object[names.Length];
            for (int i = 0; i < names.Length; ++i)
            {
                assets[i] = GetAsset(names[i]);
                if (assets[i] != null) continue;

                System.Type type = null;
                if (types != null && i < types.Length)
                    type = types[i];

                crtList.Add(StartCoroutine(LoadAsset(names[i], type)));
            }

            int waitIndex = 0;
            while (waitIndex < crtList.Count)
            {
                yield return crtList[waitIndex];
                ++waitIndex;
            }

            for (int i = 0; i < names.Length; ++i)
            {
                if (assets[i] == null)
                    assets[i] = GetAsset(names[i]);

                if (bInst && assets[i] != null)
                    Instantiate(assets[i]);
            }

            if (callback != null)
                callback(assets);
        }
    }
}