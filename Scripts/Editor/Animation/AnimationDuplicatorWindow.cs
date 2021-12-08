using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// FBX ���Ͽ� �����ϴ� �ִϸ��̼� ������ ���� ��λ� ����ų� ������ �����⤽
/// </summary>
public class AnimationDuplicatorWindow : EditorWindow
{    
    string targetFbxPath = "Assets/IcarusSource/Character";
    private Vector2 scrollPos;

    List<string> LogBuilder { get; set; } = new List<string>(1000);

    [MenuItem("ZGame/Tools/Animation Duplicator")]
    static void Init()
    {
        var window = (AnimationDuplicatorWindow)EditorWindow.GetWindow(typeof(AnimationDuplicatorWindow));
        window.titleContent = new GUIContent("AnimationDuplicatorWindow");
        window.minSize = new Vector2(450, 470);
    }

    void OnEnable()
    {
        LogBuilder.Clear();
    }

#if UNITY_EDITOR
    void OnGUI()
    {
        GUILayout.Space(10);

        EditorGUILayout.BeginHorizontal();
        {
            targetFbxPath = EditorGUILayout.TextField("�о�� FBX ���", targetFbxPath);
            if (GUILayout.Button("...", GUILayout.Width(40f)))
            {
                string path = EditorUtility.SaveFolderPanel("FBX�� �о�� ���� ����", "", "��������");
                if (!string.IsNullOrEmpty(path))
                {
                    targetFbxPath = path.Replace(Application.dataPath, "Assets");
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        {
            foreach (string msg in LogBuilder)
            {
                EditorGUILayout.LabelField(msg, ZGUIStyles.RichText);
            }
        }
        EditorGUILayout.EndScrollView();

        ZGUIStyles.Separator();

        if (GUILayout.Button("���� �۾� ����", GUILayout.Height(30)))
        {
            LogBuilder.Clear();

            DuplicateAnimationFromFBX(targetFbxPath);
        }
    }
#endif

    void DuplicateAnimationFromFBX(string targetFolderPath)
    {
        // *.anim ���� ��� ��� ã��
        List<string> animFilePaths = Directory.GetFiles(targetFolderPath, "*.anim", SearchOption.AllDirectories).ToList();

        string[] fbxFilePaths = Directory.GetFiles(targetFolderPath, "*.FBX", SearchOption.AllDirectories);

        for (int i = 0; i < fbxFilePaths.Length; ++i)
        {
            var fbxPath = fbxFilePaths[i];

            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            
            var clipObj = AssetDatabase.LoadAssetAtPath(fbxPath, typeof(AnimationClip)) as AnimationClip;
            if (null == clipObj)
                continue;
            AddLog($"�۾��� Fbx Path: {fbxPath}");

            string fileName = Path.GetFileNameWithoutExtension(fbxPath);
            int foundIndex = animFilePaths.FindIndex((s) => s.Contains(fileName));
            if (foundIndex != -1)
            {
                AddLog($"�ٲ� ���� ã�� : {animFilePaths[foundIndex]}", Color.white);
                CreateOrReplaceAsset(clipObj, animFilePaths[foundIndex]);
            }
            else
            {

            }

            Debug.LogWarning($"���� FBX�������� : {fbxPath}, ClipName: {clipObj.name}", clipObj);
            
            if (EditorUtility.DisplayCancelableProgressBar("�ִϸ��̼� ������ ������", $"{clipObj.name}", (float)i / (float)fbxFilePaths.Length))
            {
                break;
            }
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.SaveAssets();
    }

    T CreateOrReplaceAsset<T>(T asset, string path) where T : Object
    {
        T existingAsset = AssetDatabase.LoadAssetAtPath<T>(path);

        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(asset, path);
            existingAsset = asset;

            AddLog($"Animation ���� ���� : {path}", Color.blue);
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
            EditorUtility.SetDirty(existingAsset);
            AddLog($"����� �Ϸ� : {path}", Color.green);
        }

        return existingAsset;
    }

    void AddLog(string context, Color? color = null)
    {
        if (color.HasValue)
        {
            LogBuilder.Add($"<color=#{ColorUtility.ToHtmlStringRGB(color.Value)}>{context}</color>");
        }
        else
        {
            LogBuilder.Add(context);
        }
    }
}
