using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameContentsSelect : SHUIFrameBase
{
	[SerializeField]
	private SHUIWidgetContentsAdventure ContentsAdventure = null;
	[SerializeField]
	private SHUIWidgetContentsScenario ContentsScenario = null;
	//------------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
		ContentsAdventure.DoUIWidgetShowHide(true);
		ContentsScenario.DoUIWidgetShowHide(false);
	}

	//-----------------------------------------------------------------------------
	public void HandleContentsSelectAdventure()
	{
		ContentsAdventure.DoUIWidgetShowHide(true);
		ContentsScenario.DoUIWidgetShowHide(false);
	}

	public void HandleContentsSelectScenario()
	{
		ContentsAdventure.DoUIWidgetShowHide(false);
		ContentsScenario.DoUIWidgetShowHide(true);
	}
}

