using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using ArenaGame.Client;

namespace ArenaGame.Editor
{
    /// <summary>
    /// Tool to configure FBX characters as Humanoid rigs and assign them to hero configs
    /// </summary>
    public static class HeroCharacterSetup
    {
        [MenuItem("Tools/Setup/Configure Hero Characters")]
        public static void ConfigureHeroCharacters()
        {
            Debug.Log("[HeroCharacterSetup] Starting hero character setup...");
            
            // Mapping of hero types to character FBX files
            var heroCharacterMap = new[]
            {
                new { heroType = "Archer", characterName = "Elf", prefabName = "Hero_Archer" },
                new { heroType = "IceArcher", characterName = "Elf", prefabName = "Hero_IceArcher" },
                new { heroType = "Mage", characterName = "Wizard", prefabName = "Hero_Mage" },
                new { heroType = "Warrior", characterName = "Knight_Male", prefabName = "Hero_Warrior" },
                new { heroType = "DefaultHero", characterName = "Casual_Male", prefabName = "Hero_DefaultHero" },
                new { heroType = "FastHero", characterName = "Ninja_Male", prefabName = "Hero_FastHero" },
                new { heroType = "TankHero", characterName = "Knight_Golden_Male", prefabName = "Hero_TankHero" },
            };
            
            string fbxDir = "Assets/Characters/FBX";
            string prefabDir = "Assets/Resources/HeroConfigs";
            
            // Ensure prefab directory exists
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "HeroConfigs");
            }
            
            int configuredCount = 0;
            int prefabCount = 0;
            
            foreach (var mapping in heroCharacterMap)
            {
                // Find FBX file
                string fbxPath = $"{fbxDir}/{mapping.characterName}.fbx";
                ModelImporter importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
                
                if (importer == null)
                {
                    Debug.LogWarning($"[HeroCharacterSetup] FBX not found: {fbxPath}");
                    continue;
                }
                
                // Configure as Humanoid
                bool needsReimport = false;
                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    needsReimport = true;
                    Debug.Log($"[HeroCharacterSetup] Configured {mapping.characterName}.fbx as Humanoid");
                }
                
                // Set avatar generation options
                if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    needsReimport = true;
                }
                
                // Optimize mesh
                importer.optimizeMesh = true;
                importer.optimizeMeshVertices = true;
                importer.optimizeMeshPolygons = true;
                
                if (needsReimport)
                {
                    AssetDatabase.ImportAsset(fbxPath, ImportAssetOptions.ForceUpdate);
                    Debug.Log($"[HeroCharacterSetup] Reimported {mapping.characterName}.fbx");
                }
                
                configuredCount++;
                
                // Create prefab
                string prefabPath = $"{prefabDir}/{mapping.prefabName}.prefab";
                GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                
                if (existingPrefab == null)
                {
                    // Load the FBX model as a GameObject asset (the root of the FBX)
                    GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
                    if (fbxModel != null)
                    {
                        // For FBX models, we need to create the prefab in a scene context
                        // Create a temporary scene object
                        GameObject tempObject = new GameObject("Temp_" + mapping.prefabName);
                        GameObject instance = PrefabUtility.InstantiatePrefab(fbxModel, tempObject.transform) as GameObject;
                        
                        if (instance != null)
                        {
                            // Reset transform
                            instance.transform.localPosition = Vector3.zero;
                            instance.transform.localRotation = Quaternion.identity;
                            instance.transform.localScale = Vector3.one;
                            instance.name = mapping.prefabName;
                            
                            // Unparent from temp object
                            instance.transform.SetParent(null);
                            
                            // Ensure Animator component exists
                            if (instance.GetComponent<Animator>() == null)
                            {
                                instance.AddComponent<Animator>();
                            }
                            
                            // Save as prefab
                            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                            
                            // Clean up
                            Object.DestroyImmediate(instance);
                            Object.DestroyImmediate(tempObject);
                            
                            if (prefab != null)
                            {
                                Debug.Log($"[HeroCharacterSetup] Created prefab: {mapping.prefabName}.prefab");
                                prefabCount++;
                            }
                            else
                            {
                                Debug.LogWarning($"[HeroCharacterSetup] Failed to create prefab: {mapping.prefabName}");
                            }
                        }
                        else
                        {
                            Object.DestroyImmediate(tempObject);
                            Debug.LogWarning($"[HeroCharacterSetup] Could not instantiate FBX model: {fbxPath}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"[HeroCharacterSetup] Could not load FBX as GameObject: {fbxPath}");
                    }
                }
                else
                {
                    Debug.Log($"[HeroCharacterSetup] Prefab already exists: {mapping.prefabName}.prefab");
                }
                
                // Assign to hero config
                string configPath = $"{prefabDir}/{mapping.heroType}_Hero.asset";
                HeroConfigSO config = AssetDatabase.LoadAssetAtPath<HeroConfigSO>(configPath);
                
                if (config != null)
                {
                    GameObject prefabToAssign = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefabToAssign != null)
                    {
                        config.heroPrefab = prefabToAssign;
                        EditorUtility.SetDirty(config);
                        Debug.Log($"[HeroCharacterSetup] Assigned {mapping.prefabName} to {mapping.heroType} config");
                    }
                }
                else
                {
                    Debug.LogWarning($"[HeroCharacterSetup] Hero config not found: {configPath}");
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[HeroCharacterSetup] ✓ Configured {configuredCount} FBX files, created {prefabCount} prefabs!");
        }
        
        [MenuItem("Tools/Setup/Configure All FBX as Humanoid")]
        public static void ConfigureAllFBXAsHumanoid()
        {
            Debug.Log("[HeroCharacterSetup] Configuring all FBX files as Humanoid...");
            
            string fbxDir = "Assets/Characters/FBX";
            string[] fbxFiles = Directory.GetFiles(fbxDir, "*.fbx", SearchOption.TopDirectoryOnly);
            
            int configuredCount = 0;
            
            foreach (string fbxFile in fbxFiles)
            {
                string relativePath = fbxFile.Replace('\\', '/');
                if (!relativePath.StartsWith("Assets/"))
                {
                    relativePath = "Assets/" + relativePath.Substring(relativePath.IndexOf("Assets/") + 7);
                }
                
                ModelImporter importer = AssetImporter.GetAtPath(relativePath) as ModelImporter;
                if (importer == null) continue;
                
                bool needsReimport = false;
                
                // Configure as Humanoid
                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    needsReimport = true;
                }
                
                // Set avatar generation
                if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
                {
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    needsReimport = true;
                }
                
                // Optimize mesh
                importer.optimizeMesh = true;
                importer.optimizeMeshVertices = true;
                importer.optimizeMeshPolygons = true;
                
                if (needsReimport)
                {
                    AssetDatabase.ImportAsset(relativePath, ImportAssetOptions.ForceUpdate);
                    configuredCount++;
                    Debug.Log($"[HeroCharacterSetup] Configured: {Path.GetFileName(relativePath)}");
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[HeroCharacterSetup] ✓ Configured {configuredCount} FBX files as Humanoid!");
        }
    }
}

