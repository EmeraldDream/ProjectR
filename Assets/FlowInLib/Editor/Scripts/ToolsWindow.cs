using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace FlowInLib
{
    public class ToolsWindow : EditorWindow
    {
        private BuildTarget mTargetPlatform = BuildTarget.StandaloneWindows64;

        [MenuItem("FlowInLib/Open Window")]
        static void ShowWindow()
        {
            EditorWindow.GetWindow<ToolsWindow>();
        }

        void OnGUI()
        {
            if (GUILayout.Toggle(mTargetPlatform == BuildTarget.StandaloneWindows64, "Windows", EditorStyles.radioButton))
                mTargetPlatform = BuildTarget.StandaloneWindows64;

            if (GUILayout.Toggle(mTargetPlatform == BuildTarget.Android, "Android", EditorStyles.radioButton))
                mTargetPlatform = BuildTarget.Android;

            bool bBuild = false;
            BuildAssetBundleOptions opt = BuildAssetBundleOptions.ChunkBasedCompression;
            if (GUILayout.Button("Build asset bundles"))
            {
                bBuild = true;
            }

            if (GUILayout.Button("Rebuild asset bundles"))
            {
                bBuild = true;
                opt |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }

            if (bBuild)
            {
                Dictionary<string, string> asset2BundleDict = new Dictionary<string, string>();

                // 1.刷新打包资源归属路径
                RefreshAssetImporters(asset2BundleDict);

                // 2.创建 asset 到 assetbundle 的映射表
                string assetManifestFolder = "Assets/AssetManifest";
                CreateAssetManifest(assetManifestFolder, asset2BundleDict);

                // 3.确保目标文件夹存在
                string saveFolder = Path.Combine(Application.dataPath, "StreamingAssets/AssetBundles");
                if (!Directory.Exists(saveFolder))
                    Directory.CreateDirectory(saveFolder);

                // 4.执行打包
                BuildPipeline.BuildAssetBundles(saveFolder, opt, mTargetPlatform);

                // 5.删除无用的manifest文件
                string[] files = Directory.GetFiles(saveFolder);
                for (int i = 0; i < files.Length; ++i)
                {
                    if (files[i].EndsWith(".manifest"))
                    {
                        File.Delete(files[i]);
                        File.Delete(files[i] + ".meta");
                    }
                }

                // 6.删除映射表临时目录
                Directory.Delete(assetManifestFolder, true);
                File.Delete(assetManifestFolder + ".meta");

                AssetDatabase.Refresh();

                Debug.Log("build asset bundles successfully!");
            }

            if (GUILayout.Button("Pack android version"))
            {
                VersionPacker.Pack("Android");
            }
        }

#region 刷新资源所属的ab包名
        void RefreshAssetImportersInFolder(string folder, string assetBundleName, Dictionary<string, string> asset2BundleDict = null)
        {
            string[] tempFiles = Directory.GetFiles(folder);
            for (int i = 0; i < tempFiles.Length; ++i)
            {
                if (tempFiles[i].EndsWith(".meta") || tempFiles[i].EndsWith(".cs"))
                    continue;

                string relativeFile = tempFiles[i].Substring(Application.dataPath.Length - 6);
                AssetImporter importer = AssetImporter.GetAtPath(relativeFile);
                if (importer != null)
                    importer.SetAssetBundleNameAndVariant(assetBundleName, "");

                if (asset2BundleDict != null && string.IsNullOrEmpty(assetBundleName) == false)
                {
                    string fileName = Path.GetFileName(tempFiles[i]);
                    string abName;
                    if (!asset2BundleDict.TryGetValue(fileName, out abName))
                        asset2BundleDict.Add(fileName, assetBundleName);
                    else
                        LogManager.Debug(string.Format("Already has an asset named [{0}] in [{1}] asset bundle, please check asset bundle [{2}]", fileName, abName, assetBundleName));
                }
            }
        }

        void RefreshAssetImporters(Dictionary<string, string> asset2BundleDict = null)
        {
            List<string> excludeFolders = new List<string> { "flowinlib", "ngui", "editor", "streamingassets", "scripts", "scenes", "easytouchbundle", "resources" };

            RefreshAssetImportersInFolder(Application.dataPath, "", asset2BundleDict);

            List<string> folders = new List<string>();
            folders.AddRange(Directory.GetDirectories(Application.dataPath));

            List<bool> noSubfolders = new List<bool>();
            for (int i = 0; i < folders.Count; ++i)
                noSubfolders.Add(false);

            while (folders.Count > 0)
            {
                string curFolder = folders[0];
                folders.RemoveAt(0);

                bool bIgnoreSubfolder = noSubfolders[0];
                noSubfolders.RemoveAt(0);

                string[] tempFolders = Directory.GetDirectories(curFolder);
                folders.AddRange(tempFolders);

                bool bExclude = bIgnoreSubfolder || excludeFolders.Contains(Path.GetFileName(curFolder).ToLower());
                for (int i = 0; i < tempFolders.Length; ++i)
                    noSubfolders.Add(bExclude);

                string relativeFolder = curFolder.Substring(Application.dataPath.Length - 6);
                string assetBundleName = relativeFolder.ToLower().Replace('/', '_').Replace('\\', '_');

                RefreshAssetImportersInFolder(curFolder, bExclude ? "" : assetBundleName, asset2BundleDict);
            }
        }
#endregion

        void CreateAssetManifest(string saveFolder, Dictionary<string, string> asset2BundleDict)
        {
            string[] includeExtensions = new string[] { ".prefab", ".bytes" };

            AssetManifest am = ScriptableObject.CreateInstance<AssetManifest>();
            if (am == null) return;

            foreach (var item in asset2BundleDict)
            {
                for (int i = 0; i < includeExtensions.Length; ++i)
                {
                    if (item.Key.EndsWith(includeExtensions[i]))
                    {
                        int assetBundleNameIndex = am.mAssetBundleNameList.IndexOf(item.Value);
                        if (assetBundleNameIndex < 0)
                        {
                            am.mAssetBundleNameList.Add(item.Value);
                            assetBundleNameIndex = am.mAssetBundleNameList.Count - 1;
                        }

                        am.mAssetNameList.Add(item.Key);
                        am.mAssetBundleIndexList.Add(assetBundleNameIndex);
                        break;
                    }
                }
            }

            if (Directory.Exists(saveFolder))
                Directory.Delete(saveFolder, true);
            Directory.CreateDirectory(saveFolder);

            string tempFile = saveFolder + "/AssetManifest.asset";
            AssetDatabase.CreateAsset(am, tempFile);
            AssetDatabase.SaveAssets();

            AssetImporter importer = AssetImporter.GetAtPath(tempFile);
            if (importer != null)
                importer.SetAssetBundleNameAndVariant("AssetManifest", "");
        }
    }
}