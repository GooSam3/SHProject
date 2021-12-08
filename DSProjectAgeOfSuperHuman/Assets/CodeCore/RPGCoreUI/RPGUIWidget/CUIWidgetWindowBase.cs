using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 프레임 내부에서 동작하는 작동 단위로 형태에 따라 다양한 모습을 보여준다.
public abstract class CUIWidgetWindowBase : CUIWidgetBase
{

	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		GetUIWidgetParent().InternalWindowShowHide(this, bShow);
	}
}
