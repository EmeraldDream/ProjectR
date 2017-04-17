using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace FlowInLib
{
    [Serializable]
    public class FileInfo
    {
        public string _name;
        public string _md5;
    }

    [Serializable]
    public class FolderInfo
    {
        public string _name;
        public List<FileInfo> _files = new List<FileInfo>();
    }

    [Serializable]
    public class VersionInfo
    {
        public List<FolderInfo> _folders = new List<FolderInfo>();
    }

    public class VersionManager : MonoBehaviour
    {
        private static VersionManager mInstance = null;
        public static VersionManager Instance { get { return mInstance; } }

        private bool mInited = false;
        public bool Inited { get { return mInited; } }

        private bool _updated = false;
        private List<FolderInfo> _diffFolders = new List<FolderInfo>();

        void Awake()
        {
            mInstance = this;
        }

        public IEnumerator Init()
        {
            //yield return StartCoroutine(UpdateFiles());
            _updated = true;

            // 未更新成功
            if (!_updated) yield break;

            mInited = true;
        }

        public string GetFilePath(string folder, string file)
        {
            bool bUpdated = false;

            FolderInfo folderInfo = _diffFolders.Find(obj => obj._name == folder);
            if (folderInfo != null)
            {
                FileInfo fileInfo = folderInfo._files.Find(obj => obj._name == file);
                if (fileInfo != null)
                    bUpdated = true;
            }

            string folderPath = Path.Combine(bUpdated ? Application.persistentDataPath : Application.streamingAssetsPath, folder);
            return Path.Combine(folderPath, file);
        }

        private IEnumerator UpdateFiles()
        {
#if !UNITY_ANDROID
            string updateWwwPath = "file://" + Application.persistentDataPath;
            string localWwwPath = "file://" + Application.streamingAssetsPath;
#else
            string updateWwwPath = Application.persistentDataPath;
            string localWwwPath = Application.streamingAssetsPath;
#endif

            string serverMD5;

            // 获取http服务器上的版本MD5
            using (WWW www = new WWW(GenRandomURL("versionMD5")))
            {
                yield return www;
                if (www == null || !string.IsNullOrEmpty(www.error))
                {
                    LogManager.Error("VersionManager::UpdateFiles => can't get versionMD5 form server");
                    yield break;
                }
                serverMD5 = www.text;
            }

            // 检验客户端安装包自带的版本MD5是否最新
            // 是则删除本地更新文件，并结束更新
            // 否则继续往下执行
            VersionInfo versionInfo = null;
            using (WWW www = new WWW(Path.Combine(localWwwPath, "version")))
            {
                yield return www;
                if (www == null || !string.IsNullOrEmpty(www.error))
                {
                    LogManager.Error("VersionManager::UpdateFiles => can't get version file form client");
                    yield break;
                }

                if (CheckBytesMD5(www.bytes, serverMD5))
                {
                    _updated = true;
                    yield return StartCoroutine(ClearUpdatedFiles(updateWwwPath));
                    yield break;
                }

                versionInfo = JsonUtility.FromJson<VersionInfo>(www.text);
            }

            // 检验更新目录下的版本MD5是否最新
            // 是则结束更新
            // 否则继续往下执行
            VersionInfo oldInfo = null;
            if (File.Exists(Path.Combine(Application.persistentDataPath, "version")))
            {
                using (WWW www = new WWW(Path.Combine(updateWwwPath, "version")))
                {
                    yield return www;
                    if (www == null || !string.IsNullOrEmpty(www.error))
                    {
                        LogManager.Error("VersionManager::UpdateFiles => can't get version file form update path");
                        yield break;
                    }

                    if (CheckBytesMD5(www.bytes, serverMD5))
                    {
                        _updated = true;
                        yield break;
                    }

                    oldInfo = JsonUtility.FromJson<VersionInfo>(www.text);
                }
            }

            // 从服务器获取最新的版本文件
            VersionInfo newInfo = null;
            byte[] newInfoBytes = null;
            using (WWW www = new WWW(GenRandomURL("version")))
            {
                yield return www;
                if (www == null || !string.IsNullOrEmpty(www.error) || !CheckBytesMD5(www.bytes, serverMD5))
                {
                    LogManager.Error("VersionManager::UpdateFiles => can't get version file form server");
                    yield break;
                }

                newInfoBytes = www.bytes;
                newInfo = JsonUtility.FromJson<VersionInfo>(www.text);
                if (newInfo == null)
                {
                    LogManager.Error("VersionManager::UpdateFiles => can't parse version file from server");
                    yield break;
                }
            }

            // 若本地存在更新内容，则删除其中非最新的内容
            if (oldInfo != null)
            {
                for (int i = oldInfo._folders.Count - 1; i >= 0; --i)
                {
                    FolderInfo oldFolderInfo = oldInfo._folders[i];
                    string folder = oldFolderInfo._name;
                    string folderPath = Path.Combine(Application.persistentDataPath, folder);

                    FolderInfo newFolderInfo = newInfo._folders.Find(obj => obj._name == folder);
                    if (newFolderInfo == null)
                    {
                        if (Directory.Exists(folder))
                            Directory.Delete(folder);
                        continue;
                    }

                    for (int j = oldFolderInfo._files.Count; j >= 0; --j)
                    {
                        FileInfo oldFileInfo = oldFolderInfo._files[j];
                        FileInfo newFileInfo = newFolderInfo._files.Find(obj => obj._name == oldFileInfo._name);
                        if (newFileInfo == null || newFileInfo._md5 != oldFileInfo._md5)
                        {
                            string filePath = Path.Combine(folderPath, oldFileInfo._name);
                            if (File.Exists(filePath))
                                File.Delete(filePath);
                        }
                    }
                }
            }

            // 计算安装包版与最新版之间的差异
            for (int i = 0; i < newInfo._folders.Count; ++i)
            {
                FolderInfo newFolder = newInfo._folders[i];
                FolderInfo oldFolder = versionInfo._folders.Find(obj => obj._name == newFolder._name);
                if (oldFolder == null)
                {
                    _diffFolders.Add(newFolder);
                    continue;
                }

                FolderInfo diffFolder = new FolderInfo();
                for (int j = 0; j < newFolder._files.Count; ++j)
                {
                    FileInfo newFile = newFolder._files[j];
                    FileInfo oldFile = oldFolder._files.Find(obj => obj._name == newFile._name);
                    if (oldFile == null || oldFile._md5 != newFile._md5)
                        diffFolder._files.Add(newFile);
                }

                if (diffFolder._files.Count > 0)
                {
                    diffFolder._name = newFolder._name;
                    _diffFolders.Add(diffFolder);
                }
            }

            // 下载差异内容
            for (int i = 0; i < _diffFolders.Count; ++i)
            {
                FolderInfo folderInfo = _diffFolders[i];
                string folderPath = Path.Combine(Application.persistentDataPath, folderInfo._name);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                for (int j = 0; j < folderInfo._files.Count; ++j)
                {
                    FileInfo fileInfo = folderInfo._files[j];
                    string filePath = Path.Combine(folderPath, fileInfo._name);
                    if (File.Exists(filePath))
                        continue;

                    using (WWW www = new WWW(GenRandomURL("")))
                    {
                        yield return www;
                        if (www == null || !string.IsNullOrEmpty(www.error) || !CheckBytesMD5(www.bytes, fileInfo._md5))
                        {
                            LogManager.Error(string.Format("VersionManager::UpdateFiles => can't download [{0}/{1}] from server", folderInfo._name, fileInfo._name));
                            yield break;
                        }

                        File.WriteAllBytes(filePath, www.bytes);
                    }
                }
            }

            // 覆盖版本文件
            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, "version"), newInfoBytes);
        }

        private string GenRandomURL(string file)
        {
            string serverURL = "local/demo/";
            string postfix = "/version?rand=";
            string platform = "Android";

            return string.Concat(serverURL, GenRandomNumbers(6), platform, file, postfix, GenRandomNumbers(6));
        }

        private string GenRandomNumbers(int length)
        {
            char[] numbers = new char[length];
            for (int i = 0; i < length; ++i)
                numbers[i] = UnityEngine.Random.Range(0, 10).ToString()[0];
            return new string(numbers);
        }

        private bool CheckBytesMD5(byte[] bytes, string md5)
        {
            MD5 gen = new MD5CryptoServiceProvider();
            byte[] hashRes = gen.ComputeHash(bytes);

            if (hashRes.Length != md5.Length)
                return false;

            for (int i = 0; i < hashRes.Length; ++i)
            {
                if (Convert.ToString(hashRes[i], 16)[0] != md5[i])
                    return false;
            }
            return true;
        }

        // 清除所有已经更新的资源
        private IEnumerator ClearUpdatedFiles(string updateWwwPath)
        {
            if (!File.Exists(Path.Combine(Application.persistentDataPath, "version")))
                yield break;

            using (WWW www = new WWW(Path.Combine(updateWwwPath, "version")))
            {
                yield return www;
                if (www == null || !string.IsNullOrEmpty(www.error))
                    yield break;

                VersionInfo info = JsonUtility.FromJson<VersionInfo>(www.text);
                if (info == null)
                    yield break;

                for (int i = 0; i < info._folders.Count; ++i)
                {
                    string folder = Path.Combine(Application.persistentDataPath, info._folders[i]._name);
                    if (Directory.Exists(folder))
                        Directory.Delete(folder, true);
                }

                File.Delete(Path.Combine(Application.persistentDataPath, "version"));
            }
        }
    }
}