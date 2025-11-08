using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.Audio;

namespace GuiFramework
{
    public class EnhancedAudioEditor : EditorWindow
    {
        private AudioDatas targetAudioDatas;
        private SceneAudioConfig targetSceneConfig;
        private string searchPath = "Assets/";
        private Vector2 scrollPosition;
        private bool includeSubfolders = true;
        private string[] audioExtensions = new string[] { ".wav", ".mp3", ".ogg", ".aiff" };
        
        // 标签页支持
        private int selectedTab = 0;
        private readonly string[] tabNames = { "Audio Database", "Scene Config", "Quick Tools" };
        
        // 场景配置相关
        private string newSceneName = "";
        private List<AudioData> availableBGM = new List<AudioData>();
        private List<AudioData> availableSFX = new List<AudioData>();
        
        [MenuItem("Tools/GuiTools/Configurator/Enhanced Audio Editor")]
        public static void ShowWindow()
        {
            GetWindow<EnhancedAudioEditor>("Enhanced Audio Editor");
        }
    
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Enhanced Audio Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 标签页选择
            selectedTab = GUILayout.Toolbar(selectedTab, tabNames);
            EditorGUILayout.Space();
            
            switch (selectedTab)
            {
                case 0:
                    DrawAudioDatabaseTab();
                    break;
                case 1:
                    DrawSceneConfigTab();
                    break;
                case 2:
                    DrawQuickToolsTab();
                    break;
            }
        }
        
        #region Audio Database Tab
        
