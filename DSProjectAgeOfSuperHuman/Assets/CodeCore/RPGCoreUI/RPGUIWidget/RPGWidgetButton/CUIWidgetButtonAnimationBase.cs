using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetButtonAnimationBase : CUIWidgetButtonLockBase
{
	private const string c_ButtonAniName = "PlayButton";

	[SerializeField][Header("[Button Animation]")]
	private CUIAnimationController ButtonAnimation = null;
   
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		ButtonAnimation = GetComponent<CUIAnimationController>();
	}

	protected override void OnButtonClick()
	{
		if (ButtonAnimation == null)
		{
			base.OnButtonClick();
		}
		else
		{
			ButtonAnimation.DoAnimationPlay(c_ButtonAniName, (string strAniName) => {
				base.OnButtonClick();
			});
		}
	}
}
