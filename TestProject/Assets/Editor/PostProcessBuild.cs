using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

using UnityEngine;
namespace celia.game.editor
{
    public class PostProcessBuild
    {
        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuildProject)
        {
            Debug.Log("---target--->" + target + "---pathToBuildProject--->" + pathToBuildProject);
            if (target == BuildTarget.iOS)
            {
                string projPath = pathToBuildProject + "/Unity-iPhone.xcodeproj/project.pbxproj";
                PBXProject _pbxProj = new PBXProject();//创建xcode project类
                IosSDKSetting.IOSXcodeSettings(pathToBuildProject);
            }
            if (target == BuildTarget.Android)
            {

            }
        }
    }
}

