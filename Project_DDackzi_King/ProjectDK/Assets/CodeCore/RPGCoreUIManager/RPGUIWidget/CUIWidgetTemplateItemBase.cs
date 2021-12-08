using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetTemplateItemBase : CUIWidgetBase
{

	private CUIWidgetTemplate m_pParent = null;
	//--------------------------------------------------------
	internal void ImportSetTemplateParent(CUIWidgetTemplate pParent)
	{
		m_pParent = pParent;
	}

	public void DoWidgetItemShow(bool bShow)
	{
		SetMonoActive(bShow);
		OnUIWidgetTemlateItemShow(bShow);
	}

	public void DoWidgetItemReturn()
	{
		m_pParent?.DoUIWidgetTemplateReturn(this);
	}

	//--------------------------------------------------------
	protected virtual void OnUIWidgetTemlateItemShow(bool bShow) { }
}
