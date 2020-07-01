using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace celia.game.editor
{
    public class CeliaBuilder
    {
        #region 打包菜单
        [MenuItem("Tools/Build/Android/Alpha版APK+无SDK  [所有打包项，服务器随项目当前设置]")]
        public static void BuildAndroidPreview()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Alpha", "Sign:Rastar" });
        }
        [MenuItem("Tools/Build/Android/Beta版APK+无SDK")]
        public static void BuildAndroidBeta()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Beta", "Sign:Rastar" });
        }

        [MenuItem("Tools/Build/Android/Alpha版APK+星辉SDK")]
        public static void BuildAndroidNative()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Alpha", "Sign:Rastar", "SDK:Native", "SDKParams:And_Rastar" });
        }
        [MenuItem("Tools/Build/Android/Alpha版APK+矗凯SDK")]
        public static void BuildAndroidChukai()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Alpha", "Sign:Rastar", "SDK:NativeChukai", "SDKParams:And_Chukai1" });
        }

        [MenuItem("Tools/Build/Android/Alpha版APK+海外SDK")]
        public static void BuildAndroidOversea_Alpha()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Alpha", "Sign:Guanghuan", "SDK:Oversea", "SDKParams:And_Guanghuan1" });
        }

        [MenuItem("Tools/Build/Android/Beta版APK+海外SDK")]
        public static void BuildAndroidOversea_Beta()
        {
            StartBuild(new string[] { "Platform:Android", "Level:Beta", "Sign:Guanghuan", "SDK:Oversea", "SDKParams:And_Guanghuan1" });
        }

        [MenuItem("Tools/Build/iOS/Alpha版Xcode工程")]
        public static void BuildiOSDevelopment()
        {
            StartBuild(new string[] { "Platform:IOS", "Level:Alpha", "Sign:RastarInHouse" , "Path:/Users/mini/Documents/SDKXCodeProj" });
        }
        #endregion


        static CeliaBuildOption buildOption;
        public static void StartBuild(string[] args = null)
        {
            //if (GenerateBuildOtions(args))
            //{
            //    DoPreExcute(ActionLevel.Common);

            //    DoPreExcute(ActionLevel.Platform);

            //    BuildPlayer();

            //    DoPostExcute(ActionLevel.Platform);

            //    DoPostExcute(ActionLevel.Common);
            //}
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
        #endregion
        
        public static void BuildPlayer()
        {
            BuildReport report = BuildPipeline.BuildPlayer(buildOption.PlayerOption);

            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Building successed, the total size is" + summary.totalSize + " bytes");
                Debug.Log("Building time is" + summary.totalTime);
            }
            else
            {
                Debug.Log($"Building {summary.result}!");
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
