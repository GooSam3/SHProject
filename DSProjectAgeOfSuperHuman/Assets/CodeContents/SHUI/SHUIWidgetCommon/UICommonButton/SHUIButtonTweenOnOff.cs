using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
public class SHUIButtonTweenOnOff : CUIWidgetButtonSingleBase
{
	[SerializeField]
	private uTweener Tween = null;

	private bool m_bStart = false;
	//-----------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		Tween.enabled = false;
	}

	protected override void OnButtonClick()
	{
		base.OnButtonClick();
		PrivButtonTweenOnOff(!m_bStart);
	}

	//----------------------------------------------------------------------
	private void PrivButtonTweenOnOff(bool bStart)
	{
		m_bStart = bStart;
		if (m_bStart)
		{
			Tween.enabled = true;
			Tween.Play();
		}
		else
		{
			Tween.enabled = false;
		}
	}
}
