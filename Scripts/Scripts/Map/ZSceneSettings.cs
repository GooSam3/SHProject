using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 맵별 효과 적용을 위해 설정가능한 데이터들 모음
/// </remarks>
public class ZSceneSettings : ScriptableObject
{
	[Header("──── Quality 설정 ────────────")]
	[Tooltip("RenderPipeline 에 존재하는 Distance값 무시하고 새로 설정할지 여부")]
	public bool AllowShadowDistance = false;
	public float ShadowDistance = 40f;

	[Header("──── Camera 설정 ────────────")]
	public float CameraFarClipPlane = 200f;
	[Space(5)]
	public bool AllowOverrideClearFlags = true;
	public CameraClearFlags CameraClearFlags = CameraClearFlags.Skybox; // 우리게임 기본값
	public Color CameraBackgrondColor = Color.black; // 우리게임 기본값

	//[Space(20f)]
	[Header("──── Light 설정 ────────────────")]
	[Tooltip("Scene자체에 설정한 Light셋팅을 덮어씌울건지 여부")]
	public bool UseLightSetting = true;
	[Tooltip("Directional 라이트이고, 씬에 활성화 상태 1개만 존재해야 제대로 찾아서 적용됨.")]
	public Color LightColor = Color.white;
	[Tooltip("그림자 강도를 설정(진하기라고 보면됨)")]
	[Range(0, 1)]
	public float ShadowStrength = 0.82f;


	[Header("──── Global Shader 설정 ────────────")]
	[Tooltip("Light로부터 나오는 빛을 캐릭터에게만 얼마나 영향을 줄지에 대한 값")]
	public float CharLightAttenuation = 1f;
	[Tooltip("Probe로부터 나오는 빛을 캐릭터에게만 얼마나 영향을 줄지에 대한 값")]
	public float CharProbeAttenuation = 1f;
	//[Space(20f)]
	//[Tooltip("Radially Symmetric Reflectance Map")]
	//public Texture RSRM_Texture = null;
	//[Vector3Range(-.5f, -.5f, -.5f, .5f, .5f, .5f, true, "Shader에서 사용할 Fake 라이트 방향 변수")]
	//public Vector3 SceneLightDir = new Vector3(0.33f, 0.33f, 0.33f);
	//[Tooltip("Shader코드에서 tex2Dlod에 사용될 강제 밉맵 레벨 값 (0이 원본)")]
	//[Range(0, 5)]
	//public int ForcedTextureLodLevel = 0;
	//[Tooltip("해당씬에서 강제로 노멀라이즈된 노멀 스케일 설정할지 여부")]
	//public bool ApplyNormalScaleFactor = false;
	//[Range(0.1f, 1f)]
	//public float NormalScaleFactor = 1f;

	[Header("[Dissolve Shader를 위한 설정값들]")]
	public Texture Dissolve_Texture = null;
	//TO DO 이부분은 보간 보다는 outline을 세팅하는것으로 하는게 좋을꺼 같아서 바꿧습니다. 팀도 추가하였습니다.
	[Range(0.0f, 1.0f), Tooltip("Dissolve Texture를 이용하여 Line의 사이즈를 결정합니다")]
	public float Dissolve_LineSize = 0.2f;
	public Vector2 Dissolve_Tiling = new Vector2(2,2);
	[ColorUsage(true, true)]
	public Color Dissolve_TintColor = Color.white;

	[Header("[기믹 속성 상태 Texture]")]
	public Texture AttributeState_Texture = null;

	[Header("──── PostProcessing Profile ────────────")]
	[Space(20f)]
	[Tooltip("효과가 필요없는 Scene은 None으로 설정해주세요.")]
	public VolumeProfile PP_Profile;

	/// <summary> 설정된 Shader변수들 적용 </summary>
	public void ApplyShaderSettings()
	{
		if (AllowShadowDistance)
		{
			QualitySettings.shadowDistance = ShadowDistance;
		}

		if (CameraManager.hasInstance)
		{
			if (AllowOverrideClearFlags)
			{
				CameraManager.Instance.Main.clearFlags = this.CameraClearFlags;
				CameraManager.Instance.Main.backgroundColor = this.CameraBackgrondColor;
			}

			CameraManager.Instance.ChangeFarClipPlane(CameraFarClipPlane);
			CameraManager.Instance.DoSetupPostProcess(PP_Profile);
		}

		Shader.SetGlobalTexture(Shader.PropertyToID("_Dissolve_Texture"), Dissolve_Texture);
		//TO DO 타일링 추가와 타일링관련 추가.
		Shader.SetGlobalVector(Shader.PropertyToID("_Dissolve_Tiling"), Dissolve_Tiling);		
		Shader.SetGlobalFloat(Shader.PropertyToID("_Dissolve_LineSize"), Dissolve_LineSize);
		Shader.SetGlobalColor(Shader.PropertyToID("_Dissolve_TintColor"), Dissolve_TintColor);

		Shader.SetGlobalFloat(Shader.PropertyToID("_CharLightAttenuation"), CharLightAttenuation);
		Shader.SetGlobalFloat(Shader.PropertyToID("_CharProbeAttenuation"), CharProbeAttenuation);

		Shader.SetGlobalTexture(Shader.PropertyToID("_AttributeState_Texture"), AttributeState_Texture);

#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.SceneView.RepaintAll();
#endif
	}
}
