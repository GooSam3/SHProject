using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.IO;
using System.Linq;

/// <summary>
/// FBX to LOD or LobbyPrefab 으로 생성하기.
/// </summary>
public class FBXToPrefabWindow : EditorWindow
{
	private string mSaveFolderPath = "Assets/_ZAssetBundle/";

	/// <summary> 로비용 프리팹 생성시, 자동 추가될 이름. </summary>
	private string LobbyPrefabSuffix { get; set; } = "_Lobby";

	private GUIContent mLobbySuffixLabel = EditorGUIUtility.TrTextContent("Suffix for LobbyPrefab", "Lobby용 프리팹 생성시 추가 접미사(suffix)");
	private GUIContent mSelectedObjectLabel = EditorGUIUtility.TrTextContent("선택된 대상");

	
	[MenuItem("ZGame/Art/FBX to Prefab Tool")]
	public static void ShowWindow()
	{
		GetWindow(typeof(FBXToPrefabWindow));
	}

	private void OnEnable()
	{
		Selection.selectionChanged += OnSelectionChanged;
	}

	private void OnDisable()
	{
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnDestroy()
	{
		Selection.selectionChanged -= OnSelectionChanged;
	}

	private void OnSelectionChanged()
	{
		Repaint();
	}

	private void OnGUI()
	{
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox(@"FBX파일에서 LOD 및 Lobby용 프리팹 자동 생성 도구", MessageType.Info);
		EditorGUILayout.Space(10f);

		bool isEnabled = null != Selection.activeObject;
		if (!isEnabled)
		{
			EditorGUILayout.HelpBox(@"Project창에서 폴더 or FBX파일을 선택해주세요.", MessageType.Warning);
		}
		else
		{
		}

		EditorGUILayout.Space(5f);
		EditorGUILayout.BeginHorizontal();
		{
			mSaveFolderPath = EditorGUILayout.TextField("Prefab 생성될 경로", mSaveFolderPath);
			if (GUILayout.Button("...", GUILayout.Width(40f)))
			{
				string path = EditorUtility.SaveFolderPanel("저장될 폴더 설정", "", "폴더설정");
				if (!string.IsNullOrEmpty(path))
				{
					mSaveFolderPath = path.Replace(Application.dataPath, "Assets");
				}
			}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space(50f);

		GUI.enabled = isEnabled;

		ZGUIStyles.Separator();
		EditorGUILayout.ObjectField(mSelectedObjectLabel, Selection.activeObject, typeof(DefaultAsset), false);
		LobbyPrefabSuffix = EditorGUILayout.TextField(mLobbySuffixLabel, LobbyPrefabSuffix);
		ZGUIStyles.Separator();
		if (GUILayout.Button("실행", ZGUIStyles.MapButton, GUILayout.Height(30f)))
		{
			Execute();
		}
	}

	private void Execute()
	{
		Object[] objs = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
		List<GameObject> fbxs = new List<GameObject>();
		foreach (var obj in objs)
		{
			if (obj is GameObject go)
				fbxs.Add(go);
		}

		if (fbxs.Count == 0)
		{
			Debug.LogError($"{objs.Length} 아무것도 선택되지 않았습니다. FBX파일 or 폴더를 선택하시고 실행해주세요.");
			return;
		}

		for (int i = 0; i < fbxs.Count; i++)
		{
			GameObject fbxGO = fbxs[i];
			if (null == fbxGO)
			{
				Debug.LogError("유효하지 않은 GameObject 입니다.");
				continue;
			}

			string fbxPath = AssetDatabase.GetAssetPath(fbxGO);
			ModelImporter modelImporter = AssetImporter.GetAtPath(fbxPath) as ModelImporter;
			if (null == modelImporter)
			{
				Debug.LogWarning($"해당 객체[{fbxPath}]는 FBX가 아닙니다.");
				continue;
			}

			// 진행도 표시
			if (EditorUtility.DisplayCancelableProgressBar("Lobby & LOG용 Prefab생성중...", $"{fbxPath}", (float)i / (float)fbxs.Count))
			{
				break;
			}

			string saveRootPath = mSaveFolderPath;
			if (!Directory.Exists(saveRootPath))
			{
				Directory.CreateDirectory(saveRootPath);
			}

			modelImporter.importBlendShapes = false;
			modelImporter.importVisibility = false;
			modelImporter.importCameras = false;
			modelImporter.importLights = false;
			modelImporter.generateSecondaryUV = false;
			modelImporter.isReadable = true;

			CreateLobbyPrefab(fbxGO, fbxPath);
			CreateLODPrefab(fbxGO, fbxPath);

			modelImporter.isReadable = false;
		}

		EditorUtility.ClearProgressBar();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 
	/// </summary>
	private void CreateLobbyPrefab(GameObject originalFbxGO, string fbxPath)
	{
		GameObject tempCreatedGO = Object.Instantiate(originalFbxGO);
		string fbxFolderPath = Path.GetDirectoryName(fbxPath);

		CollectRenderers(tempCreatedGO, out var nonLodRens, out var lodRens);

		if (nonLodRens.Count == 0)
			return;

		var lodGroup = tempCreatedGO.GetComponent<LODGroup>();
		if (null != lodGroup)
		{
			foreach (var lod in lodGroup.GetLODs())
			{
				// LOD용 Renderer객체 삭제.
				foreach (var ren in lod.renderers)
					DestroyImmediate(ren.gameObject);
			}

			// 컴포넌삭제
			DestroyImmediate(lodGroup);
		}

		// FBX파일있는 곳에 Material 생성
		foreach (var nonLodMeshes in nonLodRens.Values)
		{
			foreach (var ren in nonLodMeshes)
			{
				Material[] newMats = new Material[ren.sharedMaterials.Length];
				for (int i = 0; i < ren.sharedMaterials.Length; i++)
				{
					string matName = ren.name;
					string saveMatFullPath = Path.Combine(fbxFolderPath, $"{matName}.mat");

					var existingMat = (Material)AssetDatabase.LoadAssetAtPath(saveMatFullPath, typeof(Material));
					if (null != existingMat)
					{
						newMats[i] = existingMat;
					}
					else
					{
						var createdMat = CreateMaterial(ren.name);
						SaveMaterialAsset(createdMat, fbxFolderPath);
						newMats[i] = createdMat;
					}
				}

				ren.sharedMaterials = newMats;

				ResetWeaponBounds(ren);
			}
		}

		SaveAsPrefabAsset(tempCreatedGO, LobbyPrefabSuffix);
		Object.DestroyImmediate(tempCreatedGO);
	}

	private void CreateLODPrefab(GameObject originalFbxGO, string fbxPath)
	{
		GameObject tempCreatedGO = Object.Instantiate(originalFbxGO);
		string fbxFolderPath = Path.GetDirectoryName(fbxPath);

		CollectRenderers(tempCreatedGO, out var nonLodRens, out var lodRens);

		if (lodRens.Count == 0)
			return;

		// LOD용 아닌 Mesh들 삭제
		foreach (var nonLodMeshes in nonLodRens.Values)
		{
			foreach (var ren in nonLodMeshes)
				DestroyImmediate(ren.gameObject);
		}

		/* 
		* FBX상 LOD0 생성규칙에 의해 미리 생성되어 있을수도 있다.
		*/
		var lodGroup = tempCreatedGO.GetOrAddComponent<LODGroup>();
		if (lodGroup.lodCount == 0)
		{
			int maxLODFound = lodRens.Max((pair) => { return pair.Value.Count; });
			List<LOD> lods = new List<LOD>();

			for (int curLevel = 0; curLevel <= maxLODFound; curLevel++)
			{
				List<Renderer> curLevelRens = new List<Renderer>();
				foreach (var pair in lodRens)
				{
					var foundRen = pair.Value.Find((ren) => ren.name.Contains($"_LOD{curLevel}"));
					if (null != foundRen)
						curLevelRens.Add(foundRen);
				}

				if (curLevelRens.Count == 0)
				{
					Debug.LogWarning($"{curLevel}단계 LOD용 Mesh가 존재하지 않습니다.");
					continue;
				}

				var lod = new LOD();
				lod.renderers = curLevelRens.ToArray();
				var screenPercentage = curLevel == maxLODFound ? 0.01f : Mathf.Pow(0.5f, curLevel + 1);
				lod.screenRelativeTransitionHeight = screenPercentage;
				lods.Add(lod);
			}

			lodGroup.SetLODs(lods.ToArray());
			lodGroup.RecalculateBounds();
		}
		else
		{
			Debug.Log($"FBX설정에 의해 LOD설정이 이미 되어있습니다. [{fbxPath}]");
		}

		// FBX파일있는 곳에 Material 생성
		foreach (var lodMeshes in lodRens.Values)
		{
			foreach (var ren in lodMeshes)
			{
				Material[] newMats = new Material[ren.sharedMaterials.Length];
				for (int i = 0; i < ren.sharedMaterials.Length; i++)
				{
					string matName = ren.name;
					string saveMatFullPath = Path.Combine(fbxFolderPath, $"{matName}.mat");

					var existingMat = (Material)AssetDatabase.LoadAssetAtPath(saveMatFullPath, typeof(Material));
					if (null != existingMat)
					{
						newMats[i] = existingMat;
					}
					else
					{
						var createdMat = CreateMaterial(ren.name);
						SaveMaterialAsset(createdMat, fbxFolderPath);
						newMats[i] = createdMat;
					}
				}

				ren.sharedMaterials = newMats;

				ResetWeaponBounds(ren);
			}
		}

		SaveAsPrefabAsset(tempCreatedGO);
		Object.DestroyImmediate(tempCreatedGO);
	}

	/// <summary>
	/// 무기용 Renderer라면, Bounds 조절하기
	/// </summary>
	private void ResetWeaponBounds(Renderer targetRen)
	{
		string renName = targetRen.name;

		var splitStrs = renName.Split(new string[] { "_LOD" }, StringSplitOptions.RemoveEmptyEntries);
		// 무기용 Naming이 포함되어 있다면
		if (splitStrs[0].EndsWith("_W") && targetRen is SkinnedMeshRenderer skinnedRen)
		{
			var newBounds = skinnedRen.localBounds;
			newBounds.extents = Vector3.zero;
			skinnedRen.localBounds = newBounds;
		}
	}

	/// <summary>Lod용 Mesh와 일반 Mesh들 별개로 수집해서 반환</summary>
	private void CollectRenderers(GameObject targetGO, out Dictionary<string, List<Renderer>> nonLodRens, out Dictionary<string, List<Renderer>> lodRens)
	{
		Renderer[] allRenderers = targetGO.GetComponentsInChildren<Renderer>();
		nonLodRens = new Dictionary<string, List<Renderer>>();
		lodRens = new Dictionary<string, List<Renderer>>();

		// NonLOD와 LOD Mesh구분짓기
		foreach (var ren in allRenderers)
		{
			string renName = ren.name;

			if (renName.Contains("_LOD"))
			{
				//Debug.Log($"{ren} | {renName}");

				string meshName = renName.Split(new string[] { "_LOD" }, StringSplitOptions.RemoveEmptyEntries)[0];

				if (!lodRens.ContainsKey(renName))
					lodRens.Add(renName, new List<Renderer>());

				lodRens[renName].Add(ren);
			}
			else
			{
				if (!nonLodRens.ContainsKey(renName))
					nonLodRens.Add(renName, new List<Renderer>());

				nonLodRens[renName].Add(ren);
			}
		}
	}

	private Material CreateMaterial(string name, Material defaultMat = null)
	{
		Material newMat = null;
		if (null != defaultMat)
			newMat = new Material(defaultMat);
		else
			newMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
		newMat.name = name;

		return newMat;
	}

	/// <summary> 정해진 경로에 Prefab생성 및 저장 </summary>
	private bool SaveAsPrefabAsset(GameObject clonedGO, string addSuffix = null)
	{
		string prefabName = clonedGO.name.Replace("(Clone)", "") + addSuffix;
		string saveFullPath = Path.Combine(mSaveFolderPath, $"{prefabName}.prefab");

		var savedPrefab = PrefabUtility.SaveAsPrefabAsset(clonedGO, saveFullPath);

		if (null != savedPrefab)
			Debug.Log($"<color=cyan>저장 성공!</color> | {saveFullPath}", savedPrefab);

		return null != savedPrefab;
	}

	public void SaveMaterialAsset(Material mat, string saveFolderPath)
	{
		string matName = mat.name;
		string saveFullPath = Path.Combine(saveFolderPath, $"{matName}.mat");
		//saveFullPath = saveFullPath.Replace("\\", "/");

		var existType = AssetDatabase.GetMainAssetTypeAtPath(saveFullPath);
		if (null == existType || typeof(Material) != existType)
		{
			AssetDatabase.CreateAsset(mat, saveFullPath);
			//AssetDatabase.SaveAssets();
		}
		else
		{
			Debug.Log($"Material[{saveFullPath}]이 이미 존재해서 새로 생성하지 않습니다.");
		}
	}

	public static void SaveMeshAsset(Mesh mesh, Texture2D tex, string path)
	{
		CleanMesh(mesh);
		string meshPath = path + mesh.name + ".asset";
		AssetDatabase.CreateAsset(mesh, meshPath);
		if (tex != null)
		{
			string srcPath = AssetDatabase.GetAssetPath(tex);
			string desPath = "Assets/Resources/Equipments/" + tex.name + ".tga";
			AssetDatabase.CopyAsset(srcPath, desPath);
		}
		AssetDatabase.SaveAssets();
	}

	public static void CleanMesh(Mesh mesh)
	{
		mesh.uv2 = null;
		mesh.uv3 = null;
		mesh.uv4 = null;
		mesh.uv5 = null;
		mesh.uv6 = null;
		mesh.uv7 = null;
		mesh.uv8 = null;
		mesh.colors = null;
		mesh.colors32 = null;
		mesh.tangents = null;
	}
}