using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEventContentBannerOnly : UIEventContentBase
{
	protected override bool SetContent(IngameEventInfoConvert _eventData)
	{
		return true;
	}

	protected override void ReleaseContent()
	{
	}
}
