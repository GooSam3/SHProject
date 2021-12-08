using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 선택된 객체의 LOD Level별 Percent값 일괄 조절
/// </summary>
public class LODAdjustor : EditorWindow
{
	public class LODInfo
	{
		public LODInfo(int lodLevel, string name, float screenPercentage)
		{
			this.LODLevel = lodLevel;
			this.LODName = name;
			this.ScreenPercent = screenPercentage;
		}

		public int LODLevel { get; }
		public string LODName { get; }
		public float ScreenPercent { get; set; }
	}

	// Default colors for each LOD group....
	public static readonly Color[] kLODColors =
	{
		new Color(0.4831376f, 0.6211768f, 0.0219608f, 1.0f),
		new Color(0.2792160f, 0.4078432f, 0.5835296f, 1.0f),
		new Color(0.2070592f, 0.5333336f, 0.6556864f, 1.0f),
		new Color(0.5333336f, 0.1600000f, 0.0282352f, 1.0f),
		new Color(0.3827448f, 0.2886272f, 0.5239216f, 1.0f),
		new Color(0.8000000f, 0.4423528f, 0.0000000f, 1.0f),
		new Color(0.4486272f, 0.4078432f, 0.0501960f, 1.0f),
		new Color(0.7749016f, 0.6368624f, 0.0250984f, 1.0f)
	};

	public static readonly Color kCulledLODColor = new Color(.4f, 0f, 0f, 1f);

	private GUIContent mSelectedObjectLabel = EditorGUIUtility.TrTextContent("선택된 대상");
	private GUIContent mLODLevelCountLabel = EditorGUIUtility.TrTextContent("LOD 레벨 개수", "최소1개 이상(Culled용)");

	private const int mIgnoreLodIndex = 0;
	int mCurLodLevelCount = 1;
	List<LODInfo> mLodInfos = new List<LODInfo>();

	[MenuItem("ZGame/Art/LOD Adjustor")]
	public static void ShowWindow()
	{
		GetWindow(typeof(LODAdjustor));
	}

	private void OnEnable()
	{
		Selection.selectionChanged += OnSelectionChanged;

		RenewLodInfos(mCurLodLevelCount);
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
		EditorGUILayout.HelpBox(@"객체에 존재하는 LODGroup의 레벨별 Bias 설정", MessageType.Info);
		EditorGUILayout.Space(10f);
		ZGUIStyles.Separator();

		DrawGUI_LOD();

		bool isEnabled = null != Selection.activeObject;
		if (!isEnabled)
		{
			EditorGUILayout.HelpBox(@"Project창에서 폴더 or Prefab을 선택해주세요.", MessageType.Warning);
		}
		else
		{
		}

		bool isValidLodLevels = GetTotalPercent() >= 99.9f;
		GUI.enabled = isEnabled && isValidLodLevels;

		if (isEnabled && !isValidLodLevels)
		{
			EditorGUILayout.HelpBox(@"LOD 레벨 총합을 100%로 맞춰줘야합니다.", MessageType.Warning);
		}

		ZGUIStyles.Separator();
		EditorGUILayout.ObjectField(mSelectedObjectLabel, Selection.activeObject, typeof(DefaultAsset), false);
		ZGUIStyles.Separator();
		if (GUILayout.Button("적용", ZGUIStyles.MapButton, GUILayout.Height(30f)))
		{
			Execute();
		}
	}

	private void RenewLodInfos(int lodCount)
	{
		mLodInfos.Clear();

		for (int i = 0; i < lodCount; ++i)
		{
			mLodInfos.Add(new LODInfo(i, $"LOD {i}", 1.0f / (i + 1) * 100f));
		}

		mLodInfos.Add(new LODInfo(lodCount, $"Culled", 5f));
	}

	private void DrawGUI_LOD()
	{
		int prevLodCount = mCurLodLevelCount;
		mCurLodLevelCount = EditorGUILayout.IntSlider(mLODLevelCountLabel, mCurLodLevelCount, 1, 5);

		if (prevLodCount != mCurLodLevelCount)
		{
			RenewLodInfos(mCurLodLevelCount);
		}

		float prevValue = 100f;
		for (int idx = 0; idx < mLodInfos.Count; ++idx)
		{
			// LOD 0은 편집불가 (100%가 기본)
			GUI.enabled = idx != 0;

			var colorCache = GUI.backgroundColor;
			GUI.backgroundColor = new Color(kLODColors[idx].r, kLODColors[idx].g, kLODColors[idx].b, 0.8f);
			
			var info = mLodInfos[idx];
			info.ScreenPercent = Mathf.Clamp(EditorGUILayout.FloatField(info.LODName + " (%)", info.ScreenPercent, GUILayout.Height(28f)), 0, prevValue);
			prevValue = info.ScreenPercent;

			GUI.backgroundColor = colorCache;
			GUI.enabled = true;
		}

		EditorGUILayout.Space(20f);
	}

	private float GetTotalPercent()
	{
		float totalPercent = 0;
		mLodInfos.ForEach((input) => { totalPercent += input.ScreenPercent; });

		return totalPercent;
	}

	private void Execute()
	{
		Object[] objs = Selection.GetFiltered<Object>(SelectionMode.DeepAssets);
		List<GameObject> gameObjs = new List<GameObject>();
		foreach (var obj in objs)
		{
			if (obj is GameObject go)
				gameObjs.Add(go);
		}

		if (gameObjs.Count == 0)
		{
			Debug.LogError($"{objs.Length} 아무것도 선택되지 않았습니다. FBX파일 or 폴더를 선택하시고 실행해주세요.");
			return;
		}

		// 가장 첫번째 레벨에 있는 값은 변경되지 않는거라 skip되도록하기.
		List<LODInfo> clonedLodInfos = new List<LODInfo>(mLodInfos);
		clonedLodInfos.RemoveAt(0);

		foreach (var go in gameObjs)
		{
			var lodGroup = go.GetComponent<LODGroup>();
			if (null == lodGroup)
				continue;

			if (lodGroup.lodCount != mCurLodLevelCount)
			{
				Debug.LogWarning($"설정한 LOD Level개수와 맞지 않는 GameObject[{go.name}]라서 무시하겠습니다.", go);
				continue;
			}

			var lods = lodGroup.GetLODs();
			for (int i = 0; i < mCurLodLevelCount; ++i)
			{
				float newValue = clonedLodInfos[i].ScreenPercent / 100f;
				//Debug.LogWarning($"[{i}] OLD Value : {lods[i].screenRelativeTransitionHeight}, NEW Value : {newValue}");
				lods[i].screenRelativeTransitionHeight = newValue;
			}

			lodGroup.SetLODs(lods);
			lodGroup.RecalculateBounds();
			EditorUtility.SetDirty(go);

			Debug.Log($"<color=green>{go} | LODGroup의 레벨별 Bias 적용 성공</color>", go);
		}

		AssetDatabase.SaveAssets();
	}
}