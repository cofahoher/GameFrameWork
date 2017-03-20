using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
//using UnityEngine;

public class XmlSerializerUtil
{
    public const string AssetResourcesDir = "Assets/resources/";
    public static void Serialize<T>(T o, string filePath)
    {
        try
        {
            XmlSerializer formatter = new XmlSerializer(o.GetType());
            StreamWriter sw = new StreamWriter(AssetResourcesDir + filePath, false);
            formatter.Serialize(sw, o);
            sw.Flush();
            sw.Close();

        }
        catch (Exception e)
        {
            LogWrapper.LogError(e.ToString());
        }

    }

    //public static T DeSerialize<T>(string filePath)
    //{
    //    try
    //    {
    //        XmlSerializer formatter = new XmlSerializer(typeof(T));
    //        TextAsset text = Resources.Load(filePath) as TextAsset;
    //        if (text == null)
    //        {
    //            LogWrapper.LogError("!");
    //        }
    //        MemoryStream ms = new MemoryStream(text.bytes);
    //        T o = (T)formatter.Deserialize(ms);
    //        ms.Close();
    //        return o;
    //    }
    //    catch (Exception e)
    //    {
    //        LogWrapper.LogError(e.ToString());
    //    }
    //    return default(T);
    //}
}

public class BinarySerializerUtil
{
    public static void Serialize<T>(T o, string filePath)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(XmlSerializerUtil.AssetResourcesDir + filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, o);
            stream.Flush();
            stream.Close();
        }
        catch (Exception e) 
        {
            LogWrapper.LogError(e.ToString());
        }
    }

    public static T DeSerialize<T>(string filePath)
    {
        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            Stream destream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            T o = (T)formatter.Deserialize(destream);
            destream.Flush();
            destream.Close();
            return o;
        }
        catch (Exception e)
        {
            LogWrapper.LogError(e.ToString());
        }
        return default(T);
    }
}