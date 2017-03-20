using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using System.Security.Cryptography;
using I18N.Common;
using I18N.CJK;

public class GameHelper
{
    public static string sm_product_prefix = "H3D_Shoot";

    static ARunningPlatformHelper m_PlatformHelperInstance;

    static ARunningPlatformHelper PlatformHelper
    {
        get
		{
			if (m_PlatformHelperInstance == null)
            {

#if UNITY_EDITOR_WIN
                m_PlatformHelperInstance = new WinEditorPlatformHelper();
#elif UNITY_EDITOR
                m_PlatformHelperInstance = new EditorPlatformHelper();
#elif UNITY_ANDROID
                m_PlatformHelperInstance = new AndroidPlatformHelper();
#elif UNITY_IOS
                m_PlatformHelperInstance = new IOSPlatformHelper();
#else
				//may be stand alone
				if(Application.platform == RuntimePlatform.WindowsPlayer)
				{
					m_PlatformHelperInstance = new WinPlatformHelper();
				}
#endif
            }

            return m_PlatformHelperInstance;
        }
    }



    //static string ex_path = "";

    public static void OpenAndroidGPSSystemSetting()
    {
        PlatformHelper.OpenAndroidGPSSystemSetting();
    }
    /// <summary>
    /// Editor:     client/H3D_Shoot/
    /// IOS:        AppHome/Documents/H3D_Shoot/
    /// Android:    /data/data/(boundle_id)/files/H3D_shoot/
    /// </summary>
    static public string StoragePath
    {
        get
        {
            string path = PlatformHelper.StoragePath;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (System.Exception ex)
            {
            }
            return path;
        }
    }
    static public string TempCachePath
    {
        get 
        {
            string path = PlatformHelper.TempCachePath;
            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (System.Exception ex)
            {
            }
            return path;
        }
    }
    /// <summary>
    /// Editor:     client/H3D_Shoot/
    /// IOS:        AppHome/Documents/
    /// Android:    /data/data/(boundle_id)/files/
    /// </summary>
    static public string PersistentPath
    {
        get
        {
            return PlatformHelper.PersistentPath;
        }
    }
    /// <summary>
    /// Editor:     file://client/H3D_Shoot/
    /// IOS:        file://AppHome/Documents/H3D_Shoot/
    /// Android:    file:///data/data/(boundle_id)/files/H3D_shoot/
    /// </summary>
    static public string PersistentUrl
    {
        get
        {
            return PlatformHelper.PersistentUrl;
        }
    }
    /// <summary>
    /// Editor:     client/Assets/StreammingAssets/
    /// IOS:        AppHome/(App)/Data/Raw/
    /// Android:    jar:file:///data/app/(bundle_id).apk/!/assets/
    /// </summary>
    static public string StreamingAssetsPath
    {
        get
        {
            return PlatformHelper.StreammingPath;
        }
    }
    /// <summary>
    /// Editor:     file://client/Assets/StreammingAssets/
    /// IOS:        file://AppHome/(App)/Data/Raw/
    /// Android:    jar:file:///data/app/(bundle_id).apk/!/assets/
    /// </summary>
    static public string StreammingAssetsUrl
    {
        get
        {
            return PlatformHelper.StreammingUrl;
        }
    }

    static public string StreamingAssetsLocalABUrl
    {
        get
        {
            return PlatformHelper.StreamingLocalABUrl;
        }
    }

    static public bool CopyStreamingAssetFile(string from, string to)
    {
        return PlatformHelper.CopyStreamingAsset(from, to);
    }

    static public bool IsFileExist(string path)
    {
        return PlatformHelper.IsFileExist(path);
    }

    static public bool IsFileExistInStreamingAssets(string path)
    {
        return PlatformHelper.IsFileExistInStreamingAssets(path);
    }

    static public string GetDeviceModel()
    {
        return PlatformHelper.GetDeviceModel();
    }

    static public bool SaveFile(string path, byte[] data)
    {
        return PlatformHelper.SaveFile(path, data);
    }

    public static bool RenameFile(string former, string dest)
    {
        return PlatformHelper.RenameFile(former, dest);
    }

    public static void DeleteFile(string path)
    {
        PlatformHelper.DeleteFile(path);
    }

    static long GetFreeSpace()
    {
        return PlatformHelper.GetFreeSpace();
    }

    static public bool IsSpaceEnough(int size)
    {
        return PlatformHelper.IsSpaceEnough((long)size);
    }

    //0 no connected 1 carrier 2 wifi
    static public NetworkReachability GetNetworkConnectedState()
    {
        return PlatformHelper.GetNetworkConnectedState();
    }



    //static public CommonInfo GetCommonInfo()
    //{
    //    return CommonInfo.Ins();
    //}

