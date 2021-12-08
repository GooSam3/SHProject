using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using uTools;

public class SHUIWidgetNumberTag : CUIWidgetTemplateItemBase
{	
	[SerializeField]
	private TextMeshProUGUI NumberText = null;
	[SerializeField]
	private uTweenPosition TweenPosition = null;
	[SerializeField]
	private uTweenAlpha TweenAlpha = null;
	[SerializeField]
	private uTweenScale TweenScale = null;

	private SHUIFrameNumberTag.ENumberTagType m_eTagType = SHUIFrameNumberTag.ENumberTagType.None;  public SHUIFrameNumberTag.ENumberTagType GetNumberTextSocket() { return m_eTagType; }
	private SHUIFrameNumberTag m_pUIFrameParents = null;
	//-------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pUIFrameParents = pParentFrame as SHUIFrameNumberTag;
	}

	//-------------------------------------------------------
	public void DoNumberText(int iNumber, SHUIFrameNumberTag.ENumberTagType eTagType, Vector3 vecOrigin, Vector3 vecDest)
	{
		m_eTagType = eTagType;
		SetUIPosition(vecOrigin.x, vecOrigin.y);
		PrivNumberTextPosition(vecOrigin, vecDest);
		PrivNumberTextStartTween();
		if (iNumber != 0)
		{
			NumberText.text = iNumber.ToString();
		}
	}

	public void DoNuberAdjustPosition(Vector3 vecOffset)
	{
		if (TweenPosition)
		{
			SetUIPositionMoveY(vecOffset.y);
			TweenPosition.from += vecOffset;
			TweenPosition.to += vecOffset;
		}
	}

	//--------------------------------------------------------
	private void PrivNumberTextPosition(Vector3 vecOrigin, Vector3 vecDest)
	{
		if (TweenPosition == null) return;

		TweenPosition.from =  vecOrigin;
		TweenPosition.to = vecDest;
	}


	private void PrivNumberTextStartTween()
	{
		if (TweenPosition)
		{
			TweenPosition.ResetPlay();
		}

		if (TweenAlpha)
		{
			TweenAlpha.ResetPlay();
		}

		if (TweenScale)
		{
			TweenScale.ResetPlay();		
		}
	}

	//--------------------------------------------------------
	public void HandleNumberTextEnd()
	{
		DoTemplateItemReturn();
	}

}
