using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class FontReplacerForPrefab : EditorWindow
{
    private Font sourceFont;
    private Font targetFont;
    private bool includeInactive = true;
    private bool showAdvancedOptions = false;
    private bool replaceAllFonts = false;
    private string searchFilter = "t:Prefab";
    private Vector2 scrollPosition;
    private List<FontReplacementResult> replacementResults = new List<FontReplacementResult>();
    
    [System.Serializable]
    public class FontReplacementResult
    {
        public string prefabName;
        public string prefabPath;
        public int replacedCount;
        public List<string> replacedComponents = new List<string>();
    }

    [MenuItem("Tools/GuiTools/Fonts/字体替换工具")]
    public static void OpenFontReplacer()
    {
        GetWindow<FontReplacerForPrefab>("字体替换工具");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("字体替换工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("此工具可以批量替换预制体中的字体", MessageType.Info);

        GUILayout.Space(10);

        // 基本设置
        EditorGUILayout.LabelField("基本设置", EditorStyles.boldLabel);
        sourceFont = (Font)EditorGUILayout.ObjectField("被替换字体", sourceFont, typeof(Font), false);
        targetFont = (Font)EditorGUILayout.ObjectField("替换字体", targetFont, typeof(Font), false);
        
        GUILayout.Space(10);
        
        // 替换选项
        EditorGUILayout.LabelField("替换选项", EditorStyles.boldLabel);
        includeInactive = EditorGUILayout.Toggle("包含未激活对象", includeInactive);
        replaceAllFonts = EditorGUILayout.Toggle("替换所有字体", replaceAllFonts);
        
        if (replaceAllFonts)
        {
            EditorGUILayout.HelpBox("将替换所有字体，无论当前是什么字体", MessageType.Warning);
        }
        else
        {
            if (sourceFont == null)
            {
                EditorGUILayout.HelpBox("请选择被替换的字体", MessageType.Warning);
            }
        }

        GUILayout.Space(10);
        
        // 高级选项
        showAdvancedOptions = EditorGUILayout.Foldout(showAdvancedOptions, "高级选项", true);
        if (showAdvancedOptions)
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.LabelField("搜索过滤器");
            searchFilter = EditorGUILayout.TextField(searchFilter);
            EditorGUILayout.HelpBox("默认: t:Prefab (所有预制体)\n示例: t:Prefab Assets/UI (UI文件夹下的预制体)", MessageType.Info);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("批量操作");
            
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("替换预制体字体", GUILayout.Height(30)))
        {
            ReplaceFontsInPrefabs();
        }
        
        GUI.enabled = CanPreview();
        if (GUILayout.Button("预览影响", GUILayout.Height(30)))
        {
            PreviewFontReplacements();
        }
        GUI.enabled = true;
        
        if (GUILayout.Button("清除结果", GUILayout.Height(30)))
        {
            replacementResults.Clear();
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);
        
        // 批量操作按钮
        if (GUILayout.Button("扫描项目字体使用情况"))
        {
            ScanFontUsageInProject();
        }

        GUILayout.Space(20);

        // 结果显示
        if (replacementResults.Count > 0)
        {
            EditorGUILayout.LabelField($"替换结果 ({replacementResults.Count} 个预制体)", EditorStyles.boldLabel);
            
            int totalReplaced = replacementResults.Sum(r => r.replacedCount);
            EditorGUILayout.HelpBox($"总共替换了 {totalReplaced} 个字体组件", MessageType.Info);
            
            foreach (var result in replacementResults)
            {
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField($"{result.prefabName}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"({result.replacedCount} 个)", GUILayout.Width(60));
                
                EditorGUILayout.EndHorizontal();
                
                if (result.replacedCount > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (var component in result.replacedComponents)
                    {
                        EditorGUILayout.LabelField(component, EditorStyles.miniLabel);
                    }
                    EditorGUI.indentLevel--;
                }
                
                EditorGUILayout.EndVertical();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private bool CanPreview()
    {
        return !replaceAllFonts && sourceFont != null;
    }

    private void ReplaceFontsInPrefabs()
    {
        if (!replaceAllFonts && sourceFont == null)
        {
            Debug.LogError("请选择被替换的字体，或启用'替换所有字体'选项");
            return;
        }

        if (targetFont == null)
        {
            Debug.LogError("请选择替换字体");
            return;
        }

        replacementResults.Clear();
        
        string[] prefabGuids = AssetDatabase.FindAssets(searchFilter);
        int totalReplaced = 0;
        int processedCount = 0;

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            processedCount++;
            var result = ReplaceFontsInPrefab(prefab, path);
            if (result.replacedCount > 0)
            {
                replacementResults.Add(result);
                totalReplaced += result.replacedCount;
            }
            
            // 进度显示
            if (processedCount % 10 == 0)
            {
                EditorUtility.DisplayProgressBar("替换字体", $"处理中... ({processedCount}/{prefabGuids.Length})", 
                    (float)processedCount / prefabGuids.Length);
            }
        }
        
        EditorUtility.ClearProgressBar();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log($"字体替换完成！处理了 {processedCount} 个预制体，替换了 {totalReplaced} 个字体组件");
    }

    private FontReplacementResult ReplaceFontsInPrefab(GameObject prefab, string path)
    {
        var result = new FontReplacementResult
        {
            prefabName = prefab.name,
            prefabPath = path
        };

        bool hasChanges = false;
        Text[] textComponents = prefab.GetComponentsInChildren<Text>(includeInactive);

        foreach (Text textComponent in textComponents)
        {
            bool shouldReplace = replaceAllFonts || textComponent.font == sourceFont;
            
            if (shouldReplace && textComponent.font != targetFont)
            {
                textComponent.font = targetFont;
                hasChanges = true;
                result.replacedCount++;
                result.replacedComponents.Add($"{textComponent.name} ({textComponent.text})");
            }
        }

        if (hasChanges)
        {
            EditorUtility.SetDirty(prefab);
            PrefabUtility.SavePrefabAsset(prefab);
        }

        return result;
    }

    private void PreviewFontReplacements()
    {
        if (sourceFont == null)
        {
            Debug.LogError("请选择被替换的字体");
            return;
        }

        replacementResults.Clear();
        
        string[] prefabGuids = AssetDatabase.FindAssets(searchFilter);
        int totalAffected = 0;

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            var result = PreviewFontsInPrefab(prefab, path);
            if (result.replacedCount > 0)
            {
                replacementResults.Add(result);
                totalAffected += result.replacedCount;
            }
        }
        
        Debug.Log($"预览完成！{replacementResults.Count} 个预制体会受到影响，共 {totalAffected} 个字体组件");
    }

    private FontReplacementResult PreviewFontsInPrefab(GameObject prefab, string path)
    {
        var result = new FontReplacementResult
        {
            prefabName = prefab.name,
            prefabPath = path
        };

        Text[] textComponents = prefab.GetComponentsInChildren<Text>(includeInactive);

        foreach (Text textComponent in textComponents)
        {
            if (textComponent.font == sourceFont)
            {
                result.replacedCount++;
                result.replacedComponents.Add($"{textComponent.name} ({textComponent.text})");
            }
        }

        return result;
    }

    private void ScanFontUsageInProject()
    {
        Dictionary<Font, int> fontUsage = new Dictionary<Font, int>();
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string prefabGuid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            Text[] textComponents = prefab.GetComponentsInChildren<Text>(true);
            foreach (Text textComponent in textComponents)
            {
                Font font = textComponent.font;
                if (font != null)
                {
                    if (fontUsage.ContainsKey(font))
                    {
                        fontUsage[font]++;
                    }
                    else
                    {
                        fontUsage[font] = 1;
                    }
                }
            }
        }

        // 显示结果
        Debug.Log("=== 项目字体使用情况 ===");
        foreach (var kvp in fontUsage.OrderByDescending(x => x.Value))
        {
            Debug.Log($"字体: {kvp.Key?.name} - 使用次数: {kvp.Value}");
        }
        Debug.Log("=== 扫描完成 ===");
    }
}