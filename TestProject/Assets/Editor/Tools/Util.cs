using System;
using System.Collections;
using System.IO;

using UnityEditor;

using UnityEngine;

public class Util :Singleton<Util>
{
    /// <summary>
    /// 复制文件夹
    /// </summary>
    /// <param name="sourcePath">原文件目录</param>
    /// <param name="targetPath">目标文件目录</param>
    /// <param name="inculdemeta">是否需要拷贝文件的meta</param>
    public void CopyFolder(string sourcePath, string targetPath,bool inculdemeta = false)
    {
        DirectoryInfo sourceFolder = new DirectoryInfo(sourcePath);
        if (!sourceFolder.Exists)
        {
            Debug.Log(sourcePath);
            return;
        }

        DirectoryInfo targetFolder = new DirectoryInfo(targetPath);
        if (!targetFolder.Exists)
        {
            Directory.CreateDirectory(targetPath);
        }

        foreach (var file in sourceFolder.GetFiles())
        {
            if (file.Extension.CompareTo(".meta") == 0)
            {
                if (!inculdemeta)
                {
                    continue;
                }
            }

            string fileTargetPath = Path.Combine(targetPath, Path.GetFileName(file.Name));
            File.Copy(file.FullName, fileTargetPath, true);
        }
        foreach (var folder in sourceFolder.GetDirectories())
        {
            string subfolderTargetPath = Path.Combine(targetPath, Path.GetFileName(folder.Name));
            CopyFolder(folder.FullName, subfolderTargetPath);
        }
        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 获取时间戳
    /// </summary>
    /// <returns></returns>
    public string GetTimeStamp()
    {
        TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalSeconds).ToString();
    }
}
