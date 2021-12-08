using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class SHUIStepLogo : MonoBehaviour
{
	private void Awake()
	{
		CanvasScaler pScaler = GetComponent<CanvasScaler>();
		pScaler.referenceResolution = CApplication.Instance.GetAppBaseResolution();
	}

	public void HandleTweenAlphaEnd()
	{
		Invoke("HandleNextScene", 1);
	}

	public void HandleNextScene()
	{
		SceneManager.LoadScene("SHFrontPatcher");
	}
}
