using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

//using StackifyHttpTracer;

using UnityEditor;

using UnityEngine;
namespace celia.game.editor
{
    public class ConfigSetting : Editor
    {
        static string streamPath = Application.streamingAssetsPath + "/Config";
        static string configPath_jianti = "/Config/Config_jianti";
        static string configPath_fanti = "/Config/Config_fanti";

        [MenuItem("Tool/test2测试文件改名")]
        public static void test2()
        {
            string path = "0409_1613_CeliaOversea_Devlopment_thethroneofgirl - game - smartplay - com - tw.main.obb";
            path = path.Replace(" ", "");
            string ApkName = "0409_1631_CeliaOversea_Devlopment_thethroneofgirl-game-smartplay-com-tw";
            string Dir = $"D:/SDKDomeProject/TestProject/Outputs/Android";


            string sourceName = $"D:/SDKDomeProject/TestProject/Outputs/Android/0409_1707_CeliaOversea_Devlopment_thethroneofgirl-game-smartplay-com-tw.main.obb";
            if (File.Exists(sourceName))
            {
                string obbName = "main." + PlayerSettings.Android.bundleVersionCode + "." + Application.identifier + ".obb";
                string destName = Path.Combine(Dir, obbName);
                Debug.Log("------destName----------" + destName);
                if (File.Exists(destName))
                {
                    Debug.Log("------destName-----123-----"+ destName);
                    File.Delete(destName);
                }
                Directory.Move(sourceName, destName);

                Debug.Log("Obb Change Name:" + sourceName + " To " + destName);
            }

            //path = path.Replace(" ", "");
            //Log.Info_green("----path----" + path);
            //Log.Info_green("----path----" + path);
            //if (path.StartsWith(ApkName))
            //{
            //    Log.Info_green("---StartsWith-----");
            //}
            //if (path.EndsWith(".obb"))
            //{
            //    Log.Info_green("----EndsWith----");
            //}
        }

        [MenuItem("Tool/test1")]
        public static void test1() {
            INIParser ini = new INIParser();
            ini.Open($"D:/SDKDomeProject/TestProject/Outputs/Android/sjoys_app.ini");
            ini.WriteValue("Player", "app_id", "123");
            ini.WriteValue("Player", "sdkversion", "5.3.3");
            ini.WriteValue("Player", "debug", 1);
            ini.Close();

        }
        [MenuItem("Tool/test")]
        public static void test() {


            string s = "0409_1840_CeliaOversea_Devlopment_thethroneofgirl-game-smartplay-com-tw.main.obb";
            string s1 = Path.Combine(streamPath,s);

            if (File.Exists(s1))
            {
                Debug.Log("----s1-----" + s1);
            }
            return;
            string configPath = new DirectoryInfo(Application.dataPath).Parent.FullName + configPath_jianti;
            //string configPath = Path.Combine(new DirectoryInfo(Application.dataPath).Parent.FullName,configPath_jianti);
            Debug.Log("-----streamPath----"+streamPath);
            //1.delel
            if (Directory.Exists(streamPath))
            {
                Debug.Log("---------");
                Directory.Delete(streamPath, true);
            }
            Directory.CreateDirectory(streamPath);
            //2.copy
            if (!Directory.Exists(configPath))
            {
                Debug.LogError("--not found configPath:--" + configPath);
                return;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(configPath);
            List<FileInfo> encryptFile = new List<FileInfo>();
            FileInfo[] fis = directoryInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.FullName.EndsWith(".json"))
                {
                    encryptFile.Add(fi);
                }
            }
            int index = 0;
            foreach (FileInfo fi in encryptFile)
            {
                index++;
                EditorUtility.DisplayCancelableProgressBar($"加密配置中({index}/{encryptFile.Count})", fi.Name, index * 1.0f / encryptFile.Count);

                string content = File.ReadAllText(fi.FullName);
                content = DESEncrypt.Encrypt(content);
                string lastPath = streamPath + "/" + fi.Name;
                WriteInfo(content, lastPath);
            }

