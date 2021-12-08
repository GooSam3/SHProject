using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// FBX 파일에 존재하는 애니메이션 복제본 동일 경로상에 만들거나 기존거 덮어씌우기ㅍ
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
            targetFbxPath = EditorGUILayout.TextField("읽어올 FBX 경로", targetFbxPath);
            if (GUILayout.Button("...", GUILayout.Width(40f)))
            {
                string path = EditorUtility.SaveFolderPanel("FBX를 읽어올 폴더 설정", "", "폴더설정");
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

        if (GUILayout.Button("변경 작업 시작", GUILayout.Height(30)))
        {
            LogBuilder.Clear();

            DuplicateAnimationFromFBX(targetFbxPath);
        }
    }
#endif

    void DuplicateAnimationFromFBX(string targetFolderPath)
    {
        // *.anim 파일 경로 모두 찾기
        List<string> animFilePaths = Directory.GetFiles(targetFolderPath, "*.anim", SearchOption.AllDirectories).ToList();

        string[] fbxFilePaths = Directory.GetFiles(targetFolderPath, "*.FBX", SearchOption.AllDirectories);

        for (int i = 0; i < fbxFilePaths.Length; ++i)
        {
            var fbxPath = fbxFilePaths[i];

            var importer = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
            
            var clipObj = AssetDatabase.LoadAssetAtPath(fbxPath, typeof(AnimationClip)) as AnimationClip;
            if (null == clipObj)
                continue;
            AddLog($"작업할 Fbx Path: {fbxPath}");

            string fileName = Path.GetFileNameWithoutExtension(fbxPath);
            int foundIndex = animFilePaths.FindIndex((s) => s.Contains(fileName));
            if (foundIndex != -1)
            {
                AddLog($"바꿀 파일 찾음 : {animFilePaths[foundIndex]}", Color.white);
                CreateOrReplaceAsset(clipObj, animFilePaths[foundIndex]);
            }
            else
            {

            }

            Debug.LogWarning($"읽은 FBX파일정보 : {fbxPath}, ClipName: {clipObj.name}", clipObj);
            
            if (EditorUtility.DisplayCancelableProgressBar("애니메이션 복제본 생성중", $"{clipObj.name}", (float)i / (float)fbxFilePaths.Length))
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

            AddLog($"Animation 파일 생성 : {path}", Color.blue);
        }
        else
        {
            EditorUtility.CopySerialized(asset, existingAsset);
            EditorUtility.SetDirty(existingAsset);
            AddLog($"덮어쓰기 완료 : {path}", Color.green);
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
