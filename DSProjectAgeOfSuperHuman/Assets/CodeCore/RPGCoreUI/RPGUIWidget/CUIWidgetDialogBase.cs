using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetDialogBase : CUIWidgetBase
{

	//-----------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		pParentFrame.ImportUIFrameDialog(true, this);
	}

	protected override void OnUIWidgetDelete(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetDelete(pParentFrame);
		pParentFrame.ImportUIFrameDialog(false, this);
	}

	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		if (bShow)
		{
			GetUIWidgetParent().InternalDialogShow(this);
		}
	}
}
