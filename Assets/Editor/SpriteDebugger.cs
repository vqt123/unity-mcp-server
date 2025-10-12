using UnityEngine;
using UnityEditor;
using System.Linq;

public class SpriteDebugger
{
    [MenuItem("Tools/List GUI Sprites")]
    public static void ListGUISprites()
    {
        string path = "Assets/2D Casual UI/Sprite/GUI.png";
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        
        Debug.Log($"=== Found {assets.Length} assets at {path} ===");
        
        foreach (var asset in assets)
        {
            if (asset is Sprite sprite)
            {
                Debug.Log($"Sprite: {sprite.name} (Type: {asset.GetType().Name})");
            }
            else
            {
                Debug.Log($"Asset: {asset.name} (Type: {asset.GetType().Name})");
            }
        }
    }
}