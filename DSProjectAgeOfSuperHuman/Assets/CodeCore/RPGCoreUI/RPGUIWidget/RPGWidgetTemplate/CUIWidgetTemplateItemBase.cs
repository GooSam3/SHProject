using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetTemplateItemBase : CUIWidgetBase
{
	private int m_iItemIndex = 0;
	private CUIWidgetTemplate m_pParent = null;

	//--------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
	}

	protected override void OnUIWidgetOwner(CUIWidgetBase pWidgetOwner)
	{
		
	}

	//-----------------------------------------------------------
	internal void ImportSetTemplateParent(CUIWidgetTemplate pParent)
	{
		m_pParent = pParent;
	}

	public void DoWTemplatItemShow(bool bShow)
	{
		SetMonoActive(bShow);
		OnUIWidgetTemplateItemShow(bShow);
	}

	public void DoTemplateItemReturn()
	{
		m_pParent.DoTemplateReturn(this);
	}

	public void DoTemplateItemRefresh(int iIndex)
	{
		m_iItemIndex = iIndex;
		OnUIWidgetTemplateItemRefresh(iIndex);
	}

	public int GetTemplateItemIndex()
	{
		return m_iItemIndex;
	}

	//--------------------------------------------------------
	protected virtual void OnUIWidgetTemplateItemShow(bool bShow) { }
	protected virtual void OnUIWidgetTemplateItemRefresh(int iIndex) { }
}
