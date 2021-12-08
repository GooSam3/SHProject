using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
using UnityEngine.Events;

public class ZUIIconRotation : ZUIIconNormal
{
	[SerializeField] uTweenRotation		RotationTween;
	[SerializeField] GameObject		RotationBackSide;
	
	private bool mRotationEnd = false;
	private UnityAction<uint> mEventSelect = null;
	//----------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		RotationTween.enabled = false;
		RotationBackSide.SetActive(true);
	}

	protected override void OnUIIconPointUp(Vector2 _PointPosition)
	{
		if (mRotationEnd)
		{
			base.OnUIIconPointUp(_PointPosition);
		}		
		else
		{
			mEventSelect?.Invoke(pIconID);
		}
	}

	protected override void OnUIIconSettingSprite()
	{
		base.OnUIIconSettingSprite();
		ResetRotation();
	}
	//------------------------------------------------------------------------------
	public void DoUIIconRotationForward(float _delay)
	{
		RotationBackSide.SetActive(true);
		StartTweener(_delay, HandleTweenFinishForward);
	}

	public void DoUIIconRotationEnd()
	{
		mRotationEnd = true;
		RotationBackSide.SetActive(false);
	}

	public void DoUIIconRotationBackward(float _delay)
	{
		RotationBackSide.SetActive(false);
		StartTweener(_delay, HandleTweenFinishBackward);
	}

	public void SetUIIConRotationSelectEvent(UnityAction<uint> _eventSelect)
	{
		mEventSelect = _eventSelect;
	}

	//----------------------------------------------------------------------------
	private void ResetRotation()
	{
		mRotationEnd = false;
	}

	private void StartTweener(float _delay, UnityAction _eventFinish)
	{
		SetIconInputEnable(false);
		RotationTween.enabled = true;
		RotationTween.ResetToBeginning();
		RotationTween.Sample(0, false);
		RotationTween.delay = _delay;
		RotationTween.onFinished.RemoveAllListeners();
		RotationTween.onFinished.AddListener(_eventFinish);
		RotationTween.PlayForward();
	}

	//------------------------------------------------------------------------------
	private void HandleTweenFinishBackward()
	{
		RotationBackSide.SetActive(true);
		RotationTween.PlayReverse();
		RotationTween.onFinished.RemoveAllListeners();
		RotationTween.onFinished.AddListener(HandleTweenFinishBackwardEnd);
	}

	private void HandleTweenFinishBackwardEnd()
	{
		SetIconInputEnable(true);
		RotationTween.enabled = false;
	}

	private void HandleTweenFinishForward()
	{
		RotationBackSide.SetActive(false);
		RotationTween.PlayReverse();
		RotationTween.onFinished.RemoveAllListeners();
		RotationTween.onFinished.AddListener(HandleTweenFinishForwardEnd);
	}
	private void HandleTweenFinishForwardEnd()
	{
		SetIconInputEnable(true);
		RotationTween.enabled = false;
	}
}
