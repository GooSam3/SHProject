using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHScreenIdleMode : CMonoBase
{
	private float m_fIdleModeStart = 500f; // 이 시간동안 입력이 없으면 방치모드로 이행한다.
	private bool m_bFirstReset = true;
	private SHUIFrameScreenIdleMode m_pScreenIdleMode = null;
	//----------------------------------------------------------------------
	protected override void OnUnityStart()
	{
		base.OnUnityStart();
	}

	private void Update()
	{
		if (m_bFirstReset)
		{
			UpdateScreenIdleInitSceen();
		}
		else
		{
			UpdateScreenIdleModeStart();
		}
	}

	//--------------------------------------------------------------------
	private void UpdateScreenIdleModeStart()
	{
		SHUIFrameScreenIdleMode pScreen = FindScreen();
		if (pScreen != null)
		{
			if (pScreen.pShow == false)
			{
				float fInputTime = UIManager.Instance.GetUIInputRefresh();
				if (fInputTime >= m_fIdleModeStart)
				{
					UIManager.Instance.DoUIMgrShow<SHUIFrameScreenIdleMode>();
					UIManager.Instance.DoUIMgrInputRefresh(true);
				}
			}
		}
	}

	private void UpdateScreenIdleInitSceen()
	{
		if (UIManager.Instance != null)
		{
			UIManager.Instance.DoUIMgrInputRefresh();
			m_bFirstReset = false;
		}
	}
	//----------------------------------------------------------------------
	private SHUIFrameScreenIdleMode FindScreen()
	{
		if (m_pScreenIdleMode != null)
		{
			return m_pScreenIdleMode;
		}
		if (UIManager.Instance == null)
		{
			return null;
		}

		return UIManager.Instance.DoUIMgrFind<SHUIFrameScreenIdleMode>();
	}
}
