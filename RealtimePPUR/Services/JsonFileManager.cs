using System;
using System.Diagnostics;
using System.IO;

namespace RealtimePPUR.Services;

public static class JsonFileManager<T> where T : class
{
    public static void SerializeClass<T1>(T1 value, string filePath) where T1 : class
    {
        try
        {
            PrepareFileDirectory(filePath);
            var json = JsonManager.Serialize(value);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
    public static T1? DeserializeClass<T1>(string filePath) where T1 : class
    {
        try
        {
            if (!File.Exists(filePath)) return null;

            var json = File.ReadAllText(filePath);
            var result = JsonManager.Deserialize<T1>(json);

            if (result == null) return null;

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            return null;
        }
    }
    public static T? Load(string path)
    {
        return DeserializeClass<T>(path);
    }

    public static void Save(T data, string path)
    {
        SerializeClass(data, path);
    }

    private static void PrepareFileDirectory(string filePath)
    {
        string directory = Path.GetDirectoryName(filePath) ?? filePath;
        Directory.CreateDirectory(directory);
    }
}
