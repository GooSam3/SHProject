using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������ ���ο��� �����ϴ� �۵� ������ ���¿� ���� �پ��� ����� �����ش�.
public abstract class CUIWidgetWindowBase : CUIWidgetBase
{

	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		GetUIWidgetParent().InternalWindowShowHide(this, bShow);
	}
}
