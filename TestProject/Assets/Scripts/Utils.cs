using System.Security.Cryptography;
using System.Text;
using System;

public static class Utils
{
    public static string ip => GameSetting.gi.ip;
    public static uint port => GameSetting.gi.port;

    public static string GetMD5Key(string oriKey)
    {
        byte[] data = new MD5CryptoServiceProvider().ComputeHash(new UTF8Encoding().GetBytes(oriKey));
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < data.Length; i++)
        {
            builder.Append(data[i].ToString("x2"));
        }
        return builder.ToString().ToLower();
    }

    public static int ConvertDateTimeInt(System.DateTime time)
    {
        double intResult = 0;
        System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
        intResult = (time - startTime).TotalSeconds;
        return (int)intResult;
    }
    public static long TimeStamp { get { return (long)(DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds; } }
}
