using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SHUIStepPatchScreen : MonoBehaviour
{
	[SerializeField]
	private Slider	PatchProgressBar = null;
	[SerializeField]
	private Text	PatchProgressText = null;

	//----------------------------------------------------------------
	private void Awake()
	{
		CPatcherBase.SPatchEvent rPatchEvent = SHManagerPatchDownloader.Instance.InitializePatcherURL();
		rPatchEvent.EventPatchInitComplete += HandlePatchInitComplete;
		rPatchEvent.EventPatchDownloadFinish += HandlePatchFinish;
		rPatchEvent.EventPatchProgress += HandlePatchProgress;
	}

	private void Start()
	{
		HandlePatchFinish();
	}

	//-----------------------------------------------------------------
	private void HandlePatchInitComplete()
	{
		//SHManagerPatchDownloader.Instance.DoPatcherDownLoadTotalSize((long lTotalSize) =>
		//{
		//	SHManagerPatchDownloader.Instance.DoPatcherStartAssetBundle();
		//});
	}

	private void HandlePatchProgress(string _patchName, long _downloadedByte, long _totalByte, float _percent, uint _loadCurrent, uint _loadMax)
	{
		float fValue = 0;
		if (_totalByte == 0)
		{
			fValue = _loadCurrent / _loadMax;			
		}
		else
		{
			fValue = _percent;
		}

		PatchProgressBar.value = fValue;
		PatchProgressText.text = string.Format("{0:0.00}%", fValue * 100f);
	}

	private void HandlePatchFinish()
	{
		PatchProgressBar.value = 1;
		SHManagerSceneLoader.Instance.DoOpenScenMain("SHSceneMainLoading", null, (string strSceneName) => {
			Debug.Log("[Patch] Finish=====================================");
		});
	}
}
