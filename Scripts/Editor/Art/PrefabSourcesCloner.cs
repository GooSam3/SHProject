using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// Prefab내의 원본 Asset들 복제해서 정해진 폴더로 복제해주는 도구
/// </summary>
public class PrefabSourcesCloner : EditorWindow
{
	private string mSaveFolderPath = "Assets/";

	private GUIContent mSelectedObjectLabel = EditorGUIUtility.TrTextContent("선택된 프리팹(Prefab)");


	[MenuItem("ZGame/Art/PrefabSourcesCloner")]
	public static void ShowWindow()
	{
		GetWindow(typeof(PrefabSourcesCloner));
	}

	private void OnEnable()
	{
		Selection.selectionChanged += OnSelectionChanged;

		EditorUtility.ClearProgressBar();
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
		EditorGUILayout.HelpBox(@"Prefab의 원본 Asset들 복제해서 정해진 폴더로 복제해주는 도구", MessageType.Info);
		EditorGUILayout.Space(10f);

		bool isEnabled = null != Selection.activeObject;
		if (!isEnabled)
		{
			EditorGUILayout.HelpBox(@"Project창에서 폴더 or Prefab파일을 선택해주세요.", MessageType.Warning);
		}
		else
		{
		}

		EditorGUILayout.Space(5f);
		EditorGUILayout.BeginHorizontal();
		{
			mSaveFolderPath = EditorGUILayout.TextField("복제될 생성될 경로(해당 경로에 프리팹이름으로 폴더 자동생성))", mSaveFolderPath);
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
		ZGUIStyles.Separator();
		if (GUILayout.Button("실행", ZGUIStyles.MapButton, GUILayout.Height(30f)))
		{
			Execute();
		}
	}

	private void Execute()
	{
		Object[] objs = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
		List<GameObject> gameObjs = new List<GameObject>();
		foreach (var obj in objs)
		{
			if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.Regular)
			{
				Debug.LogWarning($"{obj} | 오리지널 프리팹이 아닙니다.");
				continue;
			}

			if (obj is GameObject go)
				gameObjs.Add(go);

			Debug.Log($"{obj} | {PrefabUtility.GetPrefabAssetType(obj)}");
		}

		if (gameObjs.Count == 0)
		{
			Debug.LogError($"{objs.Length} 아무것도 선택되지 않았습니다. Prefab파일 or 폴더를 선택하시고 실행해주세요.");
			return;
		}

		for (int i = 0; i < gameObjs.Count; i++)
		{
			GameObject originalPrefab = gameObjs[i];
			if (null == originalPrefab)
			{
				Debug.LogError("유효하지 않은 GameObject 입니다.");
				continue;
			}

			List<Mesh> meshList = new List<Mesh>();
			List<Material> matList = new List<Material>();
			List<Texture> texList = new List<Texture>();
			List<RuntimeAnimatorController> animtCtlrList = new List<RuntimeAnimatorController>();
			List<AnimationClip> animClipList = new List<AnimationClip>();

			//--------------------------------------------------------------------
			//------------------- Meshes & Materials & Textures -----------------------------
			List<MeshFilter> meshFilterList = new List<MeshFilter>();
			List<MeshRenderer> meshRenList = new List<MeshRenderer>();
			List<SkinnedMeshRenderer> skinnedMeshRenList = new List<SkinnedMeshRenderer>();

			meshFilterList.AddRange(originalPrefab.GetComponentsInChildren<MeshFilter>());
			meshRenList.AddRange(originalPrefab.GetComponentsInChildren<MeshRenderer>());
			skinnedMeshRenList.AddRange(originalPrefab.GetComponentsInChildren<SkinnedMeshRenderer>());

			foreach (var filter in meshFilterList)
			{
				meshList.Add(filter.sharedMesh);
			}

			foreach (var ren in meshRenList)
			{
				// collect materials
				foreach (var mat in ren.sharedMaterials)
				{
					matList.Add(mat);

					foreach (var texPropName in mat.GetTexturePropertyNames())
					{
						texList.Add(mat.GetTexture(Shader.PropertyToID(texPropName)));
					}
				}
			}

			foreach (var skinnedRen in skinnedMeshRenList)
			{
				meshList.Add(skinnedRen.sharedMesh);

				// collect materials
				foreach (var mat in skinnedRen.sharedMaterials)
				{
					matList.Add(mat);

					foreach (var texPropName in mat.GetTexturePropertyNames())
					{
						texList.Add(mat.GetTexture(Shader.PropertyToID(texPropName)));
					}
				}
			}

			//--------------------------------------------------------------------
			//------------------- Animations -------------------------------------
			List<Animator> animatorList = new List<Animator>();
			animatorList.AddRange(originalPrefab.GetComponentsInChildren<Animator>());

			foreach (var animator in animatorList)
			{
				if (animtCtlrList.Contains(animator.runtimeAnimatorController))
					continue;
				animtCtlrList.Add(animator.runtimeAnimatorController);

				foreach (var animClip in animator.runtimeAnimatorController.animationClips)
				{
					if (animClipList.Contains(animClip))
						continue;
					animClipList.Add(animClip);
				}
			}


			// 저장폴더 생성
			string saveFolder = Path.Combine(mSaveFolderPath, $"{originalPrefab.name}");
			if (!Directory.Exists(saveFolder))
			{
				Directory.CreateDirectory(saveFolder);
				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			// 진행도 표시
			if (EditorUtility.DisplayCancelableProgressBar("Prefab 구성요소들 복제중...", $"{originalPrefab}", (float)i / (float)gameObjs.Count))
			{
				break;
			}

			List<Mesh> clonedMeshList = new List<Mesh>();
			List<Material> clonedMatList = new List<Material>();
			List<Texture> clonedTexList = new List<Texture>();
			List<RuntimeAnimatorController> clonedAnimtCtlrList = new List<RuntimeAnimatorController>();
			List<AnimationClip> clonedAnimClipList = new List<AnimationClip>();
			List<GameObject> clonedPrefabList = new List<GameObject>();

			clonedMeshList = CopyAssets(saveFolder, meshList);
			clonedMatList = CopyAssets(saveFolder, matList);
			clonedTexList = CopyAssets(saveFolder, texList);
			clonedAnimtCtlrList = CopyAssets(saveFolder, animtCtlrList);
			clonedAnimClipList = CopyAssets(saveFolder, animClipList);

			Queue<Mesh> clonedMeshQueue = new Queue<Mesh>(clonedMeshList);
			Queue<Material> clonedMatQueue = new Queue<Material>(clonedMatList);
			Queue<Texture> clonedTexQueue = new Queue<Texture>(clonedTexList);
			Queue<RuntimeAnimatorController> clonedAnimtCtlrQueue = new Queue<RuntimeAnimatorController>(clonedAnimtCtlrList);
			Queue<AnimationClip> clonedAnimClipQueue = new Queue<AnimationClip>(clonedAnimClipList);

			// 프리팹도 복제
			clonedPrefabList = CopyAssets(saveFolder, new List<GameObject>() { originalPrefab });
			GameObject clonedPrefab = clonedPrefabList[0];

			////--------------------------------------------------------------------
			////------------------- Meshes & Materials & Textures -----------------------------
			//List<MeshFilter> newMeshFilterList = new List<MeshFilter>();
			//List<MeshRenderer> newMeshRenList = new List<MeshRenderer>();
			//List<SkinnedMeshRenderer> newSkinnedMeshRenList = new List<SkinnedMeshRenderer>();

			//newMeshFilterList.AddRange(clonedPrefab.GetComponentsInChildren<MeshFilter>());
			//newMeshRenList.AddRange(clonedPrefab.GetComponentsInChildren<MeshRenderer>());
			//newSkinnedMeshRenList.AddRange(clonedPrefab.GetComponentsInChildren<SkinnedMeshRenderer>());

			//foreach (var filter in newMeshFilterList)
			//{
			//	filter.sharedMesh = clonedMeshQueue.Dequeue();
			//}

			//foreach (var ren in newMeshRenList)
			//{
			//	int index = 0;
			//	foreach (var mat in ren.sharedMaterials)
			//	{
			//		ren.sharedMaterials[index] = clonedMatQueue.Dequeue();
					
			//		foreach (var texPropName in ren.sharedMaterials[index].GetTexturePropertyNames())
			//		{
			//			ren.sharedMaterials[index].SetTexture(Shader.PropertyToID(texPropName), clonedTexQueue.Dequeue());
			//		}
			//		index++;
			//	}
			//}

			//foreach (var skinnedRen in newSkinnedMeshRenList)
			//{
			//	skinnedRen.sharedMesh = clonedMeshQueue.Dequeue();

			//	int index = 0;
			//	foreach (var mat in skinnedRen.sharedMaterials)
			//	{
			//		skinnedRen.sharedMaterials[index] = clonedMatQueue.Dequeue();

			//		foreach (var texPropName in skinnedRen.sharedMaterials[index].GetTexturePropertyNames())
			//		{
			//			skinnedRen.sharedMaterials[index].SetTexture(Shader.PropertyToID(texPropName), clonedTexQueue.Dequeue());
			//		}
			//		index++;
			//	}
			//}
		}

		EditorUtility.ClearProgressBar();
		AssetDatabase.SaveAssets();
		AssetDatabase.Refresh();
	}

	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="OBJECT_TYPE"></typeparam>
	/// <param name="targetFolder"></param>
	/// <param name="objList"></param>
	/// <returns>동일 복제본 리스트</returns>
	List<OBJECT_TYPE> CopyAssets<OBJECT_TYPE>(string targetFolder, List<OBJECT_TYPE> objList) where OBJECT_TYPE : Object
	{
		// 중복 복사 막기위한 변수
		Dictionary<OBJECT_TYPE, OBJECT_TYPE> movedAssetDic = new Dictionary<OBJECT_TYPE, OBJECT_TYPE>();
		List<OBJECT_TYPE> clonedList = new List<OBJECT_TYPE>(objList.Count);

		foreach (var asset in objList)
		{
			if (null == asset)
			{
				clonedList.Add(null); //순서 맞추기위함.
				continue;
			}

			string assetPath = AssetDatabase.GetAssetPath(asset);
			if (movedAssetDic.ContainsKey(asset))
			{
				clonedList.Add(movedAssetDic[asset]);
				continue;
			}

			string filename = Path.GetFileName(assetPath);

			Debug.LogWarning($"{asset} | {AssetDatabase.GetAssetPath(asset)} | {filename}");

			// 파일명 & 확장자
			string clonedAssetPath = Path.Combine(targetFolder, filename);
			AssetDatabase.CopyAsset(assetPath, clonedAssetPath);

			var clonedObj = AssetDatabase.LoadAssetAtPath<OBJECT_TYPE>(clonedAssetPath);
			clonedList.Add(clonedObj);

			movedAssetDic.Add(asset, clonedObj);
		}

		return clonedList;
	}
}
