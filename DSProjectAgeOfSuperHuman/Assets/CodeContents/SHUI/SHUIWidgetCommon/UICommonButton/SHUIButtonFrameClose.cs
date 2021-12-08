using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIButtonFrameClose : SHUIButtonNormal
{

	protected override void OnButtonClick()
	{
		base.OnButtonClick();
		SHUIFrameBase pUIFrame = GetUIWidgetParent() as SHUIFrameBase;
		pUIFrame.CloseSelf();

		if (SHManagerGameConfig.Instance.GetGameMode() != SHManagerGameConfig.EGameModeType.Combat)
		{
			UIManager.Instance.DoUIMgrShow<SHUIFrameLobby>();
		}
	}


}
