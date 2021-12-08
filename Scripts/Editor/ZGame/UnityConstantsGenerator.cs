//#define DISABLE_AUTO_GENERATION
#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// 유니티의 상수값들 접근하기 편하기 위해 코드로 만들어주는 기능.
/// </summary>
public static class UnityConstantsGenerator
{
	/// <summary> 생성된 파일이 저장될 위치 </summary>
	private const string FOLDER_LOCATION = "Scripts/";
	private const string FILE_NAME = "UnityConstants.cs";

	[MenuItem("ZGame/For Development/Generate UnityConstants.cs")]
	public static void Generate()
	{
		//// Try to find an existing file in the project called "UnityConstants.cs"
		//string filePath = string.Empty;
		//foreach (var file in Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories))
		//{
		//	if (Path.GetFileNameWithoutExtension(file) == "UnityConstants")
		//	{
		//		filePath = file;
		//		break;
		//	}
		//}

		//// If no such file exists already, use the save panel to get a folder in which the file will be placed.
		//if (string.IsNullOrEmpty(filePath))
		//{
		//	string directory = EditorUtility.OpenFolderPanel("Choose location for UnityConstants.cs", Application.dataPath, "");

		//	// Canceled choose? Do nothing.
		//	if (string.IsNullOrEmpty(directory))
		//	{
		//		return;
		//	}

		//	filePath = Path.Combine(directory, "UnityConstants.cs");
		//}

		var folderPath = Application.dataPath + "/" + FOLDER_LOCATION;
		if (!Directory.Exists(folderPath))
			Directory.CreateDirectory(folderPath);

		string filePath = Path.Combine(folderPath, FILE_NAME);

		// Write out our file
		using (var writer = new StreamWriter(filePath))
		{
			writer.WriteLine("// This file is auto-generated. Modifications are not saved.");
			writer.WriteLine();
			writer.WriteLine("namespace UnityConstants");
			writer.WriteLine("{");

			// Write out the tags ===============================================
			writer.WriteLine("    public static class Tags");
			writer.WriteLine("    {");
			foreach (var tag in UnityEditorInternal.InternalEditorUtility.tags)
			{
				//writer.WriteLine("        /// <summary>");
				//writer.WriteLine("        /// Name of tag '{0}'.", tag);
				//writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const string {0} = \"{1}\";", MakeSafeForCode(tag), tag);
			}
			writer.WriteLine("    }");
			writer.WriteLine();

			// Write out sorting layers ===============================================
			writer.WriteLine("    public static class SortingLayers");
			writer.WriteLine("    {");
			foreach (var layer in SortingLayer.layers)
			{
				//writer.WriteLine("        /// <summary>");
				//writer.WriteLine("        /// ID of sorting layer '{0}'.", layer.name);
				//writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(layer.name), layer.id);
			}
			writer.WriteLine("    }");
			writer.WriteLine();

			// Write out layers ===============================================
			writer.WriteLine("    public static class Layers");
			writer.WriteLine("    {");
			for (int i = 0; i < 32; i++)
			{
				string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
				if (!string.IsNullOrEmpty(layer))
				{
					//writer.WriteLine("        /// <summary>");
					//writer.WriteLine("        /// Index of layer '{0}'.", layer);
					//writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(layer), i);
				}
			}

			writer.WriteLine();
			writer.WriteLine();
			for (int i = 0; i < 32; i++)
			{
				string layer = UnityEditorInternal.InternalEditorUtility.GetLayerName(i);
				if (!string.IsNullOrEmpty(layer))
				{
					//writer.WriteLine("        /// <summary>");
					//writer.WriteLine("        /// Bitmask of layer '{0}'.", layer);
					//writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const int {0}Mask = 1 << {1};", MakeSafeForCode(layer), i);
				}
			}
			writer.WriteLine();
			string funcCode = @"		public static int OnlyIncluding( params int[] layers )
		{
			int mask = 0;
			for( var i = 0; i < layers.Length; i++ )
				mask |= ( 1 << layers[i] );

			return mask;
		}

		public static int EverythingBut( params int[] layers )
		{
			return ~OnlyIncluding( layers );
		}";
			writer.Write(funcCode);
			writer.WriteLine();
			writer.WriteLine("    }");
			writer.WriteLine();

			// Write out scenes ===============================================
			writer.WriteLine("    public static class Scenes");
			writer.WriteLine("    {");
			int sceneIndex = 0;
			foreach (var scene in EditorBuildSettings.scenes)
			{
				if (!scene.enabled)
				{
					continue;
				}

				var sceneName = Path.GetFileNameWithoutExtension(scene.path);

				//writer.WriteLine("        /// <summary>");
				//writer.WriteLine("        /// ID of scene '{0}'.", sceneName);
				//writer.WriteLine("        /// </summary>");
				writer.WriteLine("        public const int {0} = {1};", MakeSafeForCode(sceneName), sceneIndex);

				sceneIndex++;
			}
			writer.WriteLine("    }");
			writer.WriteLine();

			// Write out Input axes
			writer.WriteLine("    public static class Axes");
			writer.WriteLine("    {");
			var axes = new HashSet<string>();
			var inputManagerProp = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0]);
			foreach (SerializedProperty axe in inputManagerProp.FindProperty("m_Axes"))
			{
				var name = axe.FindPropertyRelative("m_Name").stringValue;
				var variableName = MakeSafeForCode(name);
				if (!axes.Contains(variableName))
				{
					//writer.WriteLine("        /// <summary>");
					//writer.WriteLine("        /// Input axis '{0}'.", name);
					//writer.WriteLine("        /// </summary>");
					writer.WriteLine("        public const string {0} = \"{1}\";", variableName, name);
					axes.Add(variableName);
				}
			}
			writer.WriteLine("    }");

			// End of namespace UnityConstants
			writer.WriteLine("}");
			writer.WriteLine();
		}

		// Refresh
		AssetDatabase.Refresh();
	}

	private static string MakeSafeForCode(string str)
	{
		str = Regex.Replace(str, "[^a-zA-Z0-9_]", "_", RegexOptions.Compiled);
		if (char.IsDigit(str[0]))
		{
			str = "_" + str;
		}
		return str;
	}
}

#if !DISABLE_AUTO_GENERATION
// this post processor listens for changes to the TagManager and automatically rebuilds all classes if it sees a change
public class ConstandsGeneratorPostProcessor : AssetPostprocessor
{
	// for some reason, OnPostprocessAllAssets often gets called multiple times in a row. This helps guard against rebuilding classes
	// when not necessary.
	static DateTime? _lastTagsAndLayersBuildTime;
	static DateTime? _lastScenesBuildTime;


	static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
	{
		// layers and tags changes
		if (importedAssets.Contains("ProjectSettings/TagManager.asset"))
		{
			if (!_lastTagsAndLayersBuildTime.HasValue || _lastTagsAndLayersBuildTime.Value.AddSeconds(5) < DateTime.Now)
			{
				_lastTagsAndLayersBuildTime = DateTime.Now;
				UnityConstantsGenerator.Generate();
			}
		}

		// scene changes
		if (importedAssets.Contains("ProjectSettings/EditorBuildSettings.asset"))
		{
			if (!_lastScenesBuildTime.HasValue || _lastScenesBuildTime.Value.AddSeconds(5) < DateTime.Now)
			{
				_lastScenesBuildTime = DateTime.Now;
				UnityConstantsGenerator.Generate();
			}
		}
	}
}
#endif //DISABLE_AUTO_GENERATION
#endif