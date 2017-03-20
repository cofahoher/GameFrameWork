using UnityEngine;
using System.IO;
using System;

public abstract class ARunningPlatformHelper
{
    protected const string PATH_END = "/";

    public abstract string StoragePath
    {
        get;
    }

    public abstract string TempCachePath
    {
        get;
    }

    public abstract string PersistentPath
    {
        get;
    }
    public abstract string PersistentUrl
    {
        get;
    }
    public abstract string StreammingPath
    {
        get;
    }

    public abstract string StreammingUrl
    {
        get;
    }

    public abstract string StreamingLocalABUrl
    {
        get;
    }

    public abstract void OpenAndroidGPSSystemSetting();

    public abstract bool IsFileExist(string path);

    public virtual bool SaveFile(string path, byte[] data)
    {
        try
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                fs.Write(data, 0, data.Length);
            }
        }
        catch (System.Exception e)
        {
            LogWrapper.LogError(e.ToString());
            return false;
        }

        return true;
    }

    public abstract bool CopyStreamingAsset(string from, string to);
    public abstract bool IsFileExistInStreamingAssets(string path);

    public virtual bool RenameFile(string former, string dest)
    {
        try
        {
            if (!File.Exists(former))
            {
                LogWrapper.LogError("renamed former file doesn't exist " + former);
                return false;
            }

            // Ensure that the target does not exist.
            if (File.Exists(dest))
                File.Delete(dest);

            // Move the file.
            File.Move(former, dest);

            return true;
        }
        catch (Exception e)
        {
            LogWrapper.LogError("rename process failed: " + e.ToString());
            return false;
        }
    }

    public virtual void DeleteFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);

        }
        catch (Exception e)
        {
        }
    }

    public abstract long GetFreeSpace();

    public abstract bool IsSpaceEnough(long size);

    public abstract NetworkReachability GetNetworkConnectedState();
    public abstract string GetDeviceModel();
}
