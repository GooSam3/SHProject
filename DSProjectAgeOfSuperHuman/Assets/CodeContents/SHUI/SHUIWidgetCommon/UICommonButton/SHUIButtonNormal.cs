using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIButtonNormal : CUIWidgetButtonAnimationBase
{

	//-----------------------------------------------------
	protected override void OnButtonPointUp()
	{
		base.OnButtonPointUp();
		UIManager.Instance.DoUIMgrInputRefresh();
	}
}
