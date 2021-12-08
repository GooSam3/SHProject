using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class CUIWidgetButtonLongPressBase : CUIWidgetButtonSingleBase
{
	[SerializeField]
	private float EventLongPressDelay = 0;
	[SerializeField]
	private UnityEvent LongPressCount;
	[SerializeField]
	private UnityEvent LongPressStart;
	[SerializeField]
	private UnityEvent LongPressEnd;

	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		
		if (EventLongPressDelay > 0)
		{
			pButton.SetButtonEventLongPress(true, EventLongPressDelay, HandleButtonLongPressStartEnd, HandleButtonLongPressCount);
		}
	}

	//----------------------------------------------------------------------
	private void HandleButtonLongPressStartEnd(bool bStart)
	{
		if (bStart)
		{
			LongPressStart?.Invoke();
		}
		else
		{
			LongPressEnd?.Invoke();
		}

		OnButtonLongPressStartEnd(bStart);
	}

	private void HandleButtonLongPressCount(int iCount)
	{
		LongPressCount?.Invoke();
		OnButtonLongPressCount(iCount);
	}
	//--------------------------------------------------------------------
	protected virtual void OnButtonLongPressStartEnd(bool bStart) { }
	protected virtual void OnButtonLongPressCount(int iCount) { }
}
