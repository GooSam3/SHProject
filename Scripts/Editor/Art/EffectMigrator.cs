using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Z_Fx_Paticle_01	ZMobile/Chaos/FX/CustomDataOneTexture					Z_Fx_Paticle_01	Zero/Particle/0105_One_Old
/// Z_Fx_Paticle_02 ZMobile/Chaos/FX/CustomDataMaskTexture					Z_Fx_Paticle_02 Zero/Particle/0206_URPMask_Old
/// Z_Fx_Paticle_03 ZMobile/Chaos/FX/CustomDataDisTexture					Z_Fx_Paticle_03 Zero/Particle/0304_URPUVDisC_Old
/// Z_Fx_Paticle_04 ZMobile/Chaos/FX/CustomDataDisTexture_AlphaBooster		Z_Fx_Paticle_03 Zero/Particle/0304_URPUVDisC_Old
/// Z_Fx_Paticle_05 ZMobile/Chaos/FX/CustomDataOneTexture_AlphaBooster		Z_Fx_Paticle_01 Zero/Particle/0105_One_Old
/// Z_Fx_Paticle_06 ZMobile/Chaos/FX/CustomDataMaskTexture_AlphaBoost		Z_Fx_Paticle_02 Zero/Particle/0206_URPMask_Old
/// Z_Fx_Trail_01   ZMobile/Chaos/FX/Z_Trail_01								Z_Fx_Trail_01 Zero/Particle/URP_TimeTrail_Old
/// 4.메터리얼을 위의 쉐이더로 교체해주시면되고..
/// 5.만약 위 쉐이더가 감지가 안되었거나 에러쉐이더일경우.  Z_Urp_Base.shader Zero/Particle/Urp_Base_Particle
/// 로연결해주시면됩니다.
/// </remarks>
public class EffectMigrator : EditorWindow
{
	private bool mCheckMaterial = true;
	private bool mCheckEffect = true;

	private GUIContent mMaterialLabel = EditorGUIUtility.TrTextContent("Material");
	private GUIContent mEffectLabel = EditorGUIUtility.TrTextContent("Effect");

	private List<Tuple<Shader, Shader>> mListReplacementShader = new List<Tuple<Shader, Shader>>();
	private Shader mErrorReplacementShader;

	[MenuItem("ZGame/Art/EffectMigration Tool")]
	public static void ShowWindow()
	{
		GetWindow(typeof(EffectMigrator));
	}

	private void OnGUI()
	{
		EditorGUILayout.Space();
		EditorGUILayout.HelpBox(@"이 에디터의 기능은 기존 Chaos에서 사용하던 [이펙트]가 사용중이던 Shader들을 현 프로젝트에 설정된 Shader로 매칭해서 변환해주는 기능.", MessageType.Info);
		EditorGUILayout.Space();

		using (var v1 = new GUILayout.VerticalScope("box"))
		{
			mCheckMaterial = EditorGUILayout.Toggle(mMaterialLabel, mCheckMaterial);
			mCheckEffect = EditorGUILayout.Toggle(mEffectLabel, mCheckEffect);
		}		

		GUI.enabled = null != Selection.activeObject;
		if (!GUI.enabled)
		{
			EditorGUILayout.HelpBox(@"Project창에서 폴더 or 파일을 선택해주세요.", MessageType.Warning);
		}
		
		if (GUILayout.Button("Execute", GUILayout.Height(30f)))
		{
			if (mCheckMaterial)
				ConvertMaterials();

			if (mCheckEffect)
				ConvertEffectPrefabs();
		}
	}

	private void ConvertMaterials()
	{
		Material[] matArr = Selection.GetFiltered<Material>(SelectionMode.DeepAssets);

		foreach (var mat in matArr)
		{
			if (null == mat.shader || string.IsNullOrEmpty(mat.shader.name))
			{
				continue;
			}

			ConvertSingleMaterial(mat);
		}

		AssetDatabase.Refresh();
	}

	private void ConvertSingleMaterial(Material _targetMat)
	{
		foreach (var comparePair in mListReplacementShader)
		{
			if (null == _targetMat.shader)
			{
				Debug.LogError("Shader error");
			}
			else if (null == comparePair.Item1)
			{
				Debug.LogError("Compare Shader error");
			}
			if (_targetMat.shader.name == comparePair.Item1.name)
			{
				if (null == comparePair.Item2)
					continue;

				Debug.Log($"{_targetMat.name} Material의 Shader 변환 성공! | <color=red>{_targetMat.shader.name}</color> -->> <color=green>{comparePair.Item1.name}</color>");

				_targetMat.shader = comparePair.Item2;

				EditorUtility.SetDirty(_targetMat);
			}
		}
	}

	private void ConvertEffectPrefabs()
	{
		GameObject[] gameObjectArr = Selection.GetFiltered<GameObject>(SelectionMode.DeepAssets);

		foreach (var effGO in gameObjectArr)
		{
			var psArr = effGO.GetComponentsInChildren<ParticleSystem>();
			foreach (var ps in psArr)
			{
				var psRenderer = ps.GetComponent<ParticleSystemRenderer>();
				if (null == psRenderer.sharedMaterial)
				{
					continue;
				}

				ConvertSingleMaterial(psRenderer.sharedMaterial);
			}
		}

		AssetDatabase.Refresh();
	}

	// When the game starts update the logger instance with the users selections
	private void OnEnable()
	{
		// 변환 대응할 Shader목록 만들기.
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataOneTexture"), Shader.Find("Zero/Particle/0105_One_Old")));
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataMaskTexture"), Shader.Find("Zero/Particle/0206_URPMask_Old")));
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataDisTexture"), Shader.Find("Zero/Particle/0304_URPUVDisC_Old")));
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataDisTexture_AlphaBooster"), Shader.Find("Zero/Particle/0304_URPUVDisC_Old")));
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataOneTexture_AlphaBooster"), Shader.Find("Zero/Particle/0105_One_Old")));
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/CustomDataMaskTexture_AlphaBoost"), Shader.Find("Zero/Particle/0206_URPMask_Old")));		
		mListReplacementShader.Add(new Tuple<Shader, Shader>(Shader.Find("ZMobile/Chaos/FX/Z_Trail_01"), Shader.Find("Zero/Particle/URP_TimeTrail_Old")));

		mErrorReplacementShader = Shader.Find("ShaderZero/Particle/Urp_Base_Particle");

		foreach (var pair in mListReplacementShader)
		{
			if (null == pair.Item1)
			{
				Debug.LogError($"존재하지 않는 Old Shader가 있습니다.");
			}

			if (null == pair.Item2)
			{
				Debug.LogError($"존재하지 않는 New Shader가 있습니다.");
			}
		}
	}
}