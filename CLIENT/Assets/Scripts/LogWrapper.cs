/*
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogWrapper
{
    private static string ToStrBuff(object[] data)
    {
        StringBuilder sb = new StringBuilder(256);
        for (int i = 0; i < data.Length; ++i)
        {
            sb.AppendFormat(" {0}", data[i]);
        }
        return sb.ToString();
    }

    public static void LogDebug(params object[] data)
    {
        LogDebug(ToStrBuff(data));
    }

    public static void LogDebug(string info)
    {
        LogDebug(info);
    }

    public static void LogInfo(params object[] data)
    {
        LogInfo(ToStrBuff(data));
    }

    public static void LogInfo(string info)
    {
        LogInfo(info);
    }
    public static void LogWarning(params object[] data)
    {
        LogWarning(ToStrBuff(data));
    }

    public static void LogWarning(string info)
    {
        LogWarning(info);
    }

    public static void LogError(params object[] data)
    {
        LogError(ToStrBuff(data));
    }

    public static void LogError(string info)
    {
        LogError(info);
    }

    public static void LogCritical(params object[] data)
    {
        LogError(ToStrBuff(data));
    }

    public static void LogCritical(string info)
    {
        LogError(info);
    }
}
*/
