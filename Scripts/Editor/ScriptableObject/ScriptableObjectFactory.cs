//https://github.com/liortal53/ScriptableObjectFactory/tree/master/Assets/Editor

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A helper class for instantiating ScriptableObjects in the editor.
/// </summary>
public class ScriptableObjectFactory
{
    [MenuItem("Assets/Create/ScriptableObject")]
    public static void CreateScriptableObject()
    {
        var assembly = GetAssembly();

        // Get all classes derived from ScriptableObject
        var allScriptableObjects = (from t in assembly.GetTypes()
                                    where t.IsSubclassOf(typeof(ScriptableObject)) && !t.IsSubclassOf(typeof(Editor)) && !t.IsAbstract
									select t).ToArray();

        // Show the selection window.
        ScriptableObjectWindow.Init(allScriptableObjects);
    }

    /// <summary>
    /// Returns the assembly that contains the script code for this project (currently hard coded)
    /// </summary>
    private static Assembly GetAssembly()
    {
        return Assembly.Load(new AssemblyName("Assembly-CSharp"));
    }

	/// <summary>Create a scriptable object asset</summary>
	public static T CreateAt<T>(string assetPath) where T : ScriptableObject
	{
		return CreateAt(typeof(T), assetPath) as T;
	}

	/// <summary>Create a scriptable object asset</summary>
	public static ScriptableObject CreateAt(Type assetType, string assetPath)
	{
		ScriptableObject asset = ScriptableObject.CreateInstance(assetType);
		if (asset == null)
		{
			Debug.LogError("failed to create instance of " + assetType.Name + " at " + assetPath);
			return null;
		}
		AssetDatabase.CreateAsset(asset, assetPath);
		return asset;
	}
}