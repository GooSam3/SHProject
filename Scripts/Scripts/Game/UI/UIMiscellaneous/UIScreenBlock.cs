using UnityEngine;

public class UIScreenBlock : CUIFrameScreenBlockBase
{
	[SerializeField]
	private uTools.uTweenAlpha BackgroundTween;

	//----------------------------------------------------------------
	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
		if (BackgroundTween)
		{
			BackgroundTween.ResetToBeginning();
			BackgroundTween.PlayForward();
		}
	}
}
