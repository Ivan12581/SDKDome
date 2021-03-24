using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
//using Wedo.Client.Network;
//using Wedo.Client.Framework;
using System.Linq;

public class AssetBundleTool
{

    public static string AssetBundle_Output_Path = "StreamingAssets";

    static string GetBuildTargetOutputPath(BuildTarget target)
    {
        if (target == BuildTarget.Android)
            return AssetBundle_Output_Path + "/Android";

        if (target == BuildTarget.iOS)
            return AssetBundle_Output_Path + "/iOS";

        return AssetBundle_Output_Path + "/Win";
    }

    //[MenuItem("AssetBundle/Build/Current")]
    public static void Build_Current()
    {
#if UNITY_ANDROID
        BuildAllAssetBundles(BuildTarget.Android);
#elif UNITY_IOS
        BuildAllAssetBundles(BuildTarget.iOS);
#else
        BuildAllAssetBundles(BuildTarget.StandaloneWindows);
#endif
    }

    //[MenuItem("AssetBundle/Build/Android")]
    public static void Build_Android()
    {
        BuildAllAssetBundles(BuildTarget.Android);
    }

    //[MenuItem("AssetBundle/Build/iOS")]
    public static void Build_iOS()
    {
        BuildAllAssetBundles(BuildTarget.iOS);
    }

    //[MenuItem("AssetBundle/Build/Windows")]
    public static void Build_Win()
    {
        BuildAllAssetBundles(BuildTarget.StandaloneWindows);
    }

    //[MenuItem("AssetBundle/Streaming/Clear")]
    public static void Clear_StreamingDir()
    {
        string streamingDir = Application.streamingAssetsPath;
        Clear_TargetDir(streamingDir);
    }

    public static void QuickBuildAssetBundle()
    {
        //Build
        Build_Current();
        //Export
        //Export_Current();
        //Streaming
        Copy_AB_To_StreamingDir_Current();
    }