            EditorUtility.ClearProgressBar();
            //3.EncryptData
        }
        [MenuItem("Tool/Encrypt Data")]
        public static void EncryptData()
        {
            //config
            Debug.Log("Encrypt Data.");

            string path = Application.streamingAssetsPath + "/Config";

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            List<FileInfo> encryptFile = new List<FileInfo>();

            FileInfo[] fis = directoryInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.FullName.EndsWith(".json"))
                {
                    encryptFile.Add(fi);
                }
            }

            int index = 0;
            foreach (FileInfo fi in encryptFile)
            {
                index++;
                EditorUtility.DisplayCancelableProgressBar($"加密配置中({index}/{encryptFile.Count})", fi.Name, index * 1.0f / encryptFile.Count);

                string content = File.ReadAllText(fi.FullName);
                if (IsEncrypted(content))
                {
                    //如果文件已经加密就跳出
                    Debug.Log("The data has been encrypted");
                    break;
                }
                content = DESEncrypt.Encrypt(content);

                WriteInfo(content, fi.FullName);
            }

            EditorUtility.ClearProgressBar();

            Debug.Log("Encrypt Data Completed.");
        }
        [MenuItem("Tool/Decrypt Data")]
        public static void DecryptData()
        {
            //config
            Debug.Log("Decrypt Data.");

            string path = Application.streamingAssetsPath + "/Config";

            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            List<FileInfo> encryptFile = new List<FileInfo>();

            FileInfo[] fis = directoryInfo.GetFiles();
            foreach (FileInfo fi in fis)
            {
                if (fi.FullName.EndsWith(".json"))
                {
                    encryptFile.Add(fi);
                }
            }

            int index = 0;
            foreach (FileInfo fi in encryptFile)
            {
                index++;
                EditorUtility.DisplayCancelableProgressBar($"解密配置中({index}/{encryptFile.Count})", fi.Name, index * 1.0f / encryptFile.Count);

                string content = File.ReadAllText(fi.FullName);
                if (!IsEncrypted(content))
                {
                    //如果文件已经解密了就跳出
                    Debug.Log("The data has been decrypted");
                    break;
                }
                content = DESEncrypt.Decrypt(content);

                WriteInfo(content, fi.FullName);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            Debug.Log("Decrypt Data Completed.");
        }
        //========================================================================
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="obj">数据</param>
        /// <param name="path">路径</param>
        /// <param name="flag">是否显示Log</param>
        public static void WriteInfo(string str, string path, bool flag = true)
        {
            Debug.Log("-WriteInfo-Write-path-" + path);
            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, new UTF8Encoding(false));
            sw.WriteLine(str);
            sw.Close();
            fs.Close();
            Debug.Log("Write Completed : " + path);

        }
        /// <summary>
        /// 返回json字符串是否被加密
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static bool IsEncrypted(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                if (json.StartsWith("{") && json.EndsWith("}"))
                {
                    return false;
                }
                return true;
            }
            return false;
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string DecryptString(string str)
        {
            if (IsEncrypted(str))
            {
                return DESEncrypt.Decrypt(str);
            }
            else
            {
                return str;
            }
        }

        [MenuItem("简繁体设置/当前语言")]
        public static void CheckCurLanguage() {
            ConfigLanguage configLanguage = GetCurLanguage();
            switch (configLanguage) {
                case ConfigLanguage.None:
                    Log.Info_blue("当前没有配置文件");
                    break;
                case ConfigLanguage.Simplified:
                    Log.Info_green("当前为简体配置文件");
                    break;
                case ConfigLanguage.Traditional:
                    Log.Info_green("当前为繁体配置文件");
                    break;
            }
        }
        public static ConfigLanguage GetCurLanguage()
        {
            //language
            string path = streamPath + "/Simplified.json";
            if (File.Exists(path))
            {
                return ConfigLanguage.Simplified;
            }
            path = streamPath + "/Traditional.json";
            if (File.Exists(path))
            {
                return ConfigLanguage.Traditional;
            }
            return ConfigLanguage.None;
        }
        [MenuItem("简繁体设置/切换为简体")]
        public static void SwitchToSimplified()
        {
            //Simplified Chinese
            if (GetCurLanguage() == ConfigLanguage.Simplified)
            {
                Log.Info_blue("当前已经为简体配置文件");
                return;
            }
            if (Directory.Exists(streamPath))
            {
                Directory.Delete(streamPath, true);
            }
            Util.Instance.CopyFolder(configPath_jianti, streamPath);
            Log.Info_green("已经切换为简体配置文件");
        }
        [MenuItem("简繁体设置/切换为繁体")]
        public static void SwitchToTraditional()
        {
            //Traditional Chinese
            if (GetCurLanguage() == ConfigLanguage.Traditional)
            {
                Log.Info_blue("当前已经为繁体配置文件");
                return;
            }
            if (Directory.Exists(streamPath))
            {
                Directory.Delete(streamPath, true);
            }
            Util.Instance.CopyFolder(configPath_fanti, streamPath);
            Log.Info_green("已经切换为繁体配置文件");
        }
        public enum ConfigLanguage
        {
            None,
            Simplified,
            Traditional,
        }

    }
}

