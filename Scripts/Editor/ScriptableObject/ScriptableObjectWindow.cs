//https://github.com/liortal53/ScriptableObjectFactory/tree/master/Assets/Editor

using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

internal class EndNameEdit : EndNameEditAction
{
    #region implemented abstract members of EndNameEditAction
    public override void Action(int instanceId, string pathName, string resourceFile)
    {
        AssetDatabase.CreateAsset(EditorUtility.InstanceIDToObject(instanceId), AssetDatabase.GenerateUniqueAssetPath(pathName));
    }

    #endregion
}

/// <summary>
/// Scriptable object window.
/// </summary>
public class ScriptableObjectWindow : EditorWindow
{
    private int selectedIndex = -1;
    private static string[] names;

    private static Type[] types;

    private static Type[] Types
    {
        get { return types; }
        set
        {
            types = value;
            names = types.Select(t => t.FullName).ToArray();
        }
    }

    public static void Init(Type[] scriptableObjects)
    {
        Types = scriptableObjects;

        var window = EditorWindow.GetWindow<ScriptableObjectWindow>(true, "Create a new ScriptableObject", true);
        window.ShowPopup();
    }

    public void OnGUI()
    {
        GUILayout.Label("ScriptableObject Class");
        
        for (int i = 0; i < names.Length; i++)
        {
            if (GUILayout.Button(names[i]))
            {
                selectedIndex = i;
            }
        }

        if (-1 == selectedIndex)
        {
            GUILayout.Label("Selected Class : Empty");
            return;
        }
        else
            GUILayout.Label("Selected Class : " + names[selectedIndex]);

        if (GUILayout.Button("Create"))
        {
			Debug.Log($"Try {types[selectedIndex]} ScriptableObject Asset");
            var asset = ScriptableObject.CreateInstance(types[selectedIndex]);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                asset.GetInstanceID(),
                ScriptableObject.CreateInstance<EndNameEdit>(),
                string.Format("{0}.asset", names[selectedIndex]),
                AssetPreview.GetMiniThumbnail(asset),
                null);

            Close();
        }
    }
}