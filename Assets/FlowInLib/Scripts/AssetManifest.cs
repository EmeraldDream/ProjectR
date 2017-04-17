using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FlowInLib
{
    public class AssetManifest : ScriptableObject
    {
        public List<string> mAssetBundleNameList = new List<string>();
        public List<string> mAssetNameList = new List<string>();
        public List<int> mAssetBundleIndexList = new List<int>();
    }
}