using System;
using System.IO;

using UnityEditor;
using UnityEditor.Build.Reporting;

using UnityEngine;

namespace celia.game.editor
{
    public class CeliaBuilder
    {
        private static RuntimePlatform runtimePlatform = RuntimePlatform.XboxOne;

        public static void GetCurPlatform()
        {
#if UNITY_ANDROID
            runtimePlatform = RuntimePlatform.Android;
            return;
#endif
#if UNITY_IOS
        runtimePlatform = RuntimePlatform.IPhonePlayer;
        return;
#endif
            runtimePlatform = RuntimePlatform.XboxOne;
            Log.Info_red("目前不支持打PC包 请注意平台信息");
        }

        #region 打包菜单

        [MenuItem("Tools/Build/NoneSDK")]
        public static void BuildNoneSDK()
        {
            GetCurPlatform();
            if (runtimePlatform == RuntimePlatform.Android)
            {
                StartBuild(new string[] { "Platform:Android", "Level:Beta", "Sign:Rastar", "SDK:None" });
            }
            else if (runtimePlatform == RuntimePlatform.IPhonePlayer)
            {
                StartBuild(new string[] { "Platform:IOS", "Level:Beta", "Sign:CeliaAdhoc", "SDK:None" });
            }
        }

        [MenuItem("Tools/Build/RastarSDK-->>101714")]
        public static void BuildRastarSDK101714()
        {
            GetCurPlatform();
            if (runtimePlatform == RuntimePlatform.Android)
            {
                StartBuild(new string[] { "Platform:Android", "Level:Beta", "Sign:Rastar", "SDK:Native", "SDKParams:And_Rastar" });
            }
            else if (runtimePlatform == RuntimePlatform.IPhonePlayer)
            {
                StartBuild(new string[] { "Platform:IOS", "Level:Beta", "Sign:RastarAhoc", "SDK:Native", "SDKParams:IOS_Rastar" });
            }
        }

        [MenuItem("Tools/Build/CeliaSDK")]
        public static void BuildCeliaSDK()
        {
            GetCurPlatform();
            if (runtimePlatform == RuntimePlatform.Android)
            {
                StartBuild(new string[] { "Platform:Android", "Level:Beta", "Sign:zmxt", "SDK:CeliaOversea", "SDKParams:And_Celia", "CompanyName:EMG TECHNOLOGY LIMITED" });
            }
            else if (runtimePlatform == RuntimePlatform.IPhonePlayer)
            {
                StartBuild(new string[] { "Platform:IOS", "Level:Beta", "Sign:CeliaAdhoc", "SDK:CeliaOversea", "SDKParams:IOS_Celia", "CompanyName:EMG TECHNOLOGY LIMITED" });
            }
        }

        #endregion 打包菜单

        public static CeliaBuildOption buildOption;

        public static void StartBuild(string[] args = null)
        {
            if (GenerateBuildOtions(args))
            {
                DoPreExcute(ActionLevel.Common);

                DoPreExcute(ActionLevel.Platform);

                BuildPlayer();

                DoPostExcute(ActionLevel.Platform);

                DoPostExcute(ActionLevel.Common);

            }
        }

        /// <summary>
        /// 从args读取生成打包配置
        /// </summary>
        public static bool GenerateBuildOtions(string[] args = null)
        {
            buildOption = new CeliaBuildOption();
            buildOption.PlayerOption = new BuildPlayerOptions();

            buildOption.Args = args == null ? Environment.GetCommandLineArgs() : args;

            string platformName = GetInputParam("Platform:", buildOption.Args);
            buildOption.ProcessCfg = AssetDatabase.LoadAssetAtPath<ProcessCfg>($"Assets/Editor/CeliaBuilder/{platformName}.asset");

            string projectValue = GetInputParam("Project:", buildOption.Args);
            bool.TryParse(projectValue, out buildOption.OutputProject);

            string levelName = GetInputParam("Level:", buildOption.Args);
            Enum.TryParse(levelName, out buildOption.ReleaseLevel);

            string sdkName = GetInputParam("SDK:", buildOption.Args);
            Enum.TryParse(sdkName, out buildOption.SDKType);

            Console.WriteLine($"Target:{buildOption.ProcessCfg.Target} Project:{buildOption.OutputProject} Release:{buildOption.ReleaseLevel}");
            return true;
        }