    //static public CommonUIInfo GetCommonUIInfo()
    //{
    //    return CommonUIInfo.Ins();
    //}

    //static public string GetErrorDesc(int id)
    //{
    //    return GetCommonUIInfo().errCfg.GetErrorDesc(id);
    //}

    static public Vector2 TransferPos(Vector2 org_pos)
    {
        Vector2 pos = new Vector2(org_pos.x, org_pos.y);
        pos -= new Vector2(Screen.width / 2, Screen.height / 2);
        pos *= 640f / (float)Screen.height;

        if (GameHelper.IsIPadResolutionMode())
        {
            pos /= GameHelper.IPAD_RESOLUTION_SCALE_VALUE;
        }

        return pos;
    }

    static public string GetUTF8String(byte[] buffer)
    {
        if (buffer == null)
            return null;

        if (buffer.Length <= 3)
        {
            return Encoding.UTF8.GetString(buffer);
        }

        byte[] bomBuffer = new byte[] { 0xef, 0xbb, 0xbf };

        if (buffer[0] == bomBuffer[0]
            && buffer[1] == bomBuffer[1]
            && buffer[2] == bomBuffer[2])
        {
            return new UTF8Encoding(false).GetString(buffer, 3, buffer.Length - 3);
        }

        return Encoding.UTF8.GetString(buffer);
    }

    //public static PopupManager GetPopupManager()
    //{
    //    return GameObject.Find("_ImmortalObjects").GetComponentInChildren<PopupManager>();
    //}

    public const float IPAD_RESOLUTION_SCALE_VALUE = 0.8f;

    public static bool IsIPadResolutionMode()
    {
        bool iPadResolution = false;
#if (UNITY_IOS) && (!UNITY_EDITOR)  
        //if (iPhone.generation == iPhoneGeneration.iPad1Gen ||
        //    iPhone.generation == iPhoneGeneration.iPad2Gen ||
        //    iPhone.generation == iPhoneGeneration.iPad3Gen || 
        //    iPhone.generation == iPhoneGeneration.iPad4Gen ||
        //    iPhone.generation == iPhoneGeneration.iPadMini1Gen ||
        //    iPhone.generation == iPhoneGeneration.iPadUnknown)
        //{
        //    iPadResolution = true;
        //}
		
        //if(iPhone.generation == iPhoneGeneration.Unknown)
        //{
        //    if(SystemInfo.deviceModel.ToLower().Contains("ipad"))
        //    {
        //        iPadResolution = true;
        //    }
        //}
		
#endif

        if (Application.platform == RuntimePlatform.OSXEditor ||
            Application.platform == RuntimePlatform.WindowsEditor)
        {
            float _r43 = 4.0f / 3.0f;
            if (Mathf.Abs((float)(Screen.width) / (float)(Screen.height) - _r43) < 0.01f)
            {
                iPadResolution = true;
            }
        }

        return iPadResolution;
    }

    public static string LoadStringFromFile(string path)
    {
        string final_path = GameHelper.StoragePath + path;
        string Str = "";
        StreamReader Reader = null;
        try
        {
            Reader = new StreamReader(final_path);
            Str = Reader.ReadToEnd();
            Reader.Close();
        }
        catch (System.Exception)
        {
            LogWrapper.LogError("Load File Failed #" + path);
        }
        return Str;
    }

    public static string GetMD5(byte[] bytValue)
    {
        byte[] bytHash = GetMD5Byte(bytValue);
        if (null == bytHash) return "";

        string sTemp = "";
        for (int i = 0; i < bytHash.Length; i++)
        {
            sTemp += bytHash[i].ToString("X").PadLeft(2, '0');
        }
        return sTemp.ToLower();
    }

    private const int MD5_SIZE = 16;
    private const int HEADER_SIZE = 2;
    private static byte[] HEADER_BYTE = new byte[] { 0x00, 0xff };
    public static byte[] GetMD5Byte(byte[] bytValue)
    {
        MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
        byte[] bytHash = md5.ComputeHash(bytValue);
        md5.Clear();
        return bytHash;
    }

    public static byte[] Encrypt(byte[] bytValue)
    {
        byte[] tmp = new byte[HEADER_SIZE + MD5_SIZE + bytValue.Length];
        byte[] md5 = GetMD5Byte(bytValue);
        if(null == md5)
        {
            LogWrapper.LogError("GetMD5Byte为null，操作失败！！");
            return null;
        }

        for (int j = 0; j < HEADER_SIZE; ++j)
        {
            tmp[j] = HEADER_BYTE[j];
        }

        for (int j = 0; j < MD5_SIZE; ++j)
        {
            tmp[j + HEADER_SIZE] = md5[j];
        }

        for (int i = 0; i < bytValue.Length; i += MD5_SIZE)
        {
            for (int j = 0; (j < MD5_SIZE) && (i + j < bytValue.Length); ++j)
            {
                tmp[i + j + HEADER_SIZE + MD5_SIZE] = Convert.ToByte(md5[j] ^ bytValue[i + j]);
            }
            md5 = GetMD5Byte(md5);
            if(null == md5)
            {
                LogWrapper.LogError("GetMD5Byte返回null，操作失败！！");
                return null;
            }
        }

        return tmp;
    }

