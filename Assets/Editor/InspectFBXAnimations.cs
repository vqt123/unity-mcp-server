using UnityEngine;
using UnityEditor;
using System.IO;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Tool to inspect what animations are in FBX files
    /// </summary>
    public static class InspectFBXAnimations
    {
        [MenuItem("Tools/Debug/Inspect FBX Animations")]
        public static void InspectAnimations()
        {
            Debug.Log("=== INSPECTING FBX ANIMATIONS ===\n");
            
            // First check the animation library
            string animationLibraryPath = "Assets/Characters/AnimationLibrary_Unity_Standard.fbx";
            Debug.Log($"\n=== ANIMATION LIBRARY ===");
            InspectFBXFile(animationLibraryPath);
            
            string fbxDir = "Assets/Characters/FBX";
            
            // Check the characters we're using
            string[] characterNames = { "Elf", "Wizard", "Knight_Male", "Casual_Male", "Ninja_Male", "Knight_Golden_Male" };
            
            foreach (string charName in characterNames)
            {
                string fbxPath = $"{fbxDir}/{charName}.fbx";
                InspectFBXFile(fbxPath);
            }
            
            Debug.Log("\n=== INSPECTION COMPLETE ===");
        }
        
        private static void InspectFBXFile(string fbxPath)
        {
            if (!File.Exists(fbxPath))
            {
                Debug.LogWarning($"[InspectFBX] File not found: {fbxPath}");
                return;
            }
            
            string fileName = Path.GetFileName(fbxPath);
            Debug.Log($"\n--- {fileName} ---");
            
            // Load all assets from the FBX
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            
            int animationCount = 0;
            int modelCount = 0;
            int materialCount = 0;
            int textureCount = 0;
            int avatarCount = 0;
            
            System.Collections.Generic.List<string> animationNames = new System.Collections.Generic.List<string>();
            
            foreach (Object asset in assets)
            {
                if (asset is AnimationClip clip)
                {
                    animationCount++;
                    animationNames.Add(clip.name);
                    Debug.Log($"  Animation: {clip.name} (Length: {clip.length:F2}s, FrameRate: {clip.frameRate:F2})");
                    
                    // Check if it loops
                    AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                    Debug.Log($"    - Loop: {settings.loopTime}, Wrap Mode: {clip.wrapMode}");
                }
                else if (asset is GameObject)
                {
                    modelCount++;
                    Debug.Log($"  Model: {asset.name}");
                }
                else if (asset is Material)
                {
                    materialCount++;
                    Debug.Log($"  Material: {asset.name}");
                }
                else if (asset is Texture2D)
                {
                    textureCount++;
                }
                else if (asset is Avatar)
                {
                    avatarCount++;
                    Debug.Log($"  Avatar: {asset.name}");
                }
                else
                {
                    Debug.Log($"  Other: {asset.name} ({asset.GetType().Name})");
                }
            }
            
            Debug.Log($"  Summary: {animationCount} animations, {modelCount} models, {avatarCount} avatars, {materialCount} materials, {textureCount} textures");
            
            if (animationNames.Count > 0)
            {
                Debug.Log($"  Animation List: {string.Join(", ", animationNames)}");
            }
            
            // Check ModelImporter settings
            ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            if (importer != null)
            {
                Debug.Log($"  Import Settings:");
                Debug.Log($"    - Animation Type: {importer.animationType}");
                Debug.Log($"    - Avatar Setup: {importer.avatarSetup}");
                Debug.Log($"    - Import Animation: {importer.importAnimation}");
                
                if (importer.importAnimation)
                {
                    Debug.Log($"    - Animation Clips: {importer.defaultClipAnimations.Length}");
                    foreach (var clipInfo in importer.defaultClipAnimations)
                    {
                        Debug.Log($"      - Clip: {clipInfo.name} (take: {clipInfo.takeName})");
                    }
                }
            }
        }
        
        [MenuItem("Tools/Debug/List All FBX Animations")]
        public static void ListAllAnimations()
        {
            Debug.Log("=== ALL FBX ANIMATIONS ===\n");
            
            string fbxDir = "Assets/Characters/FBX";
            string[] fbxFiles = Directory.GetFiles(fbxDir, "*.fbx", SearchOption.TopDirectoryOnly);
            
            foreach (string fbxFile in fbxFiles)
            {
                string relativePath = fbxFile.Replace('\\', '/');
                if (!relativePath.StartsWith("Assets/"))
                {
                    relativePath = "Assets/" + relativePath.Substring(relativePath.IndexOf("Assets/") + 7);
                }
                
                string fileName = Path.GetFileNameWithoutExtension(relativePath);
                
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(relativePath);
                System.Collections.Generic.List<string> animations = new System.Collections.Generic.List<string>();
                
                foreach (Object asset in assets)
                {
                    if (asset is AnimationClip clip)
                    {
                        animations.Add(clip.name);
                    }
                }
                
                if (animations.Count > 0)
                {
                    Debug.Log($"{fileName}: {animations.Count} animation(s)");
                    foreach (string animName in animations)
                    {
                        Debug.Log($"  - {animName}");
                    }
                }
                else
                {
                    Debug.Log($"{fileName}: No animations");
                }
            }
            
            Debug.Log("\n=== LIST COMPLETE ===");
        }
    }
}