    static void Clear_TargetDir(string dir)
    {
        try
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
            }
            Directory.CreateDirectory(dir);
            Debug.Log(string.Format("Clear_TargetDir> success! path = {0}", dir));
        }
        catch (Exception ex)
        {
            Debug.LogError(string.Format("Clear_TargetDir> failed! path = {0}", dir));
            throw ex;
        }
    }

    public static void Copy_AB_To_StreamingDir_Current()
    {
#if UNITY_ANDROID
        Copy_AB_To_StreamingDir(BuildTarget.Android);
#elif UNITY_IOS
        Copy_AB_To_StreamingDir(BuildTarget.iOS);
#else
        Copy_AB_To_StreamingDir(BuildTarget.StandaloneWindows);
#endif
    }


    //[MenuItem("AssetBundle/Streaming/Copy AB for Android")]
    public static void Copy_AB_To_StreamingDir_Android()
    {
        Copy_AB_To_StreamingDir(BuildTarget.Android);
    }

    //[MenuItem("AssetBundle/Streaming/Copy AB for Windows")]
    public static void Copy_AB_To_StreamingDir_Windows()
    {
        Copy_AB_To_StreamingDir(BuildTarget.StandaloneWindows64);
    }

    public static void Copy_AB_To_StreamingDir(BuildTarget target)
    {
        var output_path = GetBuildTargetOutputPath(target);
        var export_path = output_path + "/Export";
        if (!Directory.Exists(export_path))
        {
            Debug.LogError(export_path + " is not exist");
            return;
        }

        Clear_StreamingDir();
        string streamingDir = Application.streamingAssetsPath;

        foreach (var path in Directory.GetFiles(export_path, "*.*", SearchOption.AllDirectories))
        {
            Debug.Log(path);
            string newPath = path.Replace(export_path, streamingDir);
            Debug.Log(newPath);
            string dir = Path.GetDirectoryName(newPath);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.Copy(path, newPath);
        }
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 自动打包所有资源（设置了Assetbundle Name的资源）
    /// </summary>
    public static void BuildAllAssetBundles(BuildTarget target)
    {
        //StringBuilder sb = new StringBuilder();
        //string[] abNames = AssetDatabase.GetAllAssetBundleNames();
        //foreach (var abName in abNames)
        //{
        //    var abNameNoHashPostfix = abName.EndsWith(AssetConfig.Bundle_Postfix) ? abName.Substring(0, abName.Length - AssetConfig.Bundle_Postfix.Length) : abName;
        //    var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);

        //    if (assetPaths != null && assetPaths.Length > 0 && !abName.EndsWith(AssetConfig.Bundle_Postfix))
        //    {
        //        throw new Exception("No .bundle postfix for AssetBundle " + abName);
        //    }

        //    if (!abNameNoHashPostfix.Equals(AssetConfig.AssetBundle_Build_List_Name) && assetPaths != null && assetPaths.Length > 0)
        //    {
        //        sb.Append(abNameNoHashPostfix).Append("\n");
        //        foreach (var assetPath in assetPaths)
        //        {
        //            sb.Append("\t").Append(assetPath).Append("\n");
        //        }
        //    }
        //}
        //var dir = Path.GetDirectoryName(AssetConfig.AssetBundle_Build_List_Path);
        //if (!Directory.Exists(dir))
        //    Directory.CreateDirectory(dir);
        //File.WriteAllText(AssetConfig.AssetBundle_Build_List_Path, sb.ToString());

        //AssetDatabase.Refresh();

        //var importer = AssetImporter.GetAtPath(AssetConfig.AssetBundle_Build_List_Path);
        //importer.SetAssetBundleNameAndVariant(AssetConfig.AssetBundle_Build_List_Name + AssetConfig.Bundle_Postfix, string.Empty);

        var output_path = GetBuildTargetOutputPath(target);
        Clear_TargetDir(output_path);

        Caching.ClearCache();

        var manifest = BuildPipeline.BuildAssetBundles(output_path,
            BuildAssetBundleOptions.AppendHashToAssetBundleName |
            BuildAssetBundleOptions.None |
            BuildAssetBundleOptions.DeterministicAssetBundle |
            BuildAssetBundleOptions.StrictMode, target);

        //sb.Length = 0;

        string[] abs = manifest.GetAllAssetBundles();
        foreach (var ab in abs)
        {
            var hash = manifest.GetAssetBundleHash(ab).ToString();
            //var ab_no_hash = ab.Replace(AssetConfig.Bundle_Postfix + "_" + hash, string.Empty);
            var len = new FileInfo(output_path + "/" + ab).Length;
            //sb.Append(ab_no_hash).Append("|").Append(hash).Append("|").Append(len).Append("\n");
        }

        var manifestName = Path.GetFileName(output_path);

        //var md5 = AssetsUtil.md5(sb.ToString());
        //var newFile = output_path + "/" + AssetConfig.AssetBundleManifest_Name + AssetConfig.Bundle_Postfix + "_" + md5;
        //if (File.Exists(newFile))
        //    File.Delete(newFile);
        //File.Move(output_path + "/" + manifestName, newFile);

        //var manifest_len = new FileInfo(newFile).Length;

#if HALL_PROJECT
        var config = AssetDatabase.LoadAssetAtPath<DataConfig>(BuildTool.configPath);
        var res_version = config.AssetBundle_Version;
#else
        //var res_version = AssetConfig.Version;
#endif

        var new_sb = new StringBuilder();
        TimeSpan timeSpan = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        new_sb.Append(Application.version)
            .Append("#")
            //.Append(res_version)
            //.Append("#")
            .Append(abs.Length + 1)
            .Append("#")
            .Append(Convert.ToInt64(timeSpan.TotalMilliseconds))
            .Append("\n");
        //new_sb.Append(sb);

        //new_sb.Append(AssetConfig.AssetBundleManifest_Name).Append("|").Append(md5).Append("|").Append(manifest_len).Append("\n");
        //File.WriteAllText(output_path + "/" + AssetConfig.File_List_Name, new_sb.ToString());

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Debug.Log(string.Format("BuildAllAssetBundles> done! targetDir:{0}", output_path));
        //}

        //[MenuItem("AssetBundle/Export/Current")]
        //static void Export_Current()
//        {
//#if UNITY_ANDROID
//            //Export(BuildTarget.Android);
//#elif UNITY_IOS
//        //Export(BuildTarget.iOS);
//#else
//        //Export(BuildTarget.StandaloneWindows);
//#endif
//        }

        //[MenuItem("AssetBundle/Export/Android")]
        //static void Export_Android()
        //{
        //    //Export(BuildTarget.Android);
        //}

        //[MenuItem("AssetBundle/Export/iOS")]
        //static void Export_iOS()
        //{
        //    //Export(BuildTarget.iOS);
        //}

        //[MenuItem("AssetBundle/Export/Win")]
        //static void Export_Win()
        //{
        //    //Export(BuildTarget.StandaloneWindows);
        //}

        //static void Export(BuildTarget target)
        //{
        //    //var output_path = GetBuildTargetOutputPath(target);
        //    //if (!Directory.Exists(output_path))
        //    //{
        //    //    Debug.LogError(output_path + " is not exist");
        //    //    return;
        //    //}

        //    //var export_path = output_path + "/Export";
        //    //Export_To_Path(output_path, export_path);
        }

        //public static void Export_To_Path(string output_path, string export_path)
        //{
        //    if (!Directory.Exists(output_path))
        //    {
        //        Debug.LogError(output_path + " is not exist");
        //        return;
        //    }

        //    //var fileListPath = output_path + "/" + AssetConfig.File_List_Name;
        //    if (!File.Exists(fileListPath))
        //    {
        //        Debug.LogError(fileListPath + " is not exist");
        //        return;
        //    }

        //    if (Directory.Exists(export_path))
        //        Directory.Delete(export_path, true);
        //    Directory.CreateDirectory(export_path);

        //    var lines = File.ReadAllLines(fileListPath);
        //    foreach (var line in lines)
        //    {
        //        if (string.IsNullOrEmpty(line) || line.Contains("#"))
        //            continue;

        //        var strs = line.Split('|');
        //        //var file_name = strs[0] + AssetConfig.Bundle_Postfix + "_" + strs[1];
        //        //var file_path = output_path + "/" + file_name;
        //        if (!File.Exists(file_path))
        //        {
        //            Debug.LogError(file_path + " is not exist");
        //            return;
        //        }
        //        //var new_file_path = export_path + "/" + file_name;
        //        var new_dir = Path.GetDirectoryName(new_file_path);
        //        if (!Directory.Exists(new_dir))
        //            Directory.CreateDirectory(new_dir);
        //        File.Copy(file_path, new_file_path);
        //    }
        //    //File.Copy(fileListPath, export_path + "/" + AssetConfig.File_List_Name);

        //    Debug.Log(string.Format("Export_To_Path> done. targetDir;{0}", export_path));
        //}

        [MenuItem("Setting/预处理/Clear AssetBundle Names")]
        static void ClearNames()
        {
            EditorUtility.DisplayProgressBar("Clear-Names", "Scanning...", 0f);

            string[] allPath = AssetDatabase.GetAllAssetPaths();
            for (int i = 0; i < allPath.Length; ++i)
            {
                string file = allPath[i];
                if (!file.EndsWith(".js") && !file.EndsWith(".cs"))
                {
                    if (file.IndexOf("Assets/", StringComparison.Ordinal) < 0)
                        continue;

                    file = file.Substring(file.IndexOf("Assets/", StringComparison.Ordinal));
                    AssetImporter importer = AssetImporter.GetAtPath(file);
                    if (importer != null)
                    {
                        importer.assetBundleName = "";
                    }

                    float progress = (float)i / allPath.Length;
                    EditorUtility.DisplayProgressBar("Clear-Names", "Scanning..." + progress * 100 + "%", progress);
                }
            }

            AssetDatabase.Refresh();

            EditorUtility.ClearProgressBar();
        }

        [MenuItem("Setting/预处理/检查Lua脚本Label(已改为自动添加，可废弃)")]
        static void GenBundleNames()
        {
            EditorUtility.DisplayProgressBar("Gen-Names", "Scanning...", 0f);

            //SetDirBundleNameAndLabel(AssetConfig.Lua_Src_Paths, ".lua", "", "LuaScript");

            AssetDatabase.Refresh();
            EditorUtility.ClearProgressBar();
        }

        static void SetDirBundleNameAndLabel(string[] dirs, string ext, string bundleName = "", string label = "")
        {
            var guids = new List<string>(AssetDatabase.FindAssets("", dirs));
            Debug.Log(string.Format("in target dirs> count:{0}", guids.Count));
            var pathList = guids.Select((guid) => { return AssetDatabase.GUIDToAssetPath(guid); });
            var luaList = pathList.Where((path) => { return path.EndsWith(ext); }).ToList();
            int i = 0;
            string[] labels = new string[] { label };
            luaList.ForEach((path) =>
            {
                string innerPath = path.Substring(path.IndexOf("Assets/", StringComparison.Ordinal));
                var obj = AssetDatabase.LoadMainAssetAtPath(innerPath);
                AssetImporter importer = AssetImporter.GetAtPath(innerPath);
                if (importer != null)
                {
                    importer.assetBundleName = bundleName;
                }
                if (obj != null)
                {
                    AssetDatabase.ClearLabels(obj);
                    AssetDatabase.SetLabels(obj, labels);
                }

                float progress = (float)i++ / luaList.Count;
                EditorUtility.DisplayProgressBar("SetDirBundleName", "Scanning..." + progress * 100 + "%", progress);
            });
        }

}