    public static byte[] Decrypt(byte[] bytValue)
    {
        for (int j = 0; j < HEADER_SIZE; ++j)
        {
            if (bytValue[j] != HEADER_BYTE[j])
            {
                return bytValue;
            }
        }

        byte[] tmp = new byte[bytValue.Length - MD5_SIZE - HEADER_SIZE];
        byte[] md5 = new byte[MD5_SIZE];

        for (int j = 0; j < MD5_SIZE; ++j)
        {
            md5[j] = bytValue[j + HEADER_SIZE];
        }

        for (int i = 0; i < tmp.Length; i += MD5_SIZE)
        {
            for (int j = 0; (j < MD5_SIZE) && (i + j < tmp.Length); ++j)
            {
                tmp[i + j] = Convert.ToByte(md5[j] ^ bytValue[i + j + HEADER_SIZE + MD5_SIZE]);
            }
            md5 = GetMD5Byte(md5);
            if(null== md5)
            {
                LogWrapper.LogError("GetMD5Byte返回null，操作失败！！");
                return null;
            }
        }

        return tmp;
    }

    public static void CopyStream(System.IO.Stream input, System.IO.Stream output)
    {
        byte[] buffer = new byte[2000];
        int len;
        while ((len = input.Read(buffer, 0, 2000)) > 0)
        {
            output.Write(buffer, 0, len);
        }
        output.Flush();
    }

#if !CONSOLE_CLIENT
    public static void ClearUsedTextures(List<UITexture> texs)
    {
        foreach (UITexture item in texs)
        {
            if (item.mainTexture != null)
            {
                item.mainTexture = null;
            }
        }
        texs.Clear();
        Resources.UnloadUnusedAssets();
    }
#endif

    static Encoding sGBEncoding;
    static Encoding GBEncoding
    {
        get
        {
            if (sGBEncoding == null)
                sGBEncoding = NewEncoding();
            return sGBEncoding;
        }
    }
    static Encoding NewEncoding()
    {
        int[] page = {54936, 20936, 936};
        for (int i = 0; i < page.Length; ++i)
        {
            try
            {
                var coding = GB18030Encoding.GetEncoding(page[i]);
                if (null == coding) continue;

                LogWrapper.LogInfo("NewEncoding code page ", page[i], " found");
                return coding;
            }
            catch (Exception e)
            {
                LogWrapper.LogError("Code page ", page[i], " not supported. Message: ", e.Message);
            }
        }

        return null;
    }

    /// <summary>
    /// CSV文件进行转码
    /// </summary>
    /// <param name="original"></param>
    /// <returns></returns>
    private static GB18030.GB18030Decoder decoder = new GB18030.GB18030Decoder();
    static public byte[] CookCsvBytes(byte[] original)
    {
        if (null == original) { return new byte[1]; }

        try
        {
            return Encoding.UTF8.GetBytes(decoder.GetChars(original, 0, original.Length));
        }
        catch (Exception e)
        {
            LogWrapper.LogError("CookCsvBytes: Use build-in coder failed. try I18N. Exception:", e.Message);
            return Encoding.Convert(GBEncoding, Encoding.UTF8, original);
        }
    }

    /// <summary>
    /// 使用AB在编辑器会有Shader丢失的情况，用这个方法进行修复
    /// </summary>
    /// <param name="go">被修复的GameObject</param>
    static public void FixShader(GameObject go)
    {
#if UNITY_EDITOR && USE_TPS_RESOURCES
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        foreach (var renderer in renderers)
        {
            if (renderer == null)
                continue;
            else
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (mat == null)
                    {
                        continue;
                    }
                    if (mat.shader == null)
                    {
                        continue;
                    }
                    mat.shader = Shader.Find(mat.shader.name);
                    //if (string.IsNullOrEmpty(mat.shader.name))
                    //{
                    //}
                    //else
                    //{
                    //    if (mat.shader.name.Contains("H3D"))
                    //    {
                    //        LogWrapper.LogWarning("Fixing [" + mat.shader.name + "]");
                    //    }
                    //    else
                    //        LogWrapper.Log("Fixing [" + mat.shader.name + "]");
                    //}
                }
            }
        }
#endif
    }
}