using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Debug = UnityEngine.Debug;

public class BuildWindow : EditorWindow
{
    [MenuItem("Setting/清理缓存")]
    public static void ClaerCache()
    {
        Directory.Delete(Application.persistentDataPath, true);
        PlayerPrefs.DeleteAll();
        //Debug.Log("清理结束");
    }

    [MenuItem("Setting/Build")]
    public static void OpenBuildWindows()
    {
        BuildWindow window = GetWindow<BuildWindow>("打包工具");
        window.minSize = new Vector2(300, 400);
        window.Show();
    }
    [Serializable]
    public class KeystoreConfig
    {
        /// <summary>
        /// 项目包名
        /// </summary>
        public string bundleIdentifier;
        /// <summary>
        /// 密码1
        /// </summary>
        public string keypass;
        /// <summary>
        /// 别名
        /// </summary>
        public string keyaliname;
        /// <summary>
        /// 密码2
        /// </summary>
        public string keyalipass;
        /// <summary>
        /// 路径
        /// </summary>
        public string keystore;
    }

    private KeystoreConfig keystoreCnf;
    private Setting setting;
    private static string SettingPath
    {
        get
        {
            return "editor_config/setting.json";
        }
    }

    private static string KeystorePath
    {
        get
        {
            return "editor_config/keystore_setting.json";
        }
    }

    private static string gitCommitID;
    
    public void Awake()
    {
        ReadSetting();
        InitSysConfig();
        //setting = new Setting();
        //keystoreCnf = new KeystoreConfig();
    }

    public void ReadSetting()
    {
        setting = EditorTools.LoadObjectFromJsonFile<Setting>(SettingPath);
        keystoreCnf = EditorTools.LoadObjectFromJsonFile<KeystoreConfig>(KeystorePath);
    }

