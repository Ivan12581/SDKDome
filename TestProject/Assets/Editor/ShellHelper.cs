using UnityEngine;
using System.Collections;
using System.Diagnostics;
using UnityEditor;
using System.Collections.Generic;
using System.Text;

public class ShellHelper
{

	public class ShellRequest
	{
		public event System.Action<int, string> onLog;
		public event System.Action onError;
		public event System.Action onDone;

		public void Log(int type, string log)
		{
			if (onLog != null)
			{
				onLog(type, log);
			}
			if (type == 1)
			{
				UnityEngine.Debug.LogError(log);
			}
		}

		public void NotifyDone()
		{
			if (onDone != null)
			{
				onDone();
			}
		}

		public void Error()
		{
			if (onError != null)
			{
				onError();
			}
		}
	}


	private static string shellApp
	{
		get
		{
#if UNITY_EDITOR_WIN
			string app = "cmd.exe";
#elif UNITY_EDITOR_OSX
			string app = "bash";
#endif
			return app;
		}
	}


	private static List<System.Action> _queue = new List<System.Action>();


	static ShellHelper()
	{
		_queue = new List<System.Action>();
		EditorApplication.update += OnUpdate;
	}
	private static void OnUpdate()
	{
		for (int i = 0; i < _queue.Count; i++)
		{
			try
			{
				var action = _queue[i];
				if (action != null)
				{
					action();
				}
			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogException(e);
			}
		}
		_queue.Clear();
	}


	[System.Runtime.InteropServices.DllImport("kernel32.dll")]
	private static extern int GetSystemDefaultLCID();

	public static ShellRequest ProcessCommand(string cmd, string workDirectory, List<string> environmentVars = null)
	{
		ShellRequest req = new ShellRequest();
		System.Threading.ThreadPool.QueueUserWorkItem(delegate (object state) {
			Process p = null;
			try
			{
				ProcessStartInfo start = new ProcessStartInfo(shellApp);

#if UNITY_EDITOR_OSX
				string splitChar = ":";
				start.Arguments = "-c";
				start.Arguments += (" \"" + cmd + " \"");
#elif UNITY_EDITOR_WIN
                string splitChar = ";";
				start.Arguments = "/c" + cmd;
#endif

				if (environmentVars != null)
				{
					foreach (string var in environmentVars)
					{
						start.EnvironmentVariables["PATH"] += (splitChar + var);
					}
				}

				start.CreateNoWindow = true;
				start.ErrorDialog = true;
				start.UseShellExecute = false;
				start.WorkingDirectory = workDirectory;

#if UNITY_EDITOR_OSX
				start.RedirectStandardOutput = false;
				start.RedirectStandardError = false;
				start.RedirectStandardInput = false;
#else
				if (start.UseShellExecute)
				{
					start.RedirectStandardOutput = false;
					start.RedirectStandardError = false;
					start.RedirectStandardInput = false;
				}
				else
				{
					start.RedirectStandardOutput = true;
					start.RedirectStandardError = true;
					start.RedirectStandardInput = true;
					var ci = System.Globalization.CultureInfo.GetCultureInfo(GetSystemDefaultLCID());
					start.StandardOutputEncoding = Encoding.GetEncoding(ci.TextInfo.OEMCodePage);
					start.StandardErrorEncoding = Encoding.GetEncoding(ci.TextInfo.OEMCodePage);
				}
#endif

				p = Process.Start(start);
				p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					UnityEngine.Debug.LogError(e.Data);
				};
				p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e) {
					UnityEngine.Debug.LogError(e.Data);
				};
				p.Exited += delegate (object sender, System.EventArgs e) {
					UnityEngine.Debug.LogError(e.ToString());
				};

				string output = p.StandardOutput.ReadToEnd();
				string error = p.StandardError.ReadToEnd();
				p.WaitForExit();
				p.Close();

				if (!string.IsNullOrEmpty(output))
				{
					_queue.Add(delegate () {
						req.Log(0, EncodingConvert(output, start.StandardErrorEncoding, Encoding.UTF8));
					});
				}
				if (!string.IsNullOrEmpty(error))
				{
					_queue.Add(delegate () {
						req.Log(1, EncodingConvert(error, start.StandardErrorEncoding, Encoding.UTF8));
					});
				}

				if (!string.IsNullOrEmpty(error))
				{
					_queue.Add(delegate () {
						req.Error();
					});
				}
				else
				{
					_queue.Add(delegate () {
						req.NotifyDone();
					});
				}

			}
			catch (System.Exception e)
			{
				UnityEngine.Debug.LogException(e);
				if (p != null)
				{
					p.Close();
				}
			}
		});
		return req;
	}

	private List<string> _enviroumentVars = new List<string>();

	public void AddEnvironmentVars(params string[] vars)
	{
		for (int i = 0; i < vars.Length; i++)
		{
			if (vars[i] == null)
			{
				continue;
			}
			if (string.IsNullOrEmpty(vars[i].Trim()))
			{
				continue;
			}
			_enviroumentVars.Add(vars[i]);
		}
	}

	public ShellRequest ProcessCMD(string cmd, string workDir)
	{
		return ShellHelper.ProcessCommand(cmd, workDir, _enviroumentVars);
	}

	private static string EncodingConvert(string fromString, Encoding fromEncoding, Encoding toEncoding)
	{
		byte[] fromBytes = fromEncoding.GetBytes(fromString);
		byte[] toBytes = Encoding.Convert(fromEncoding, toEncoding, fromBytes);

		string toString = toEncoding.GetString(toBytes);
		return toString;
	}
}