        private void DrawAudioDatabaseTab()
        {
            EditorGUILayout.LabelField("Audio Database Management", EditorStyles.boldLabel);
            EditorGUILayout.Space();
    
            targetAudioDatas = (AudioDatas)EditorGUILayout.ObjectField("Audio Database", targetAudioDatas, typeof(AudioDatas), false);
    
            if (targetAudioDatas == null)
            {
                EditorGUILayout.HelpBox("Please assign an AudioDatas asset first.", MessageType.Warning);
                
                // 提供创建新数据库的选项
                if (GUILayout.Button("Create New Audio Database"))
                {
                    CreateNewAudioDatabase();
                }
                return;
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
            
            // 搜索路径设置
            EditorGUILayout.BeginHorizontal();
            searchPath = EditorGUILayout.TextField("Search Path", searchPath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel("Select Folder to Search", searchPath, "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    searchPath = "Assets" + newPath.Replace(Application.dataPath, "");
                }
            }
            EditorGUILayout.EndHorizontal();
    
            includeSubfolders = EditorGUILayout.Toggle("Include Subfolders", includeSubfolders);
    
            EditorGUILayout.Space();
            
            // 批量操作按钮
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Scan for Audio Files", GUILayout.Height(30)))
            {
                ScanForAudioFiles();
            }
            
            if (GUILayout.Button("Add Single Audio", GUILayout.Height(30)))
            {
                AddSingleAudioFile();
            }
            EditorGUILayout.EndHorizontal();
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Data List", EditorStyles.boldLabel);
            
            // 显示当前音频数据
            if (targetAudioDatas.audioDataList != null && targetAudioDatas.audioDataList.Count > 0)
            {
                DrawAudioDataList();
            }
            else
            {
                EditorGUILayout.HelpBox("No audio data configured. Click 'Scan for Audio Files' to populate.", MessageType.Info);
            }
        }
        
        private void DrawAudioDataList()
        {
            // 搜索和过滤
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Filter:", GUILayout.Width(40));
            string searchFilter = EditorGUILayout.TextField("", GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            
            // 统计信息
            EditorGUILayout.LabelField($"Total: {targetAudioDatas.audioDataList.Count} audio files", EditorStyles.miniLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            int displayedCount = 0;
            for (int i = 0; i < targetAudioDatas.audioDataList.Count; i++)
            {
                var audioData = targetAudioDatas.audioDataList[i];
                
                // 应用搜索过滤
                if (!string.IsNullOrEmpty(searchFilter) && 
                    !audioData.audioName.ToLower().Contains(searchFilter.ToLower()) &&
                    !audioData.audioPath.ToLower().Contains(searchFilter.ToLower()))
                {
                    continue;
                }
    
                displayedCount++;
                
                EditorGUILayout.BeginVertical("box");
                
                // 音频信息
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Name:", GUILayout.Width(40));
                audioData.audioName = EditorGUILayout.TextField(audioData.audioName);
                EditorGUILayout.EndHorizontal();
    
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Path:", GUILayout.Width(40));
                EditorGUILayout.SelectableLabel(audioData.audioPath, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                EditorGUILayout.EndHorizontal();
                
                // 预览和测试
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play", GUILayout.Width(50)))
                {
                    PlayAudioPreview(audioData.audioPath);
                }
                
                if (GUILayout.Button("Test in Game", GUILayout.Width(80)))
                {
                    TestAudioInGame(audioData.audioName);
                }
                
                GUILayout.FlexibleSpace();
                
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    if (EditorUtility.DisplayDialog("Remove Audio Data", 
                        $"Are you sure you want to remove '{audioData.audioName}'?", "Yes", "No"))
                    {
                        targetAudioDatas.audioDataList.RemoveAt(i);
                        SaveChanges();
                        break;
                    }
                }
                EditorGUILayout.EndHorizontal();
    
                EditorGUILayout.EndVertical();
            }
    
            if (displayedCount == 0 && !string.IsNullOrEmpty(searchFilter))
            {
                EditorGUILayout.HelpBox("No items match your search filter.", MessageType.Info);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 操作按钮
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Clear All Data", 
                    "Are you sure you want to clear all audio data?", "Yes", "No"))
                {
                    targetAudioDatas.audioDataList.Clear();
                    SaveChanges();
                }
            }
            
            if (GUILayout.Button("Save Changes", GUILayout.Height(25)))
            {
                SaveChanges();
            }
            EditorGUILayout.EndHorizontal();
        }
        
        #endregion
        
        #region Scene Config Tab
        
        private void DrawSceneConfigTab()
        {
            EditorGUILayout.LabelField("Scene Audio Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            targetSceneConfig = (SceneAudioConfig)EditorGUILayout.ObjectField("Scene Config", targetSceneConfig, typeof(SceneAudioConfig), false);
            
            if (targetSceneConfig == null)
            {
                EditorGUILayout.HelpBox("Please assign or create a SceneAudioConfig asset.", MessageType.Warning);
                
                EditorGUILayout.BeginHorizontal();
                newSceneName = EditorGUILayout.TextField("New Scene Name", newSceneName);
                if (GUILayout.Button("Create New", GUILayout.Width(80)))
                {
                    CreateNewSceneConfig();
                }
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            // 刷新可用音频列表
            if (targetAudioDatas != null && targetAudioDatas.audioDataList != null)
            {
                availableBGM = targetAudioDatas.audioDataList
                    .Where(a => a.audioPath.ToLower().Contains("bgm") || a.audioName.ToLower().Contains("bgm"))
                    .ToList();
                availableSFX = targetAudioDatas.audioDataList
                    .Where(a => !a.audioPath.ToLower().Contains("bgm") && !a.audioName.ToLower().Contains("bgm"))
                    .ToList();
            }
            
            EditorGUILayout.Space();
            
            // 场景配置编辑
            targetSceneConfig.sceneName = EditorGUILayout.TextField("Scene Name", targetSceneConfig.sceneName);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Background Music", EditorStyles.boldLabel);
            
            // 默认BGM选择
            int selectedBGMIndex = availableBGM.FindIndex(a => a == targetSceneConfig.defaultBGM);
            string[] bgmOptions = availableBGM.Select(a => a.audioName).ToArray();
            int newBGMIndex = EditorGUILayout.Popup("Default BGM", selectedBGMIndex, bgmOptions);
            if (newBGMIndex >= 0 && newBGMIndex < availableBGM.Count)
            {
                targetSceneConfig.defaultBGM = availableBGM[newBGMIndex];
            }
            
            // 备用BGM列表
            EditorGUILayout.LabelField("Alternative BGM:");
            for (int i = 0; i < targetSceneConfig.alternativeBGM.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int altIndex = availableBGM.FindIndex(a => a == targetSceneConfig.alternativeBGM[i]);
                int newAltIndex = EditorGUILayout.Popup($"BGM {i + 1}", altIndex, bgmOptions);
                if (newAltIndex >= 0 && newAltIndex < availableBGM.Count)
                {
                    targetSceneConfig.alternativeBGM[i] = availableBGM[newAltIndex];
                }
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    targetSceneConfig.alternativeBGM.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Alternative BGM"))
            {
                targetSceneConfig.alternativeBGM.Add(null);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene Sound Effects", EditorStyles.boldLabel);
            
            // 场景SFX列表
            for (int i = 0; i < targetSceneConfig.sceneSFX.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                int sfxIndex = availableSFX.FindIndex(a => a == targetSceneConfig.sceneSFX[i]);
                string[] sfxOptions = availableSFX.Select(a => a.audioName).ToArray();
                int newSFXIndex = EditorGUILayout.Popup($"SFX {i + 1}", sfxIndex, sfxOptions);
                if (newSFXIndex >= 0 && newSFXIndex < availableSFX.Count)
                {
                    targetSceneConfig.sceneSFX[i] = availableSFX[newSFXIndex];
                }
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    targetSceneConfig.sceneSFX.RemoveAt(i);
                    break;
                }
                EditorGUILayout.EndHorizontal();
            }
            
            if (GUILayout.Button("Add Scene SFX"))
            {
                targetSceneConfig.sceneSFX.Add(null);
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Audio Mix Settings", EditorStyles.boldLabel);
            
            targetSceneConfig.bgmVolume = EditorGUILayout.Slider("BGM Volume", targetSceneConfig.bgmVolume, 0f, 1f);
            targetSceneConfig.sfxVolume = EditorGUILayout.Slider("SFX Volume", targetSceneConfig.sfxVolume, 0f, 1f);
            
            targetSceneConfig.defaultSnapshot = (AudioMixerSnapshot)EditorGUILayout.ObjectField("Audio Snapshot", targetSceneConfig.defaultSnapshot, typeof(AudioMixerSnapshot), false);
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Save Scene Config", GUILayout.Height(30)))
            {
                EditorUtility.SetDirty(targetSceneConfig);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Save Complete", "Scene configuration saved successfully!", "OK");
            }
        }
        
        #endregion
        
        #region Quick Tools Tab
        
        private void DrawQuickToolsTab()
        {
            EditorGUILayout.LabelField("Quick Tools", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            EditorGUILayout.HelpBox("Quick tools for audio management and testing.", MessageType.Info);
            
            // 快速测试工具
            EditorGUILayout.LabelField("Audio Testing", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test All BGM", GUILayout.Height(25)))
            {
                TestAllBGM();
            }
            if (GUILayout.Button("Test Random SFX", GUILayout.Height(25)))
            {
                TestRandomSFX();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            
            // 批量操作工具
            EditorGUILayout.LabelField("Batch Operations", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Validate Audio Paths", GUILayout.Height(25)))
            {
                ValidateAudioPaths();
            }
            
            if (GUILayout.Button("Generate Scene Configs", GUILayout.Height(25)))
            {
                GenerateSceneConfigs();
            }
            
            if (GUILayout.Button("Update Audio Naming", GUILayout.Height(25)))
            {
                UpdateAudioNaming();
            }
            
            EditorGUILayout.Space();
            
            // 统计信息
            if (targetAudioDatas != null)
            {
                EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
                int bgmCount = targetAudioDatas.audioDataList.Count(a => 
                    a.audioName.ToLower().Contains("bgm") || a.audioPath.ToLower().Contains("bgm"));
                int sfxCount = targetAudioDatas.audioDataList.Count - bgmCount;
                
                EditorGUILayout.LabelField($"Total Audio Files: {targetAudioDatas.audioDataList.Count}");
                EditorGUILayout.LabelField($"BGM: {bgmCount}");
                EditorGUILayout.LabelField($"SFX: {sfxCount}");
            }
        }
        
        #endregion
        
        #region Core Methods
        
        private void ScanForAudioFiles()
        {
            if (targetAudioDatas == null) return;
    
            SearchOption searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            List<AudioData> foundAudioFiles = new List<AudioData>();
    
            int processedCount = 0;
            int validCount = 0;
    
            foreach (string extension in audioExtensions)
            {
                string[] audioFiles = Directory.GetFiles(searchPath, "*" + extension, searchOption);
                
                foreach (string filePath in audioFiles)
                {
                    string assetPath = filePath.Replace("\\", "/");
                    if (assetPath.EndsWith(".meta")) continue;
    
                    processedCount++;
                    
                    string resourcesRelativePath = GetResourcesRelativePath(assetPath);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);
                    
                    // 检查是否已存在
                    if (!targetAudioDatas.audioDataList.Any(a => a.audioPath == resourcesRelativePath))
                    {
                        AudioData audioData = new AudioData
                        {
                            audioName = fileName,
                            audioPath = resourcesRelativePath
                        };
                        
                        foundAudioFiles.Add(audioData);
                        validCount++;
                    }
                }
            }
    
            // 添加到现有列表（不覆盖）
            targetAudioDatas.audioDataList.AddRange(foundAudioFiles);
            SaveChanges();
    
            Debug.Log($"Scan completed! Added {validCount} new audio files.");
            EditorUtility.DisplayDialog("Scan Complete", 
                $"Added {validCount} new audio files", "OK");
        }
        
        private void AddSingleAudioFile()
        {
            string filePath = EditorUtility.OpenFilePanel("Select Audio File", searchPath, 
                "wav,mp3,ogg,aiff");
                
            if (!string.IsNullOrEmpty(filePath))
            {
                string assetPath = "Assets" + filePath.Replace(Application.dataPath, "");
                string resourcesRelativePath = GetResourcesRelativePath(assetPath);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);
                
                AudioData audioData = new AudioData
                {
                    audioName = fileName,
                    audioPath = resourcesRelativePath
                };
                
                targetAudioDatas.audioDataList.Add(audioData);
                SaveChanges();
                
                Debug.Log($"Added audio: {fileName}");
            }
        }
        
        private string GetResourcesRelativePath(string fullPath)
        {
            int resourcesIndex = fullPath.IndexOf("Resources/");
            if (resourcesIndex == -1)
            {
                Debug.LogWarning($"Audio file is not in a Resources folder: {fullPath}");
                if (fullPath.StartsWith("Assets/"))
                {
                    return fullPath.Substring("Assets/".Length).Replace(".meta", "");
                }
                return fullPath.Replace(".meta", "");
            }
    
            string relativePath = fullPath.Substring(resourcesIndex + "Resources/".Length);
            relativePath = relativePath.Replace(".wav", "")
                                      .Replace(".mp3", "")
                                      .Replace(".ogg", "")
                                      .Replace(".aiff", "")
                                      .Replace(".meta", "");
            
            return relativePath;
        }
        
        private void PlayAudioPreview(string audioPath)
        {
            AudioClip clip = Resources.Load<AudioClip>(audioPath);
            if (clip != null)
            {
                // 方法1: 使用 UnityEditor 内部的预览功能（如果可用）
#if UNITY_EDITOR
                try
                {
                    // 尝试使用 UnityEditor 的预览 API
                    System.Reflection.Assembly assembly = typeof(UnityEditor.Editor).Assembly;
                    System.Type audioUtilType = assembly.GetType("UnityEditor.AudioUtil");
                    System.Reflection.MethodInfo playPreviewClipMethod = audioUtilType.GetMethod("PlayPreviewClip", 
                        System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, 
                        null, 
                        new System.Type[] { typeof(AudioClip) }, 
                        null);
            
                    if (playPreviewClipMethod != null)
                    {
                        playPreviewClipMethod.Invoke(null, new object[] { clip });
                        Debug.Log($"Playing preview: {audioPath}");
                    }
                    else
                    {
                        Debug.LogWarning("Audio preview method not available in this Unity version");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not play audio preview: {e.Message}");
                }
#endif
            }
            else
            {
                Debug.LogWarning($"无法加载音频剪辑: {audioPath}");
            }
        }
        
        private void TestAudioInGame(string audioName)
        {
            if (Application.isPlaying)
            {
                AudioManager.Instance.PlaySfx(audioName);
            }
            else
            {
                Debug.Log("请在游戏运行时测试音频");
            }
        }
        
        private void SaveChanges()
        {
            if (targetAudioDatas != null)
            {
                EditorUtility.SetDirty(targetAudioDatas);
                AssetDatabase.SaveAssets();
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        private void CreateNewAudioDatabase()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create Audio Database", 
                "NewAudioDatabase", "asset", "Create new audio database");
                
            if (!string.IsNullOrEmpty(path))
            {
                AudioDatas newDatabase = CreateInstance<AudioDatas>();
                newDatabase.audioDataList = new List<AudioData>();
                AssetDatabase.CreateAsset(newDatabase, path);
                AssetDatabase.SaveAssets();
                targetAudioDatas = newDatabase;
            }
        }
        
        private void CreateNewSceneConfig()
        {
            if (string.IsNullOrEmpty(newSceneName))
            {
                EditorUtility.DisplayDialog("Error", "Please enter a scene name", "OK");
                return;
            }
            
            string path = EditorUtility.SaveFilePanelInProject("Create Scene Config", 
                $"SceneConfig_{newSceneName}", "asset", "Create new scene audio config");
                
            if (!string.IsNullOrEmpty(path))
            {
                SceneAudioConfig newConfig = CreateInstance<SceneAudioConfig>();
                newConfig.sceneName = newSceneName;
                newConfig.alternativeBGM = new List<AudioData>();
                newConfig.sceneSFX = new List<AudioData>();
                AssetDatabase.CreateAsset(newConfig, path);
                AssetDatabase.SaveAssets();
                targetSceneConfig = newConfig;
                newSceneName = "";
            }
        }
        
        private void TestAllBGM()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请在游戏运行时测试BGM");
                return;
            }
            
            if (targetAudioDatas == null) return;
            
            var bgmList = targetAudioDatas.audioDataList
                .Where(a => a.audioName.ToLower().Contains("bgm"))
                .ToList();
                
            Debug.Log($"Found {bgmList.Count} BGM files to test");
        }
        
        private void TestRandomSFX()
        {
            if (!Application.isPlaying) return;
            if (targetAudioDatas == null || targetAudioDatas.audioDataList.Count == 0) return;
            
            var sfxList = targetAudioDatas.audioDataList
                .Where(a => !a.audioName.ToLower().Contains("bgm"))
                .ToList();
                
            if (sfxList.Count > 0)
            {
                var randomSFX = sfxList[Random.Range(0, sfxList.Count)];
                AudioManager.Instance.PlaySfx(randomSFX.audioName);
                Debug.Log($"Playing random SFX: {randomSFX.audioName}");
            }
        }
        
        private void ValidateAudioPaths()
        {
            if (targetAudioDatas == null) return;
            
            int invalidCount = 0;
            foreach (var audioData in targetAudioDatas.audioDataList)
            {
                AudioClip clip = Resources.Load<AudioClip>(audioData.audioPath);
                if (clip == null)
                {
                    Debug.LogWarning($"Invalid audio path: {audioData.audioName} -> {audioData.audioPath}");
                    invalidCount++;
                }
            }
            
            if (invalidCount == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete", "All audio paths are valid!", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete", 
                    $"Found {invalidCount} invalid audio paths. Check console for details.", "OK");
            }
        }
        
        private void GenerateSceneConfigs()
        {
            // 自动为所有场景生成配置
            Debug.Log("Scene config generation feature");
        }
        
        private void UpdateAudioNaming()
        {
            // 批量更新音频命名
            if (targetAudioDatas == null) return;
            
            foreach (var audioData in targetAudioDatas.audioDataList)
            {
                // 简单的命名规范化
                audioData.audioName = audioData.audioName
                    .Replace("_", " ")
                    .Replace("-", " ")
                    .Trim();
            }
            
            SaveChanges();
            EditorUtility.DisplayDialog("Update Complete", "Audio naming updated successfully!", "OK");
        }
        
        #endregion
        
        // 右键菜单项
        [MenuItem("Assets/Create/Enhanced Audio/Scene Config", false, 100)]
        public static void CreateSceneConfigFromSelection()
        {
            string path = "Assets/SceneAudioConfig.asset";
            if (Selection.activeObject)
            {
                path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (Directory.Exists(path))
                {
                    path = Path.Combine(path, "SceneAudioConfig.asset");
                }
                else
                {
                    path = Path.GetDirectoryName(path) + "/SceneAudioConfig.asset";
                }
            }
            
            SceneAudioConfig newConfig = CreateInstance<SceneAudioConfig>();
            newConfig.sceneName = "NewScene";
            newConfig.alternativeBGM = new List<AudioData>();
            newConfig.sceneSFX = new List<AudioData>();
            
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = newConfig;
        }
    }
}
