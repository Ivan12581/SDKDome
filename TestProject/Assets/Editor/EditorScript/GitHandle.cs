using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

using UnityEditor;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace celia.game.editor
{
    public class GitHandle : Editor
    {
		const string SVNTortoise = "TortoiseProc.exe";
		const string GitTortoise = "TortoiseGitProc.exe";
		const string Tortoise = GitTortoise;
		[MenuItem("SVN/提交", false, 1)]
        static void SVNCommit()
        {
            List<string> pathList = new List<string>();
            string basePath = SVNProjectPath + "/Assets";
            pathList.Add(basePath);
            pathList.Add(SVNProjectPath + "/ProjectSettings");

            string commitPath = string.Join("*", pathList.ToArray());
            ProcessCommand(Tortoise, "/command:commit /path:" + commitPath);
        }

		[MenuItem("SVN/更新", false, 2)]
		static void SVNUpdate()
		{
			ProcessCommand(Tortoise, "/command:update /path:" + SVNProjectPath + " /closeonend:0");
		}

		[MenuItem("SVN/", false, 3)]
		static void Breaker() { }

		[MenuItem("SVN/清理缓存", false, 4)]
		static void SVNCleanUp()
		{
			ProcessCommand(Tortoise, "/command:cleanup /path:" + SVNProjectPath);
		}

		[MenuItem("SVN/打开日志", false, 5)]
		static void SVNLog()
		{
			ProcessCommand(Tortoise, "/command:log /path:" + SVNProjectPath);
		}
		[MenuItem("SVN/Test", false, 6)]
		static void SVNTest()
		{
			//ProcessCommand("git-cmd.exe", "/command:log /path:" + SVNProjectPath);

			string gitExePath = @"E:/WorkSoft/Git/Git/git-cmd.exe";
			Debug.Log("Path.AltDirectorySeparatorChar.ToString():" + Path.AltDirectorySeparatorChar.ToString());
			if (!File.Exists(gitExePath))
            {
				Debug.LogError("--!File.Exists(gitExePath)--");
				return;
            }
			ProcessStartInfo info = new ProcessStartInfo(gitExePath);
			info.Arguments = "/command:log /path:" + SVNProjectPath;
			info.CreateNoWindow = true;
			info.ErrorDialog = true;
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			info.RedirectStandardInput = true;
			info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
			info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;

			Process process = Process.Start(info);
			process.WaitForExit(1000);
			using (StreamReader sr = process.StandardOutput)
			{

				String str = sr.ReadLine();

				while (null != str)
				{

					Console.WriteLine(str);

					str = sr.ReadLine();

				}

			}

			if (process.HasExited)
				process.Close();
		}
		static string SVNProjectPath
		{
			get
			{
				System.IO.DirectoryInfo parent = System.IO.Directory.GetParent(Application.dataPath);
				return parent.ToString();
			}
		}
		public static void ProcessCommand(string command, string argument)
		{
			System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(command);
			info.Arguments = argument;
			info.CreateNoWindow = false;
			info.ErrorDialog = true;
			info.UseShellExecute = true;

			if (info.UseShellExecute)
			{
				info.RedirectStandardOutput = false;
				info.RedirectStandardError = false;
				info.RedirectStandardInput = false;
			}
			else
			{
				info.RedirectStandardOutput = true;
				info.RedirectStandardError = true;
				info.RedirectStandardInput = true;
				info.StandardOutputEncoding = System.Text.UTF8Encoding.UTF8;
				info.StandardErrorEncoding = System.Text.UTF8Encoding.UTF8;
			}

			System.Diagnostics.Process process = System.Diagnostics.Process.Start(info);

			if (!info.UseShellExecute)
			{
				Debug.Log(process.StandardOutput);
				Debug.Log(process.StandardError);
			}

			process.WaitForExit();
			process.Close();
		}
	}
}

