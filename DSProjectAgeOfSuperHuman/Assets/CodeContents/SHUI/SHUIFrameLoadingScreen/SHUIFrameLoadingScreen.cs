using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SHUIFrameLoadingScreen : SHUIFrameBase
{
	[SerializeField]
	private Slider ProgressBar = null;
	[SerializeField]
	private CText  ProgressText = null;

	private float m_fLoadingTime = 0;
	//-------------------------------------------------------------------------------
	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		m_fLoadingTime = Time.time;
	}

	public void DoUILoadingProgress(float fProgress)
	{
		ProgressBar.value = fProgress;
		int iPercent = (int)(fProgress * 100f);
		ProgressText.text = string.Format("{0}%", iPercent);
	}

}
