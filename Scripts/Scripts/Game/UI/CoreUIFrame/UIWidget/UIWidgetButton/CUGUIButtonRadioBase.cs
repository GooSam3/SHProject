using System.Collections.Generic;
using UnityEngine;
abstract public class CUGUIButtonRadioBase : CUGUIButtonToggleBase
{
	[System.Serializable]
	protected class SRadioGraphic 
	{
		public UnityEngine.UI.Graphic SelectObject = null;
		public Color SelectColor = Color.white;
	}

	[SerializeField] private bool DefaultRadioClick = false;
	[SerializeField] protected List<SRadioGraphic> RadioGraphic = new List<SRadioGraphic>(); 

	private int mRadioButtonGroup = 0; public int pRadioButtonGroup { get { return mRadioButtonGroup; } }
	private CUIFrameWidgetBase mUIFrameWidget = null;
	//----------------------------------------------------------------------
	protected void SetRadioGroup(int _RadioGroup)
	{
        mRadioButtonGroup = _RadioGroup;
		mUIFrameWidget = mUIFrameParent as CUIFrameWidgetBase;		
	}

	public void DoRadioButtonToggleOn(bool _doEvent = true)
	{
		if (IsToggleOnwer())
		{
			DoToggleAction(true);
			if (mUIFrameWidget != null)
				mUIFrameWidget.ImportUIWidgetRadioSelect(mRadioButtonGroup, this);

			if (_doEvent)
			{
				DoUIButtonClickEvent();
			}
		}
	}

	//---------------------------------------------------------------------
	protected override void OnUIWidgetCommandExcute()
	{
		if (pToggleOn == false)
		{					
			mToggleOnPair.DoUIButtonClickEvent();
		}
		else
		{
			DoToggleAction(true);
			mUIFrameWidget.ImportUIWidgetRadioSelect(mRadioButtonGroup, this);			
		}
	}

	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{	
		base.OnUIWidgetInitialize(_UIFrameParent);

		if (IsToggleOnwer())
		{
			if (mUIFrameWidget.ImportUIWidgetRadioRegist(mRadioButtonGroup, this))
			{
				DefaultRadioClick = true;
			}
		}
	}

	protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent) 
	{
		base.OnUIWidgetInitializePost(_UIFrameParent);
		if (DefaultRadioClick && IsToggleOnwer())
		{
			DefaultClick();
		}
	}

	protected override void OnUnityStart()
	{
		base.OnUnityStart();
	}

	protected override void OnUIToggleShow()
	{
		base.OnUIToggleShow();
		RadioGraphicRefresh();	
	}

	protected override void OnUIWidgetRemove()
	{
		base.OnUIWidgetRemove();
		if (IsToggleOnwer())
		{
			mUIFrameWidget.ImportUIWidgetRadioUnRegist(mRadioButtonGroup, this);
		}
	}

	//---------------------------------------------------------------------
	private void DefaultClick()
	{
		DefaultToggleOff = false;
		mUIFrameWidget.ImportUIWidgetRadioSelect(mRadioButtonGroup, this);
		DoToggleAction(true);
	}

	private void RadioGraphicRefresh()
	{
		for (int i = 0; i < RadioGraphic.Count; i++)
		{
			if (RadioGraphic[i].SelectObject != null)
			{
				RadioGraphic[i].SelectObject.color = RadioGraphic[i].SelectColor;
			}
		}
	}
}
