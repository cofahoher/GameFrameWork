using UnityEngine;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class IOSPlatformHelper : ARunningPlatformHelper
{
    //[DllImport("__Internal")]
    //private static extern string _GetIOSDeviceModel();

    //[DllImport("__Internal")]
    //private static extern UInt64 _GetIOSFreeDiskSpace();

    //[DllImport("__Internal")]
    //private static extern int _GetIOSNetworkState();

    public override string StoragePath
    {
        get
        {
            return Application.persistentDataPath + "/" + GameHelper.sm_product_prefix + PATH_END;
        }
    }

    public override string TempCachePath
    {
        get { return Application.temporaryCachePath + "/" + GameHelper.sm_product_prefix + PATH_END; }
    }

    public override string PersistentPath
    {
        get { return Application.persistentDataPath + PATH_END; }
    }

    public override string PersistentUrl
    {
        get { return "file://" + Application.persistentDataPath + PATH_END; }
    }

    public override string StreammingPath
    {
        get { return Application.streamingAssetsPath + PATH_END; }
    }

    public override string StreammingUrl
    {
        get { return "file://" + Application.streamingAssetsPath + PATH_END; }
    }

    public override string StreamingLocalABUrl
    {
        get { return Application.streamingAssetsPath + PATH_END; }
    }

  

    public override bool IsFileExist(string path)
    {
        try
        {
            return File.Exists(path);
        }
        catch (System.Exception ex)
        {
        	
        }
        return false;
    }

    public override void OpenAndroidGPSSystemSetting()
    {
    }

    public override bool CopyStreamingAsset(string from, string to)
    {
        try
        {
            string absfrom = StreammingPath + from;
            if (!File.Exists(absfrom))
                return false;
            string destDir = Path.GetDirectoryName(to);
            if (!Directory.Exists(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
            File.Copy(absfrom, to, true);
            return true;
        }
        catch (System.Exception ex)
        {
        	
        }
        return false;
    }

    public override bool IsFileExistInStreamingAssets(string path)
    {
        try
        {
            string abspath = StreammingPath + path;

            return File.Exists(abspath);
        }
        catch (System.Exception ex)
        {
        	
        }
        return false;
    }

    public override long GetFreeSpace()
    {
        return 0;
        //long freeSpace = (long)_GetIOSFreeDiskSpace();
        //return freeSpace;
    }

    public override bool IsSpaceEnough(long size)
    {
        return true;
        //UInt64 freeSpace = _GetIOSFreeDiskSpace();
        //LogWrapper.LogInfo("-------------------------free disk space(M) : " + (freeSpace / 1024 / 1024));
        //return freeSpace > (UInt64)size;
    }

    public override NetworkReachability GetNetworkConnectedState()
    {
        return NetworkReachability.ReachableViaLocalAreaNetwork;
        //return (NetworkReachability)_GetIOSNetworkState();
    }

    public override string GetDeviceModel()
    {
        //string device_string = _GetIOSDeviceModel();
        //return device_string;
        return "";
    }
}