using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class CreateAB : Editor
{
    [MenuItem("资源管理/AssetBundls/标记")]
    static void CreateAllBundleName()
    {
        string path = "Assets/Res";
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        FileInfo[] fileInfos = directoryInfo.GetFiles();
    }
    //[MenuItem("资源管理/AssetBundls/Build")]
    //static void CreateAllBundleName2()
    //{
    //    BuildPipeline.BuildAssetBundles(Application.dataPath+"");
    //}


    private static string _dirName = "";
    /// <summary>
    /// 批量命名所选文件夹下资源的AssetBundleName.
    /// </summary>
    [MenuItem("Tools/Asset Bundle/Set Asset Bundle Name")]
    [MenuItem("Assets/Asset Bundle/Set Asset Bundle Name")]
    static void SetSelectFolderFileBundleName()
    {
        UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(Object), SelectionMode.Unfiltered);
        foreach (Object item in selObj)
        {
            string objPath = AssetDatabase.GetAssetPath(item);
            DirectoryInfo dirInfo = new DirectoryInfo(objPath);
            if (dirInfo == null)
            {
                Debug.LogError("******请检查，是否选中了非文件夹对象******");
                return;
            }
            _dirName = dirInfo.Name;

            string filePath = dirInfo.FullName.Replace('\\', '/');
            filePath = filePath.Replace(Application.dataPath, "Assets");
            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            ai.assetBundleName = _dirName;

            SetAssetBundleName(dirInfo);
        }
        AssetDatabase.Refresh();
        Debug.Log("******批量设置AssetBundle名称成功******");

    }

    static void SetAssetBundleName(DirectoryInfo dirInfo)
    {
        FileSystemInfo[] files = dirInfo.GetFileSystemInfos();
        foreach (FileSystemInfo file in files)
        {
            if (file is FileInfo && file.Extension != ".meta" && file.Extension != ".txt")
            {
                string filePath = file.FullName.Replace('\\', '/');
                filePath = filePath.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                ai.assetBundleName = _dirName;
            }
            else if (file is DirectoryInfo)
            {
                string filePath = file.FullName.Replace('\\', '/');
                filePath = filePath.Replace(Application.dataPath, "Assets");
                AssetImporter ai = AssetImporter.GetAtPath(filePath);
                ai.assetBundleName = _dirName;
                SetAssetBundleName(file as DirectoryInfo);
            }
        }
    }

    /// <summary>
    /// 批量清空所选文件夹下资源的AssetBundleName.
    /// </summary>
    [MenuItem("Tools/Asset Bundle/Reset Asset Bundle Name")]
    [MenuItem("Assets/Asset Bundle/Reset Asset Bundle Name")]
    static void ResetSelectFolderFileBundleName()
    {
        UnityEngine.Object[] selObj = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Unfiltered);
        foreach (UnityEngine.Object item in selObj)
        {
            string objPath = AssetDatabase.GetAssetPath(item);
            DirectoryInfo dirInfo = new DirectoryInfo(objPath);
            if (dirInfo == null)
            {
                Debug.LogError("******请检查，是否选中了非文件夹对象******");
                return;
            }
            _dirName = null;

            string filePath = dirInfo.FullName.Replace('\\', '/');
            filePath = filePath.Replace(Application.dataPath, "Assets");
            AssetImporter ai = AssetImporter.GetAtPath(filePath);
            ai.assetBundleName = _dirName;

            SetAssetBundleName(dirInfo);
        }
        AssetDatabase.Refresh();
        Debug.Log("******批量清除AssetBundle名称成功******");


    }
}
