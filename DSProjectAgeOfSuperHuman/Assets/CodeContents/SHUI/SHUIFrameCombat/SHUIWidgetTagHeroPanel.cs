using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
public class SHUIWidgetTagHeroPanel : CUIWidgetBase
{
	[SerializeField]
	private CText Level = null;
	[SerializeField]
	private uTweenPosition TweenPosition;

	private float m_fEndPosition = 0;
	private Vector3 m_vecStartPosition = Vector3.zero;
	//----------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		RectTransform pRect = GetRectTransform();
		m_vecStartPosition = pRect.localPosition;
	}

	public void DoTagHeroPanelStart(int iLevel)
	{
		Level.text = string.Format("Lv {0}", iLevel);
		PrivTagHeroPanelTweenStart();
	}

	//-------------------------------------------------------------------
	private void PrivTagHeroPanelTweenStart()
	{
		DoUIWidgetShowHide(true);

		float fScreenWidth = UIManager.Instance.GetUIScreenSize().x;
		RectTransform pRect = GetRectTransform();
		m_fEndPosition = fScreenWidth - (pRect.rect.width / 2);
		

		Vector3 vecEndPosition = m_vecStartPosition;
		vecEndPosition.x = m_fEndPosition;
		TweenPosition.from = m_vecStartPosition;
		TweenPosition.to = vecEndPosition;


		TweenPosition.ResetPlay();
	}
	//-------------------------------------------------------------------
	public void HandleTagHeroPanelFinish()
	{
		DoUIWidgetShowHide(false);
	}

}
