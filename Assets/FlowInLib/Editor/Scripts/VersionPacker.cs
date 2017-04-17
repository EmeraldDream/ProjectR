using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Security.Cryptography;

namespace FlowInLib
{
    public class VersionPacker
    {
        // 需要更新的文件夹
        public static string[] _updateFolders = new string[] { "AssetBundles" };

        public static void Pack(string folderName)
        {
            // 创建保存目录
            string savePath = Path.Combine(Path.Combine(Application.dataPath, ".."), folderName);
            if (Directory.Exists(savePath))
                Directory.Delete(savePath, true);

            Directory.CreateDirectory(savePath);

            // 收集文件MD5信息并复制文件到保存目录
            VersionInfo versionInfo = new VersionInfo();
            for (int i = 0; i < _updateFolders.Length; ++i)
            {
                FolderInfo folderInfo = new FolderInfo();
                folderInfo._name = _updateFolders[i];

                // 建立保存目录
                string saveFolderPath = Path.Combine(savePath, folderInfo._name);
                if (Directory.Exists(saveFolderPath))
                    Directory.Delete(saveFolderPath, true);
                Directory.CreateDirectory(saveFolderPath);

                string folderPath = Path.Combine(Application.streamingAssetsPath, folderInfo._name);
                string[] filePaths = Directory.GetFiles(folderPath);
                for (int j = 0; j < filePaths.Length; ++j)
                {
                    string filePath = filePaths[j];
                    if (filePath.EndsWith(".meta") || filePath.EndsWith(".manifest"))
                        continue;

                    FileInfo fileInfo = new FileInfo();
                    fileInfo._name = Path.GetFileName(filePath);

                    // 计算MD5
                    fileInfo._md5 = GenMD5(File.ReadAllBytes(filePath));

                    folderInfo._files.Add(fileInfo);

                    // 复制文件到保存目录
                    File.Copy(filePath, Path.Combine(saveFolderPath, fileInfo._name));
                }

                if (folderInfo._files.Count > 0)
                {
                    versionInfo._folders.Add(folderInfo);
                }
                else
                {
                    Directory.Delete(saveFolderPath, true);
                }
            }

            // 生成 version 和 versionMD5 文件
            File.WriteAllText(Path.Combine(savePath, "version"), JsonUtility.ToJson(versionInfo));
            string versionMD5 = GenMD5(File.ReadAllBytes(Path.Combine(savePath, "version")));
            File.WriteAllText(Path.Combine(savePath, "versionMD5"), versionMD5);
            Debug.Log(folderName + " pack successfully!");
        }

        protected static string GenMD5(byte[] bytes)
        {
            MD5 gen = new MD5CryptoServiceProvider();
            byte[] hashRes = gen.ComputeHash(bytes);
            char[] chars = new char[hashRes.Length];
            for (int k = 0; k < hashRes.Length; ++k)
                chars[k] = Convert.ToString(hashRes[k], 16)[0];
            return new string(chars);
        }
    }
}