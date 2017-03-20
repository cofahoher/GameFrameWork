using UnityEngine;
using System.IO;
using System;

public class EditorPlatformHelper : ARunningPlatformHelper
{
    public override string StoragePath
    {
        get
        {
            return Application.dataPath + "/../" + GameHelper.sm_product_prefix + PATH_END;
        }
    }
    public override string TempCachePath
    {
        get { return Application.temporaryCachePath + PATH_END; }
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
        //get { return Application.persistentDataPath + PATH_END; }
    }

    public override bool IsFileExist(string path)
    {
        return File.Exists(path);
    }

    public override void OpenAndroidGPSSystemSetting()
    {
    }


    public override bool CopyStreamingAsset(string from, string to)
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

    public override bool IsFileExistInStreamingAssets(string path)
    {
        string abspath = StreammingPath + path;

        return File.Exists(abspath);
    }

    public override long GetFreeSpace()
    {
        string driveName = Path.GetPathRoot(Application.dataPath);
        DriveInfo[] drives = DriveInfo.GetDrives();
        foreach (DriveInfo drive in drives)
        {
            if (drive.Name == driveName)
                return drive.AvailableFreeSpace;
        }
        return -1;
    }

    public override bool IsSpaceEnough(long size)
    {
        return size > GetFreeSpace();
    }

    public override NetworkReachability GetNetworkConnectedState()
    {
        return Application.internetReachability;
    }

    public override string GetDeviceModel()
    {
        return SystemInfo.deviceModel;
    }
}