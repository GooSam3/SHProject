using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIStepLogin : CMonoBase
{
	private bool m_bLoadFinsih = false;
	//----------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
	}

	private void Update()
	{
		if (m_bLoadFinsih == false)
		{ 
			if (UIManager.Instance.IsLoadFinish)
			{
				Debug.Log("[StepLogin] ================= Login UI");
				SHManagerGameConfig.Instance.SetGameMode(SHManagerGameConfig.EGameModeType.Login);
				UIManager.Instance.DoUIMgrScreenLoadingScreenHide();
				UIManager.Instance.DoUIMgrShow<SHUIFrameLogin>();
				m_bLoadFinsih = true;
			}
		}
	}
}