    public void InitSysConfig()
    {
        string tmp = keystoreCnf.bundleIdentifier == "" ? PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Standalone) : keystoreCnf.bundleIdentifier;
        keystoreCnf.bundleIdentifier = tmp;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android | BuildTargetGroup.iOS | BuildTargetGroup.Standalone, keystoreCnf.bundleIdentifier);

        if (keystoreCnf.keystore == "")
        {
            tmp = PlayerSettings.Android.keystoreName;
            tmp = tmp.Replace(Environment.CurrentDirectory.Replace("\\", "/"), "");
            keystoreCnf.keystore = tmp;
        }
        else
        {
            PlayerSettings.Android.keystoreName = keystoreCnf.keystore;
        }

        tmp = keystoreCnf.keyaliname == "" ? PlayerSettings.Android.keyaliasName : keystoreCnf.keyaliname;
        PlayerSettings.Android.keyaliasName = keystoreCnf.keyaliname = tmp;

        tmp = keystoreCnf.keyalipass == "" ? PlayerSettings.Android.keyaliasPass : keystoreCnf.keyalipass;
        PlayerSettings.Android.keyaliasPass = keystoreCnf.keyalipass = tmp;
    }

    Vector2 scroll_pos;
    //绘制窗口时调用
    void OnGUI()
    {
        scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos);
        //输入框控件
        GUILayout.Space(10);
        setting.appName = EditorGUILayout.TextField("项目名称:", setting.appName);
        GUILayout.Space(5);
        keystoreCnf.bundleIdentifier = EditorGUILayout.TextField("项目包名:", keystoreCnf.bundleIdentifier);
        GUILayout.Space(5);
        setting.version = EditorGUILayout.TextField("版本号:", setting.version);
        GUILayout.Space(5);
        setting.serverHost = EditorGUILayout.TextField("服务器地址:", setting.serverHost);
        GUILayout.Space(5);
        setting.resouceHost = EditorGUILayout.TextField("热更资源地址:", setting.resouceHost);
        GUILayout.Space(5);
        setting.umengAndroidKey = EditorGUILayout.TextField("友盟Android key:", setting.umengAndroidKey);
        GUILayout.Space(5);
        setting.umengIOSKey = EditorGUILayout.TextField("友盟iOS key:", setting.umengIOSKey);
        GUILayout.Space(5);
        setting.quickProductCode = EditorGUILayout.TextField("Quick ProductCode:", setting.quickProductCode);
        GUILayout.Space(5);
        setting.selfChannelID = EditorGUILayout.TextField("默认渠道ID:", setting.selfChannelID);
        GUILayout.Space(5);
        //setting.weChatAppId = EditorGUILayout.TextField("WeChatAppId:", setting.weChatAppId);
        //GUILayout.Space(5);
        //setting.weChatSecret = EditorGUILayout.TextField("WeChatSecret:", setting.weChatSecret);
        //GUILayout.Space(5);
        setting.isDevMode = EditorGUILayout.Toggle("测试模式", setting.isDevMode);
        GUILayout.Space(5);
        setting.isChannel = EditorGUILayout.Toggle("渠道包", setting.isChannel);
        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("登录方式:", GUILayout.Width(146));
        setting.touristLogin = EditorGUILayout.Toggle("游客登录", setting.touristLogin, GUILayout.Width(220));
        GUILayout.Space(5);
        setting.channelLogin = EditorGUILayout.Toggle("渠道登录", setting.channelLogin, GUILayout.Width(220));
        GUILayout.Space(5);
        setting.weChatLogin = EditorGUILayout.Toggle("微信登录", setting.weChatLogin, GUILayout.Width(220));
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        GUILayout.Space(5);
        GUILayout.Label("keystore", GUILayout.Width(146), GUILayout.Height(18f));
        GUILayout.Label(keystoreCnf.keystore, "HelpBox", GUILayout.Height(18f));
        if (GUILayout.Button(new GUIContent("浏览", "浏览文件夹")))
        {
            string path = EditorUtility.OpenFilePanel("keystore", keystoreCnf.keystore, "keystore");
            keystoreCnf.keystore = path.Replace(System.Environment.CurrentDirectory.Replace("\\", "/"), ".");
            SaveSetting();
            SaveKeystoreSetting();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(5);

        keystoreCnf.keypass = EditorGUILayout.TextField("keypass:", keystoreCnf.keypass);
        GUILayout.Space(5);
        keystoreCnf.keyaliname = EditorGUILayout.TextField("keyaliname:", keystoreCnf.keyaliname);
        GUILayout.Space(5);
        keystoreCnf.keyalipass = EditorGUILayout.TextField("keyalipass:", keystoreCnf.keyalipass);
        GUILayout.Space(5);
        gitCommitID = EditorGUILayout.TextField("git报告提交ID:", gitCommitID);
        GUILayout.Space(5);

        PlayerSettings.productName = setting.appName;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android | BuildTargetGroup.iOS | BuildTargetGroup.Standalone, keystoreCnf.bundleIdentifier);
        PlayerSettings.bundleVersion = setting.version;
        PlayerSettings.Android.keystoreName = keystoreCnf.keystore;
        PlayerSettings.Android.keystorePass = keystoreCnf.keypass;
        PlayerSettings.Android.keyaliasName = keystoreCnf.keyaliname;
        PlayerSettings.Android.keyaliasPass = keystoreCnf.keyalipass;

        GUILayout.Space(5);

        //if (GUILayout.Button("清理C#Warp代码"))
        //{
        //    AssetDatabase.Refresh();
        //}

        //if (GUILayout.Button("生成C#Warp代码"))
        //{
        //    if (EditorApplication.isCompiling)
        //    {
        //        EditorUtility.DisplayDialog("提示", "请等待编译完成后执行!", "OK");
        //    }
        //    else
        //    {
        //        //Generator.GenAll();
        //    }
        //}

        if (GUILayout.Button("一键打AssetBundle"))
        {
            AssetBundleTool.QuickBuildAssetBundle();
            SaveSetting();
        }

        if (GUILayout.Button("快速构建（不打ab）"))
        {
            SaveSetting();
            Build();
        }

        if (GUILayout.Button("导出Gradle工程"))
        {
            SaveSetting();
            ExportGradle();
        }
        if (GUILayout.Button("SaveSetting"))
        {
            SaveSetting();
        }
        EditorGUILayout.EndScrollView();
    }

    private static void ExportGradle()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

        string path = PlayerPrefs.GetString("gradlePath","");
        path = string.IsNullOrEmpty(path)
            ? System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments)
            : path;
        path = EditorUtility.SaveFolderPanel("选择Gradle目录", path, "");
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        PlayerPrefs.SetString("gradlePath", path);
        PlayerPrefs.Save();

        string assetsPath = path + "/src/main/assets".Replace("/",Path.AltDirectorySeparatorChar.ToString());
        string newPath = path;
        bool hasProject = Directory.Exists(assetsPath);
        if (hasProject)
        {
            path.TrimEnd(Path.AltDirectorySeparatorChar);
            newPath = path.Substring(0,path.LastIndexOf(Path.AltDirectorySeparatorChar)+1);
            newPath = newPath + Application.productName + "gradle";
            if (Directory.Exists(newPath))
            {
                Directory.Delete(newPath, true);
            }
        }
        
        Caching.ClearCache();
        var error = BuildPipeline.BuildPlayer(GetBuildScenes(), newPath, BuildTarget.Android, BuildOptions.AcceptExternalModificationsToPlayer);
        //if (!string.IsNullOrEmpty(error))
        //{
        //    Clear();
        //}

        if (hasProject)
        {
            if (Directory.Exists(assetsPath))
            {
                Directory.Delete(assetsPath, true);
            }
            Directory.Move(newPath+Path.AltDirectorySeparatorChar+Application.productName+"/src/main/assets".Replace("/",Path.AltDirectorySeparatorChar.ToString()), assetsPath);
            
            string cmd = path + Path.AltDirectorySeparatorChar+"gradlew";
            ProcessStartInfo info = new ProcessStartInfo(cmd);
            info.WorkingDirectory = path;
            info.Arguments = "assembleRelease";
            info.CreateNoWindow = false;
            info.ErrorDialog = true;
            info.UseShellExecute = true;
		
            if(info.UseShellExecute){
                info.RedirectStandardOutput = false;
                info.RedirectStandardOutput = false;
                info.RedirectStandardError = false;
                info.RedirectStandardInput = false;
            } else{
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.RedirectStandardInput = true;
                info.StandardOutputEncoding = UTF8Encoding.UTF8;
                info.StandardErrorEncoding = UTF8Encoding.UTF8;
            }
		
            Process process = Process.Start(info);
		
            if(!info.UseShellExecute){
                //Debug.Log(process.StandardOutput);
                //Debug.Log(process.StandardError);
            }
            process.WaitForExit();
            process.Close();
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string openPath =
                    Path.GetFullPath(path + "/build/outputs/apk/release/".Replace("/", Path.AltDirectorySeparatorChar.ToString()));
                Process.Start("explorer.exe", openPath);

                if (!string.IsNullOrEmpty(gitCommitID))
                {                    
                    Process pro = new Process();
                    pro.StartInfo.FileName = "cmd";
                    pro.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
                    pro.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
                    pro.StartInfo.RedirectStandardInput = true;  // 重定向输入   
                    pro.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
                    pro.StartInfo.RedirectStandardError = false; //重定向标准错误
                    // 重定向错误输出  
    //            pro.StartInfo.WorkingDirectory = Application.dataPath;  //定义执行的路径 
                    Console.OutputEncoding = Encoding.GetEncoding(936);
                    pro.Start();
                    pro.StandardInput.AutoFlush = true; 
                    string gitlog = "git log --pretty=format:\"%s%n        ========> %an , %ai%n\" {0}^..HEAD --grep \"·新增·\\|·修改·\\|·删除·\\|·其他·\" > {1}.log";
                    gitlog = string.Format(gitlog, gitCommitID.Trim(),openPath+DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                    pro.StandardInput.WriteLine(gitlog); //向cmd中输入命令 
                    pro.StandardInput.WriteLine("exit"); //退出
                    string outRead = pro.StandardOutput.ReadToEnd();  //获得所有标准输出流
                    //Debug.Log(outRead);
                    pro.WaitForExit(); //等待命令执行完成后退出
                    pro.Close(); //关闭窗口
                }
                
            }
        }
        
    }

    private static void Build(bool isGradleExport = false)
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            BuildAndroid();
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64 ||
            EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {
            TryBuildPC();
        }
        else
        {
            EditorUtility.DisplayDialog("tips", "只有安卓、PC可以打包。\n请切一下平台吧!", "^_^");
        }
    }

    void OnFocus()
    {
        ReadSetting();
        InitSysConfig();
    }

    void OnLostFocus()
    {
        SaveSetting();
        SaveKeystoreSetting();
    }

    public void SaveKeystoreSetting()
    {
        //PlayerSettings.productName = setting.appName;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android | BuildTargetGroup.iOS | BuildTargetGroup.Standalone, keystoreCnf.bundleIdentifier);
        //PlayerSettings.bundleVersion = setting.version;

        PlayerSettings.Android.keystoreName = keystoreCnf.keystore;
        PlayerSettings.Android.keystorePass = keystoreCnf.keypass;
        PlayerSettings.Android.keyaliasName = keystoreCnf.keyaliname;
        PlayerSettings.Android.keyaliasPass = keystoreCnf.keyalipass;
        //EditorTools.WriteFileWithCode(KeystorePath, LitJson.JsonMapper.ToJson(keystoreCnf), Encoding.UTF8);
    }

    public void SaveSetting()
    {
        string time = DateTime.Now.ToString();
        string timeFilePath = Application.streamingAssetsPath + "/time.txt";
        File.WriteAllText(timeFilePath, time, Encoding.UTF8);

        string jsonStr = JsonUtility.ToJson(setting, true);
        EditorTools.WriteFileWithCode(SettingPath, jsonStr, null);
        //EnCodeSetting();
        Debug.Log("--SaveSetting--");
    }

    void OnInspectorUpdate()
    {
        //这里开启窗口的重绘，不然窗口信息不会刷新
        Repaint();
    }

    public static void CopyDirectory(string sourcePath, string destinationPath, List<string> list = null)
    {
        sourcePath = sourcePath.Replace("\r", "").Replace("\n", "");
        destinationPath = destinationPath.Replace("\r", "").Replace("\n", "");
        if (!Directory.Exists(sourcePath))
            return;
        DirectoryInfo info = new DirectoryInfo(sourcePath);
        if (list != null)
        {
            var infos = info.GetDirectories();
            for (int i = 0; i < infos.Length; i++)
            {
                list.Add(infos[i].Name);
            }
        }

        if (!Directory.Exists(destinationPath))
            Directory.CreateDirectory(destinationPath);

        foreach (FileSystemInfo fsi in info.GetFileSystemInfos())
        {
            string destName = Path.Combine(destinationPath, fsi.Name);
            if (fsi is FileInfo)
            {
                if (fsi.FullName.IndexOf(".manifest") > 0) continue;
                File.Copy(fsi.FullName, destName, true);
            }
            else
            {
                if (!Directory.Exists(destName))
                    Directory.CreateDirectory(destName);
                CopyDirectory(fsi.FullName, destName);
            }
        }
    }
    //在这里找出你当前工程所有的场景文件，假设你只想把部分的scene文件打包 那么这里可以写你的条件判断 总之返回一个字符串数组。
    static string[] GetBuildScenes()
    {
        List<string> names = new List<string>();
        foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
        {
            if (e == null)
                continue;
            if (e.enabled)
                names.Add(e.path);
        }
        return names.ToArray();
    }

    /// <summary>
    /// 一键打包android
    /// </summary>
    public static void BuildAndroid()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Internal;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;

        DateTime date = DateTime.Now;
        string apkPath = EditorUtility.SaveFilePanel("保存apk", Application.dataPath, string.Format("{0}_{1}", "DDZ", date.ToString("MMdd_HHmm")), "apk");
        if (string.IsNullOrEmpty(apkPath))
        {
            return;
        }

        Caching.ClearCache();

        var run = BuildOptions.None;

        var error = BuildPipeline.BuildPlayer(GetBuildScenes(), apkPath, BuildTarget.Android, run);
        //if (!string.IsNullOrEmpty(error))
        //{
        //    Clear();
        //}
    }

    static void Clear()
    {
        EditorUtility.ClearProgressBar();
    }

    //[MenuItem("Setting/BuildPC")]
    public static void TryBuildPC()
    {
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows64)
        {
            BuildPC();
        }
        else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneWindows)
        {
            if (EditorUtility.DisplayDialog("提示", "目前32位pc\n是否切换64位并且继续打包", "确定", "取消"))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
                BuildPC();
            }
        }
        else
        {
            EditorUtility.DisplayDialog("tips", "目前不是PC平台。\n请切一下平台吧!", "^_^");
        }
    }

    private static void BuildPC()
    {
        string path = EditorUtility.SaveFilePanel("保存", Application.dataPath, "", "exe");

        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        var error = BuildPipeline.BuildPlayer(GetBuildScenes(), path, BuildTarget.StandaloneWindows64, BuildOptions.None);
        //if (!string.IsNullOrEmpty(error))
        //{
        //    Clear();
        //}
    }

    private static void EnCodeSetting()
    {
        //byte[] data = File.ReadAllBytes(SettingPath);
        //data = Setting.EnCode(data);
        //File.WriteAllBytes(Application.streamingAssetsPath + "/setting.json", data);
    }


}

public class Setting {
    private static Setting setting;
    public string appName;
    public string version;
    public string serverHost;
    public string resouceHost;

    //public string weChatAppId;
    //public string weChatSecret;
    public string umengAndroidKey;
    public string umengIOSKey;
    public string quickProductCode;
    public string selfChannelID;
    public bool isDevMode;
    public bool isChannel;
    public bool touristLogin;
    public bool weChatLogin;
    public bool channelLogin;
}
