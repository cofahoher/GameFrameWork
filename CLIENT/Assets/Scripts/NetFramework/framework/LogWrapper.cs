using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if CONSOLE_CLIENT
using System.Diagnostics;
#else
using UnityEngine;
#endif

public interface ILogWriter
{
    void WriteLog(string line);
}

public class LogFileWriter : ILogWriter
{
    /// <summary>
    /// 文件流
    /// </summary>
    private StreamWriter mStreamWriter = null;
    /// <summary>
    /// 初始化标记，避免重复初始化
    /// </summary>
    private bool mIsInit = false;

    const int MAX_LOG_ENTRY_NUM = 10;

    public void WriteLog(string line)
    {
        if (mStreamWriter != null)
        {
            lock (mStreamWriter)
            {
                try
                {
                    mStreamWriter.WriteLine(line);
                }
                catch (System.Exception)
                {

                }
            }
        }
    }

    private static string[] GetLogFiles(string logFilePath)
    {
        return Directory.GetFiles(logFilePath, "*.txt", SearchOption.TopDirectoryOnly);
    }

    public void Init()
    {
        if (mIsInit)
            return;

        try
        {
#if CONSOLE_CLIENT
            if (!Directory.Exists(LogWrapper.LOG_DIR))
            {
                Directory.CreateDirectory(LogWrapper.LOG_DIR);
            }
            string fileName = LogWrapper.LOG_DIR + string.Format("{0}_{1:D2}_{2:D2}_{3:D2}_{4:D2}_{5:D2}.{6:D5}.{7}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second, Process.GetCurrentProcess().Id, "log.txt");
#else
            string logDir = GameHelper.StoragePath + "log";

#if UNITY_IOS
            logDir = GameHelper.TempCachePath + "log";
#endif

            if (!Directory.Exists(logDir))
            {
                Directory.CreateDirectory(logDir);
            }

            string[] logs = GetLogFiles(logDir);

            if (logs.Length > MAX_LOG_ENTRY_NUM)
            {
                foreach (var log in logs)
                {
                    File.Delete(log);
                }
            }

            string fileName = logDir + "/" + string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}",
                DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute,
                DateTime.Now.Second, "log.txt");
#endif
            FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate);
            mStreamWriter = new StreamWriter(fs);
            mStreamWriter.AutoFlush = true;
        }
        catch (System.Exception)
        {
            mStreamWriter = null;
        }

        mIsInit = true;
    }
}

public class LogWrapper
{
    public static string sm_log_prefix = ": H3D_Shoot";
    static LogFileWriter sm_log_file_writer = new LogFileWriter();
    static ILogWriter sm_gui_writer = null;

    public enum ELogLevel
    {
        DEBUG = 800,
        INFO = 700,
        WARNING = 500,
        ERROR = 400,
        CRITICAL = 300,
    }
    private static ELogLevel m_log_level = ELogLevel.DEBUG;
    public static ELogLevel LogLevel
    {
        get { return m_log_level; }
        set { m_log_level = value; }
    }

#if CONSOLE_CLIENT
   
    public static String LOG_DIR = "../logs/";
#else
#endif
    private static bool mEnabled = true;
    public static ILogWriter GUIWriter { set { sm_gui_writer = value; } }
    public static bool Enabled
    {
        get
        {
            return mEnabled;
        }
        set
        {
            mEnabled = value;
        }
    }

    static bool CheckLogLevel(ELogLevel level)
    {
        if (mEnabled && m_log_level >= level)
        {
            sm_log_file_writer.Init();
            return true;
        }
        return false;
    }

    private static string ToStrBuff(object[] data)
    {
        StringBuilder sb = new StringBuilder(256);
        for (int i = 0; i < data.Length; ++i)
        {
            sb.AppendFormat(" {0}", data[i]);
        }
        return sb.ToString();
    }

    private static void WriteLogLine(string levelname, ELogLevel level, string info)
    {
        if (!CheckLogLevel(level))
            return;

        //-----------------------
#if CONSOLE_CLIENT
        string line = string.Format("{1} {2} {3} - {0}", info, DateTime.Now.ToString(), sm_log_prefix, levelname);
        Console.WriteLine( line );
#else
        string line = string.Format("{1} {2} {3} - {0}", info, DateTime.Now.ToString(), sm_log_prefix, levelname);
        switch (level)
        {
            case ELogLevel.DEBUG:
                Debug.Log(line);
                break;
            case ELogLevel.INFO:
                Debug.Log(line);
                break;
            case ELogLevel.WARNING:
                Debug.LogWarning(line);
                break;
            case ELogLevel.ERROR:
                Debug.LogError(line);
                break;
            case ELogLevel.CRITICAL:
                Debug.LogError(line);
                break;
            default:
                Debug.Log(line);
                break;
        }
#endif
        //-----------------------

        sm_log_file_writer.WriteLog(line);

        if (sm_gui_writer != null)
        {
            sm_gui_writer.WriteLog(line);
        }
    }

    public static void LogDebug(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.DEBUG))
            return;

        LogDebug(ToStrBuff(data));
    }

    public static void LogDebug(string info)
    {
        WriteLogLine("Debug", ELogLevel.DEBUG, info);
    }

    public static void LogInfo(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.INFO))
            return;

        LogInfo(ToStrBuff(data));
    }

    public static void LogInfo(string info)
    {
        WriteLogLine("Info", ELogLevel.INFO, info);
    }
    public static void LogWarning(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.WARNING))
            return;

        LogWarning(ToStrBuff(data));
    }

    public static void LogWarning(string info)
    {
        WriteLogLine("Warning", ELogLevel.WARNING, info);
    }

    public static void LogError(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.ERROR))
            return;

        LogError(ToStrBuff(data));
    }

    public static void LogError(string info)
    {
        WriteLogLine("Error", ELogLevel.ERROR, info);
    }

    public static void LogCritical(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.CRITICAL))
            return;

        LogCritical(ToStrBuff(data));
    }

    public static void LogCritical(string info)
    {
        WriteLogLine("Critical", ELogLevel.CRITICAL, info);
    }

    public static void Exception(Exception exp)
    {
        sm_log_file_writer.Init();

        WriteLogLine("Exception", ELogLevel.CRITICAL, exp.ToString());
    }

    public static void LogErrorDebugWrapper(string info)
    {
        string line = string.Format("{0}{1}{2}", "<color=#22BB00FF>DEBUG_ONLY: ", info, "</color>");
        WriteLogLine("Error", ELogLevel.ERROR, line);
    }

    public static void LogErrorDebugWrapper(params object[] data)
    {
        if (!CheckLogLevel(ELogLevel.ERROR))
            return;

        LogErrorDebugWrapper(ToStrBuff(data));
    }
}
