using System;
using System.IO;
using System.Text;

using UnityEditor;

public class Tools : Editor
{
    [MenuItem("Tools/文件编码")]
    public static void CheckEncoding()
    {
        String folderPath = @"F:\TheLegendOfCelia\TheLegendOfCelia_Dev\";
        ParseDirectory(folderPath, "*.cs", (filePath) =>
        {
            FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            Encoding e = GetType(fs, filePath);
            fs.Close();
            //Encoding utf8 = new UTF8Encoding();

            //string text = string.Empty;
            //Encoding oldEncoding = getEncoding(filePath);
            //Log.Info_blue(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "---oldEncoding--" + oldEncoding.ToString());
            //using (StreamReader sr = new StreamReader(filePath, oldEncoding))
            //{
            //    text = sr.ReadToEnd();
            //    Log.Info_blue("--text-->" + text);
            //}
            ////new UTF8Encoding()    new UTF8Encoding(false) 不带BOM
            ////new UTF8Encoding(true) 带BOM
            //using (StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding()))
            //{
            //    sw.Write(text);
            //    Log.Info_blue(filePath.Substring(filePath.LastIndexOf("\\") + 1) + " 编码标准化完成");
            //}
        });
    }

    [MenuItem("Tools/文件编码格式检查")]
    public static void CheckFileBom()
    {
        String folderPath = @"D:\SDKDomeProject\TestProject\Assets\Editor\Base\";

        ParseDirectory(folderPath, "*.cs", (filePath) =>
        {
            string text = string.Empty;
            Encoding oldEncoding = getEncoding(filePath);
            Log.Info_blue(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "---oldEncoding--" + oldEncoding.ToString());
            using (StreamReader sr = new StreamReader(filePath, oldEncoding))
            {
                text = sr.ReadToEnd();
            }
            using (StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false)))
            {
                sw.Write(text);
                Log.Info_blue(filePath.Substring(filePath.LastIndexOf("\\") + 1) + " 编码标准化完成");
            }
        });
    }

    [MenuItem("Tools/文件行尾检查")]
    public static void CheckFile()
    {
        string s = Environment.NewLine;
        String folderPath = @"F:\TheLegendOfCelia\TheLegendOfCelia_Dev\";

        ParseDirectory(folderPath, "*.cs", (filePath) =>
       {
           string text = "";
           string text2 = "";
           using (StreamReader read = new StreamReader(filePath, Encoding.Default))
           {
               string oldtext = read.ReadToEnd();
               text = oldtext;
               text2 = oldtext;
               text = text.Replace("\r", "\r\n");
               text = text.Replace("\r\n\n", "\r\n"); // 防止替换了正常的换行符(CR->CRLF)
               if (oldtext.Length != text.Length)
               {
                   Log.Info_red(filePath + "当前行尾为CR");
                   return;
               }
               text2 = text2.Replace("\n", "\r\n");
               text2 = text2.Replace("\r\r\n", "\r\n"); // 防止替换了正常的换行符(LF->CRLF)
               if (oldtext.Length != text2.Length)
               {
                   Log.Info_red(filePath + "当前行尾为LF");
                   return;
               }
               Log.Info_green(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "当前行尾为CRLF");
           }

           //using (StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false)))
           //{
           //    sw.Write(text);
           //    Log.Info_blue(filePath.Substring(filePath.LastIndexOf("\\") + 1) + " 行尾标准化完成");
           //}
       });
    }

    /// <summary>递归所有的目录，根据过滤器找到文件，并使用委托来统一处理</summary>
    /// <param name="info"></param>
    /// <param name="filter"></param>
    /// <param name="action"></param>
    private static void ParseDirectory(string folderPath, string filter, Action<string> action)
    {
        if (string.IsNullOrWhiteSpace(folderPath)
           || folderPath.EndsWith("debug", StringComparison.OrdinalIgnoreCase)
           || folderPath.EndsWith("obj", StringComparison.OrdinalIgnoreCase)
           || folderPath.EndsWith("bin", StringComparison.OrdinalIgnoreCase))
            return;

        // 处理文件
        string[] fileNameArray = Directory.GetFiles(folderPath, filter);
        if (fileNameArray.Length > 0)
        {
            foreach (var filePath in fileNameArray)
            {
                action(filePath);
            }
        }
        else
        {
        }

        //得到子目录，递归处理
        string[] dirs = Directory.GetDirectories(folderPath);
        var iter = dirs.GetEnumerator();
        while (iter.MoveNext())
        {
            string str = (string)(iter.Current);
            ParseDirectory(str, filter, action);
        }
    }

    /// <summary>
    /// 获取文件编码格式
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    private static Encoding getEncoding(string filePath)
    {
        using (var reader = new StreamReader(filePath, Encoding.Default, true))
        {
            if (reader.Peek() >= 0) // you need this!
                reader.Read();

            Encoding encoding = reader.CurrentEncoding;
            reader.Close();
            return encoding;
        }
    }

    /// <summary>
    /// 通过给定的文件流，判断文件的编码类型
    /// </summary>
    /// <param name=“fs“>文件流</param>
    /// <returns>文件的编码类型</returns>
    public static System.Text.Encoding GetType(FileStream fs, string filePath)
    {
        byte[] Unicode = new byte[] { 255, 254, 65 };
        byte[] UnicodeBIG = new byte[] { 254, 255, 0 };
        byte[] UTF8 = new byte[] { 239, 187, 191 }; //带BOM    //{ oxEF, oxBB, oxBF }
        Encoding reVal = Encoding.Default;

        BinaryReader r = new BinaryReader(fs);
        //BinaryReader r = new BinaryReader(fs, System.Text.Encoding.Default);
        int i;
        int.TryParse(fs.Length.ToString(), out i);
        byte[] ss = r.ReadBytes(i);
        if (IsUTF8Bytes(ss))
        {
            if ((ss[0] == 0xEF && ss[1] == 0xBB && ss[2] == 0xBF))
            {
                Log.Info_red(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "编码为：BOM-UTF8");
            }
            else
            {
                Log.Info_green(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "编码为：UTF8");
            }
        }
        else if (ss[0] == 0xFE && ss[1] == 0xFF && ss[2] == 0x00)
        {
            Log.Info_red(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "编码为：BigEndianUnicode");
            reVal = Encoding.BigEndianUnicode;
        }
        else if (ss[0] == 0xFF && ss[1] == 0xFE && ss[2] == 0x41)
        {
            Log.Info_red(filePath.Substring(filePath.LastIndexOf("\\") + 1) + "编码为：Unicode");
            reVal = Encoding.Unicode;
        }
        r.Close();
        return reVal;
    }

    /// <summary>
    /// 判断是否是不带 BOM 的 UTF8 格式
    /// </summary>
    /// <param name=“data“></param>
    /// <returns></returns>
    private static bool IsUTF8Bytes(byte[] data)
    {
        int charByteCounter = 1; //计算当前正分析的字符应还有的字节数
        byte curByte; //当前分析的字节.
        for (int i = 0; i < data.Length; i++)
        {
            curByte = data[i];
            if (charByteCounter == 1)
            {
                if (curByte >= 0x80)
                {
                    //判断当前
                    while (((curByte <<= 1) & 0x80) != 0)
                    {
                        charByteCounter++;
                    }
                    //标记位首位若为非0 则至少以2个1开始 如:110XXXXX...........1111110X
                    if (charByteCounter == 1 || charByteCounter > 6)
                    {
                        return false;
                    }
                }
            }
            else
            {
                //若是UTF-8 此时第一位必须为1
                if ((curByte & 0xC0) != 0x80)
                {
                    return false;
                }
                charByteCounter--;
            }
        }
        if (charByteCounter > 1)
        {
            throw new Exception("非预期的byte格式");
        }
        return true;
    }
}