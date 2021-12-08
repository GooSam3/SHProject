using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 켄버스를 사용하므로 드로우콜을 발생시킴
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class CUIWidgetCanvasBase : CUIWidgetBase
{
	[SerializeField]
	private int OrderOffset = 1;

	private Canvas m_pWidgetCanvas = null;
	//-------------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pWidgetCanvas = GetComponent<Canvas>();
	}

	protected override void OnUIWidgetRefreshOrder(int iOrder)
	{
		base.OnUIWidgetRefreshOrder(iOrder);
		PrivWidgetCanvasInitialize(GetUIWidgetParent().GetUIFrameCanvas());
	}

	protected override void OnUIWidgetAdd(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetAdd(pParentFrame);
		PrivWidgetCanvasInitialize(pParentFrame.GetUIFrameCanvas());
	}

	//----------------------------------------------------------------------
	private void PrivWidgetCanvasInitialize(Canvas pFrameCanvas)
	{
		m_pWidgetCanvas.overrideSorting = true;
		m_pWidgetCanvas.sortingLayerID = pFrameCanvas.sortingLayerID;
		m_pWidgetCanvas.sortingOrder = pFrameCanvas.sortingOrder + OrderOffset;
	}
}
