using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using UnityEditor;

using UnityEngine;
namespace celia.game.editor
{
    public class GitHandle : Editor
    {
        [MenuItem("Git/test")]
        public static void Run()
        {
            Process pro = new Process();
            pro.StartInfo.FileName = "cmd";
            pro.StartInfo.CreateNoWindow = true;         // 不创建新窗口    
            pro.StartInfo.UseShellExecute = false;       //不启用shell启动进程  
            pro.StartInfo.RedirectStandardInput = true;  // 重定向输入   
            pro.StartInfo.RedirectStandardOutput = true; // 重定向标准输出    
            pro.StartInfo.RedirectStandardError = false; //重定向标准错误
            pro.Start();
            pro.StandardInput.AutoFlush = true;
            string gitlog = "git log --pretty=format:\"%s%n        ========> %an , %ai%n\" {0}^..HEAD --grep \"·新增·\\|·修改·\\|·删除·\\|·其他·\" > {1}.log";
            gitlog = string.Format(gitlog, DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            pro.StandardInput.WriteLine(gitlog); //向cmd中输入命令 
            pro.StandardInput.WriteLine("exit"); //退出
            string outRead = pro.StandardOutput.ReadToEnd();  //获得所有标准输出流
            UnityEngine.Debug.Log(outRead);
            pro.WaitForExit(); //等待命令执行完成后退出
            pro.Close(); //关闭窗口
        }
    }
}

