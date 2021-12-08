using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(GraphicRaycaster))]
abstract public class CUIWidgetPopupBase : CUGUIWidgetBase
{
	[SerializeField] int	SortOffset = 10;
	[SerializeField] bool ShowOnly = true;  //  true 일 경우 이 팝업이 켜지면 다른 팝업은 내려간다.
	private Canvas mCanvas = null;
	//----------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
	}

	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mCanvas = GetComponent<Canvas>();
		if (SortOffset < 3) SortOffset = 2;
	}

	protected override void OnUIWidgetShowHide(bool _Show)
	{
		base.OnUIWidgetShowHide(_Show);

		if (ShowOnly && _Show) 
		{
			CUIFrameWidgetBase uiFrameWidget = mUIFrameParent as CUIFrameWidgetBase;
			uiFrameWidget.ImportUIWidgetPopupShow(this);
			mCanvas.overrideSorting = true;
		}
	}

	//-----------------------------------------------------------------
	public void ImportWidgetPopupRefreshLayer(int _layerOrder)
	{
		RefreshPopupLayer(_layerOrder);
	}

	//----------------------------------------------------------------
	private void RefreshPopupLayer(int _layerOrder)
	{
		mCanvas.overrideSorting = true;
		mCanvas.sortingLayerID = mUIFrameParent.LayerID;
		mCanvas.sortingOrder = _layerOrder + SortOffset;
		OnWidgetPopupRefreshLayer(_layerOrder);
	}

	//---------------------------------------------------------------
	protected virtual void OnWidgetPopupRefreshLayer(int _layerOrder) { }
	

}
