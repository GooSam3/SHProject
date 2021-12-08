using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SHUIStepMainLoading : MonoBehaviour
{
	[SerializeField]
	private Slider ProgressBar = null;
	[SerializeField]
	private Text   ProgressText = null;
	[SerializeField]
	private string ScriptPrefabName = "SHPrefabScript";
	[SerializeField]
	private string UIMasterName  = "UIMasterRoot";
	[SerializeField]
	private CanvasScaler CanvasScaler = null;

	//----------------------------------------------------------------
	private void Awake()
	{
		ProgressBar.value = 0;
		CanvasScaler.referenceResolution = CApplication.Instance.GetAppBaseResolution();
	}

	void Start()
    {
		SHManagerResourceLoader.Instance.LoadPrefab(ScriptPrefabName, (string strName, GameObject pScriptPrefab) =>
		{
			Debug.Log("[MainLoading] Prefab Loaded =====================================");
			pScriptPrefab.SetActive(true);
			SHManagerSceneLoader.Instance.DoOpenSceneAdditive(UIMasterName, HandleMainLoadingProgress, HandleMainLoadingEnd);
		});
    }

	//-----------------------------------------------------------------
	private void HandleMainLoadingProgress(float fProgress)
	{
		ProgressBar.value = fProgress;
		int iPercent = (int)(fProgress * 100f);
		ProgressText.text = string.Format("{0}%", iPercent);
	}

	private void HandleMainLoadingEnd(string strSceneName)
	{
		HandleMainLoadingProgress(1f);
		Debug.Log("[MainLoading] Loading Finish =====================================");
		UIManager.Instance.DoUIMgrSceneLoadingStart(EGameSceneType.SHSceneLogin, null);
	}
   
}
