using System.IO;
using Config;
using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public static class ConfigMenu
{
    public static void Load<T>() where T : ScriptableObject
    {
        var type       = typeof(T);
        var configPath = $"Assets/{type.Name}.asset";
        if (!File.Exists(configPath))
        {
            var obj = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(obj, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        var asset = AssetDatabase.LoadAssetAtPath(configPath, type);
        AssetDatabase.OpenAsset(asset);
    }

    [MenuItem("Config/GameCfg")]
    public static void LoadGameCfg()
    {
        Load<GameCfg>();
    }
}