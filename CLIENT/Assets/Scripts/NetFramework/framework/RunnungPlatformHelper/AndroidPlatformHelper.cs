using UnityEngine;
using System.IO;
using System;

public class AndroidPlatformHelper : ARunningPlatformHelper
{
#if UNITY_ANDROID

    public static string sm_storage_class = "com.horizon3d.Storage";
    static AndroidJavaClass unity = null;
    static AndroidJavaObject activity = null;
    static AndroidJavaClass storage = null;

    static void CheckAndroidVar()
    {
        if (unity == null)
        {
            unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            storage = new AndroidJavaClass(sm_storage_class);
        }
    }
    
#endif

    public override string StoragePath
    {
        get
        {
            return PersistentPath + "/" + GameHelper.sm_product_prefix + PATH_END;
        }
    }

    public override string TempCachePath
    {
        get
        {
            return Application.temporaryCachePath + "/" + GameHelper.sm_product_prefix + PATH_END;
        }
    }

    public override string PersistentPath
    {
        get
        {
            return Application.persistentDataPath + PATH_END;
        }
    }

    public override string PersistentUrl
    {
        get
        {
            return "file://" + Application.persistentDataPath + PATH_END;
        }
    }
    /// <summary>
    /// 安卓平台下 StreamingAssets 文件夹在APK包内，所以返回的路径是安卓风格的url
    /// </summary>
    public override string StreammingPath
    {
        get { return Application.streamingAssetsPath + PATH_END; }
    }
    /// <summary>
    /// 安卓平台下 StreamingAssets 文件夹在APK包内，所以返回的路径是安卓风格的url
    /// 这里和StreammingPath一致
    /// </summary>
    public override string StreammingUrl
    {
        get { return Application.streamingAssetsPath + PATH_END; }
    }

    public override string StreamingLocalABUrl
    {
        get { return Application.dataPath + "!assets/"; }
       // get { return Application.persistentDataPath + "/"; }
    }

    public override void OpenAndroidGPSSystemSetting()
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();
            //LogWrapper.LogInfo("gps setting#########################################################");
            storage.CallStatic("OpenSystemGPSSetting", activity);
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
        }
#endif
    }
    public override bool IsFileExist(string path)
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();

            return storage.CallStatic<bool>("IsFileExist", path);
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
        }
#endif
        return false;
    }

    public override bool CopyStreamingAsset(string from, string to)
    {
        throw new NotImplementedException();
    }

    public override bool IsFileExistInStreamingAssets(string path)
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();

            return storage.CallStatic<bool>("IsFileExistInAsset", path, activity);
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
        }
#endif
        return false;
    }

    public override long GetFreeSpace()
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();

            return storage.CallStatic<long>("GetFreeSpace");
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
        }
#endif
        return 0;
    }

    public override bool IsSpaceEnough(long size)
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();
            bool result = storage.CallStatic<bool>("IsSpaceEnough", size);
            return result;
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
        }
#endif
        return false;
    }

    public override NetworkReachability GetNetworkConnectedState()
    {

#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();

            return (NetworkReachability)storage.CallStatic<int>("getNetworkState", activity);
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
            return NetworkReachability.NotReachable;
        }
#else
        return NetworkReachability.ReachableViaLocalAreaNetwork;
#endif
    }

    public override string GetDeviceModel()
    {
#if UNITY_ANDROID
        try
        {
            CheckAndroidVar();

            return storage.CallStatic<string>("GetDeviceModel");
        }
        catch (System.Exception ex)
        {
            LogWrapper.LogInfo(ex.ToString());
            return "Unknown Android Device";
        }
#else
        return string.Empty;
#endif
    }
}