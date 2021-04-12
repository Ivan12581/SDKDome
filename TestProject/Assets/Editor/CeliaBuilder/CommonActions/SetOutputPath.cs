using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace celia.game.editor
{
    public class SetOutputPath : CommonAction
    {
        public override void PreExcute(CeliaBuildOption option)
        {
            string outputPath = CeliaBuilder.GetInputParam("Path:", option.Args);

            EditorUserBuildSettings.exportAsGoogleAndroidProject = option.OutputProject;
            CeliaBuilder.GetPakageName();
            // iOS不能成包
            if (option.OutputProject || option.ProcessCfg.Target == UnityEditor.BuildTarget.iOS)
            {

                if (!string.IsNullOrEmpty(outputPath))
                {
                    option.PlayerOption.locationPathName = outputPath;
                }
                else
                {
                    option.PlayerOption.locationPathName = $"Outputs/{option.ProcessCfg.Target}/{CeliaBuilder.ApkName}_Project";
                }
            }
            else
            {
                string suffix = option.ProcessCfg.Target == UnityEditor.BuildTarget.Android ? ".apk" : "";
                if (!string.IsNullOrEmpty(outputPath))
                {
                    option.PlayerOption.locationPathName = $"{outputPath}{suffix}";
                }
                else
                {
                    option.PlayerOption.locationPathName = $"Outputs/{option.ProcessCfg.Target}/{CeliaBuilder.ApkName}{suffix}";
                }
            }

            //Debug.Log("SetOutputPath PreExcuted!");
        }
        public override void PostExcute(CeliaBuildOption option)
        {
            Debug.Log("SetOutputPath PostExcute!");
        }

        //private string GetPakageName(CeliaBuildOption option)
        //{
        //    string IP = GameSetting.gi.ip.Replace(".", "-");
        //    return $"{DateTime.Now:MMdd_HHmm}_{option.SDKType}_{GameSetting.gi.VERSION}_{IP}";
        //}

        public static void DeleteFolder(string deletePath)
        {
            DirectoryInfo folder = new DirectoryInfo(deletePath);
            if (!folder.Exists)
            {
                return;
            }
            FileSystemInfo[] fileinfo = folder.GetFileSystemInfos();
            foreach (FileSystemInfo info in fileinfo)
            {
                if (info is DirectoryInfo)
                {
                    DeleteFolder(info.FullName);
                } 
                else
                {
                    info.Delete();
                }
            } 
            folder.Delete();
        }
    }
}