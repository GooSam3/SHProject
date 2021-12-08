using System;
using System.Collections.Generic;
using UnityEngine;

abstract public class CUGUIButtonToggleBase : CUGUIButtonBase
{
	[Serializable]
	protected class SToggleGraphic
	{
		public UnityEngine.UI.Graphic SelectObject = null;
		public Color SelectColor = Color.white;
	}
	[SerializeField] protected List<SToggleGraphic> ToggleGraphic = new List<SToggleGraphic>();
	[SerializeField][Tooltip("입력하면 토글 On버튼으로 작동. 입력하지 않으면 Off버튼으로 작동")] protected CUGUIButtonToggleBase TogglePair = null;
	[SerializeField] protected bool DefaultToggleOff = true;

	protected CUGUIButtonToggleBase mToggleOnPair = null;
	private bool				     mToggleOn = false;    public bool pToggleOn { get { return mToggleOn; } }
	private bool					 mToggleFocus = false; public bool pToggleFocus { get { return mToggleFocus; } }  
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);

		if (TogglePair != null)
		{
			if (TogglePair.IsToggleOnwer() == false)
			{
				ImportSetToggleOn();                                // 자신은 온으로 셋팅
				TogglePair.ImportSetToggleOff(this);                // 페어 버튼은 오프로 셋팅				
			}
			else
			{
				TogglePair = null;
				ZLog.LogError(ZLogChannel.UI, "[UIToggle] Toggles should not be paired whith each other. plz delete pair ");
			}
		}
	}

	protected override void OnUIWidgetInitializePost(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitializePost(_UIFrameParent);
		
		if (DefaultToggleOff && IsToggleOnwer())
        {
			DoToggleAction(false);
		}
	}

    protected override void OnUIWidgetCommandExcute() 
	{
		base.OnUIWidgetCommandExcute();
		DoToggleAction(false);
	}

	protected override void OnUIWidgetSetText(string _Text)
	{
		base.OnUIWidgetSetText(_Text);
		if (TogglePair && IsToggleOnwer())
		{
			TogglePair.SetUIWidgetText(_Text);
		}
	}

	protected override void OnUIButtonInteractionOn(bool _interaction) 
	{
		if (TogglePair != null)
		{
			TogglePair.SetUIButtonInteraction(_interaction);
		}
	}

	protected override void OnAlarmOnOff(bool _on, int _alarmCount) 
	{
		if (TogglePair)
		{
			if (_on)
			{
				TogglePair.DoAlarmActionAdd(_alarmCount);
			}
			else
			{
				TogglePair.DoAlarmActionDelete(_alarmCount);
			}
		}
	}

	//-------------------------------------------------------------------------
	public void DoToggleAction(bool _Show, bool _excuteButtonEvent = false)
	{				
		if (_Show)
		{
			ToggleActionOn();

			if (_excuteButtonEvent)
			{
				DoUIButtonClickEvent();
			}
		}
		else
		{
			ToggleActionOff();

			if (_excuteButtonEvent)
			{
				TogglePair.DoUIButtonClickEvent();
			}
		}
		ToggleGraphicRefresh();
	}

	public bool IsToggleOnwer()
	{	
		return TogglePair != null;
	}

	public void SetTogglePair(CUGUIButtonToggleBase _uiButtonToggle)
	{
		TogglePair = _uiButtonToggle;
	}

	public void SetToggleSprite(Sprite _sprite)
	{
		SetButtonImage(_sprite);

		if (TogglePair)
		{
			TogglePair.SetButtonImage(_sprite);
		}
	}

	//-------------------------------------------------------------

	private void ToggleActionOn()
	{	
		if (TogglePair != null)
		{
			TogglePair.ToggleActionRemoteOff();
		}
		else if (mToggleOnPair != null)
		{
			mToggleOnPair.ToggleActionRemoteOff();
		}

		PrivToggleShowHide(true);
	}

	private void ToggleActionRemoteOn()
	{
		PrivToggleShowHide(true);
	}

	private void ToggleActionOff()
	{
		if (TogglePair != null)
		{
			TogglePair.ToggleActionRemoteOn();
		}
		else if (mToggleOnPair != null)
		{
			mToggleOnPair.ToggleActionRemoteOn();
		}
		PrivToggleShowHide(false);
	}

	private void ToggleActionRemoteOff()
	{
		PrivToggleShowHide(false);
	}

	//--------------------------------------------------------------
	private void ImportSetToggleOn()
	{
		SetMonoActive(true);
		mToggleOn = true;
		OnUIToggleInitializeOn();
	}

	private void ImportSetToggleOff(CUGUIButtonToggleBase _ToggleOnButton)
	{
		SetMonoActive(false);
		TogglePair = null;
		mToggleOnPair = _ToggleOnButton;
		mToggleOn = false;
		
		OnUIToggleInitializeOff(_ToggleOnButton);
	}

	private void PrivToggleShowHide(bool _Show)
	{
		mToggleFocus = _Show;

		if (_Show)
		{
			SetMonoActive(true);
			OnUIToggleShow();
		}
		else
		{
			SetMonoActive(false);
			OnUIToggleHide();
		}
	}

	private void ToggleGraphicRefresh()
	{
		for (int i = 0; i < ToggleGraphic.Count; i++)
		{
			if (ToggleGraphic[i].SelectObject != null)
			{
				ToggleGraphic[i].SelectObject.color = ToggleGraphic[i].SelectColor;
			}
		}
	}
	
	//------------------------------------------------------------
	protected virtual void OnUIToggleInitializeOn() { }
	protected virtual void OnUIToggleInitializeOff(CUGUIButtonToggleBase _ToggleOnButton) { }	
	protected virtual void OnUIToggleShow() { }
	protected virtual void OnUIToggleHide() { }
}
