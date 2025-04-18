using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AssetRelationBrowser : EditorWindow
{

    private static readonly string[] CheckPaths = new string[]
    {
        "Assets"
    };

    private static readonly string[] CheckDependenceTypes = new string[]
    {
        "prefab",
        "scene"
    };

    private static AssetRelationBrowser instance;
    Vector2 scrollPositionReference = Vector2.zero;
    Vector2 scrollPositionDependece = Vector2.zero;

    private string selectionPath;
    private Dictionary<Type, List<string>> referenceMap;
    private Dictionary<Type, List<string>> dependenceMap;

    void OnEnable() { instance = this; }
    void OnDisable() { instance = null; }

    private static void Open()
    {
        instance = instance ? instance : GetWindow<AssetRelationBrowser>("AssetRelationBrowser");
        instance.Show();
        instance.Focus();
    }


    [MenuItem("Assets/AssetRefDeps/Browse Relation", false, 0)]
    private static void BrowseRelation()
    {
        Open();
        instance.dependenceMap = GetDependence();
        instance.referenceMap = GetReference();
    }

    //获取引用选择的资源的资源
    private static Dictionary<Type, List<string>> GetDependence()
    {
        if (CheckDependenceTypes == null || CheckDependenceTypes.Length == 0)
        {
            return null;
        }

        StringBuilder stringBuilder = new StringBuilder();
        foreach (var item in CheckDependenceTypes)
        {
            stringBuilder.Append("t:");
            stringBuilder.Append(item);
            stringBuilder.Append(" ");
        }
        string checkType = stringBuilder.ToString().TrimEnd();

        string[] guids = AssetDatabase.FindAssets(checkType, CheckPaths);
        if (guids == null || guids.Length == 0)
        {
            return null;
        }
        Dictionary<Type, List<string>> dependenceMap = new Dictionary<Type, List<string>>();
        string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());

        int i = 0;
        ShowProgress(0, 0, 0);
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath) == false)
            {
                var paths = AssetDatabase.GetDependencies(new string[] { assetPath });
                if (paths != null && paths.Length > 0)
                {
                    foreach (var path in paths)
                    {
                        if (path.Equals(selectionPath) == true)
                        {
                            var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                            if (dependenceMap.ContainsKey(assetType) == false || dependenceMap[assetType] == null)
                            {
                                dependenceMap[assetType] = new List<string>();
                            }
                            if (dependenceMap[assetType].Contains(assetPath) == false && selectionPath != assetPath)
                            {
                                dependenceMap[assetType].Add(assetPath);
                            }
                            break;
                        }
                    }
                }
            }
            i++;
            ShowProgress((float)i / (float)guids.Length, guids.Length, i);
        }
        EditorUtility.ClearProgressBar();
        instance.selectionPath = selectionPath;
        return dependenceMap;
    }

    //获取选择的资源引用的资源
    private static Dictionary<Type, List<string>> GetReference()
    {
        string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
        string[] paths = AssetDatabase.GetDependencies(new string[] { selectionPath });
        if (paths == null || paths.Length == 0)
        {
            return null;
        }
        Dictionary<Type, List<string>> referenceMap = new Dictionary<Type, List<string>>();
        int i = 0;
        ShowProgress(0, 0, 0);
        foreach (var path in paths)
        {
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
            if (referenceMap.ContainsKey(assetType) == false || referenceMap[assetType] == null)
            {
                referenceMap[assetType] = new List<string>();
            }
            if (referenceMap[assetType].Contains(path) == false && selectionPath != path)
            {
                referenceMap[assetType].Add(path);
            }
            i++;
            ShowProgress((float)i / (float)paths.Length, paths.Length, i);
        }
        EditorUtility.ClearProgressBar();
        AssetRelationBrowser.instance.selectionPath = selectionPath;
        return referenceMap;
    }

    private void Draw(Dictionary<Type, List<string>> map, ref Vector2 scrollPosition)
    {
        if (map == null || map.Count == 0)
        {
            return;
        }
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        EditorGUI.indentLevel++;
        foreach (var item in map)
        {
            var type = item.Key;
            var paths = item.Value;
            if (paths != null && paths.Count > 0)
            {
                if (DrawHeader(type.Name.ToString()) == true)
                {
                    EditorGUI.indentLevel++;
                    foreach (var path in paths)
                    {
                        var asset = AssetDatabase.LoadAssetAtPath(path, type);
                        if (asset != null)
                        {
                            EditorGUILayout.ObjectField(asset, type, false);
                        }
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }
        EditorGUI.indentLevel--;
        GUILayout.EndScrollView();
    }
    void OnGUI()
    {
        if (string.IsNullOrEmpty(selectionPath))
        {
            Debug.LogError("AssetRelationBrowser OnGUI selectionPath is null");
            return;
        }
        var asset = AssetDatabase.LoadMainAssetAtPath(selectionPath);
        if (asset is null)
        {
            Debug.LogError($"AssetRelationBrowser OnGUI asset is null, selectionPath is {selectionPath}");
            return;
        }
        
        EditorGUILayout.ObjectField("Selection", asset, asset.GetType(), false);
        if (DrawHeader("Dependece"))
        {
            Draw(instance.dependenceMap, ref scrollPositionDependece);
        }
        if (DrawHeader("Reference"))
        {
            Draw(instance.referenceMap, ref scrollPositionReference);
        }
        GUILayout.FlexibleSpace();
    }

    private bool DrawHeader(string text)
    {
        return DrawHeader(text, text, true, false);
    }
    
    private bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        if (!minimalistic) GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        if (minimalistic)
        {
            if (state) text = "\u25BC" + (char)0x200a + text;
            else text = "\u25BA" + (char)0x200a + text;

            GUILayout.BeginHorizontal();
            GUI.contentColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.7f) : new Color(0f, 0f, 0f, 0.7f);
            if (!GUILayout.Toggle(true, text, "PreToolbar2", GUILayout.MinWidth(20f))) state = !state;
            GUI.contentColor = Color.white;
            GUILayout.EndHorizontal();
        }
        else
        {
            text = "<b><size=11>" + text + "</size></b>";
            if (state) text = "\u25BC " + text;
            else text = "\u25BA " + text;
            GUILayout.Space(EditorGUI.indentLevel * 20.0f);
            if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;
        }

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        if (!minimalistic) GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }

    /// <summary>
    /// 显示进度条
    /// </summary>
    private static void ShowProgress(float progress, int total, int current)
    {
        EditorUtility.DisplayProgressBar("Searching", string.Format("Checking ({0}/{1}), please wait...", current, total), progress);
    }
}