        /// <summary>
        /// 从args读取配置的参数，如，SDK:1得1，默认返回空字符
        /// </summary>
        public static string GetInputParam(string prefix, string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                string param = args[i];
                if (param.Contains(prefix))
                {
                    return param.Replace(prefix, "");
                }
            }
            return "";
        }

        public static string ApkName { set; get; }

        public static void GetPakageName()
        {
            string IP = GameSetting.gi.ip.Replace(".", "-");
            ApkName = $"{DateTime.Now:MMdd_HHmm}_{buildOption.SDKType}_{GameSetting.gi.VERSION}_{IP}";
        }

        #region Excute

        public static void DoPreExcute(ActionLevel actionLevel)
        {
            ProcessCfg processCfg = buildOption.ProcessCfg;
            BuildAction[] actions;
            if (actionLevel == ActionLevel.Common)
            {
                actions = processCfg.CommonActions;
            }
            else
            {
                actions = processCfg.PlatformActions;
            }
            for (int i = 0; i < actions.Length; i++)
            {
                actions[i].PreExcute(buildOption);
            }
        }

        public static void DoPostExcute(ActionLevel actionLevel)
        {
            ProcessCfg processCfg = buildOption.ProcessCfg;
            BuildAction[] actions;
            if (actionLevel == ActionLevel.Common)
            {
                actions = processCfg.CommonActions;
            }
            else
            {
                actions = processCfg.PlatformActions;
            }
            for (int i = actions.Length - 1; i >= 0; i--)
            {
                actions[i].PostExcute(buildOption);
            }
        }

        #endregion Excute

        public static void BuildPlayer()
        {
            BuildReport report = BuildPipeline.BuildPlayer(buildOption.PlayerOption);

            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Log.Info_green("Building successed, the total size is" + summary.totalSize / 1000000 + " bytes" + " and  the totalTime is " + summary.totalTime);
            }
            else
            {
                Log.Info_yellow($"Building {summary.result}!");
            }
            //BuildStep[] buildSteps = report.steps;
            //foreach (var item in buildSteps)
            //{
            //    Log.Info_blue("name:" + item.name + "   depth:" + item.depth + "  duration:" + item.duration);
            //    foreach (var item1 in item.messages)
            //    {
            //        if (item1.type != LogType.Warning)
            //        {
            //            Log.Info_blue("messages type:" + item1.type + "  content type:" + item1.content);
            //        }
            //    }
            //}
            if (buildOption.PlayerOption.target == BuildTarget.Android && PlayerSettings.Android.useAPKExpansionFiles)
            {
                ChangeObbName();
            }
        }

        private static void ChangeObbName()
        {
            string apkParentDir = Directory.GetParent(buildOption.PlayerOption.locationPathName).FullName;
            apkParentDir = apkParentDir.Replace(@"\", "/");
            string obbName_old = ApkName + ".main.obb";
            string sourceName = Path.Combine(apkParentDir, obbName_old);
            if (File.Exists(sourceName))
            {
                string obbName_new = "main." + PlayerSettings.Android.bundleVersionCode + "." + Application.identifier + ".obb";
                string destName = Path.Combine(apkParentDir, obbName_new);
                if (File.Exists(destName))
                {
                    File.Delete(destName);
                }
                File.Move(sourceName, destName);
                Log.Info_green("Obb Change Name:" + obbName_old + " To " + obbName_new);
            }
        }
    }

    /// <summary>
    /// 全局配置信息
    /// </summary>
    public class CeliaBuildOption
    {
        public BuildPlayerOptions PlayerOption;

        public string[] Args;
        public ProcessCfg ProcessCfg;
        public ReleaseLevel ReleaseLevel;
        public bool OutputProject;
        public SDKType SDKType;
    }

    public enum ReleaseLevel
    {
        Alpha,
        Beta
    }

    public enum ActionLevel
    {
        Common,
        Platform,
    }

    public enum IOSSignType
    {
        Ad,
        Dis
    }
}