using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetButtonRadioBase : CUIWidgetButtonToggleBase
{
	private static CMultiSortedDictionary<string, CUIWidgetButtonRadioBase> g_mapRadioGroup = new CMultiSortedDictionary<string, CUIWidgetButtonRadioBase>();

	[SerializeField][Header("[Button Radio]")]
	private string RadioGroup;
	//--------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		PrivButtonRadioRegist();
	}

	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		PrivButtonRadioRegist();
		base.OnUIWidgetInitialize(pParentFrame);
	}

	protected override void OnButtonClick()
	{
		if (m_bToggleOn) return;

		base.OnButtonClick();
	}

	protected override void OnButtonToggleOn()
	{
		base.OnButtonToggleOn();
	}

	protected override void OnButtonToggleEventFire(bool bOnEvent)
	{
		base.OnButtonToggleEventFire(bOnEvent);
		if (bOnEvent)
		{
			PrivButtonRadioFocus();
		}
	}

	//-------------------------------------------------------------------
	internal void ImportButtonRadioFocusOff()
	{
		DoButtonToggleOff();
	}

	//------------------------------------------------------------------
	private void PrivButtonRadioRegist()
	{
		if (RadioGroup == string.Empty) return;

		if (g_mapRadioGroup.ContainsKey(RadioGroup))
		{
			List<CUIWidgetButtonRadioBase> pList = g_mapRadioGroup[RadioGroup];
			if (pList.Contains(this) == false)
			{
				pList.Add(this);
			}
		}
		else
		{
			g_mapRadioGroup.Add(RadioGroup, this);
		}
	}

	private void PrivButtonRadioFocus()
	{
		if (RadioGroup == string.Empty) return;
		List<CUIWidgetButtonRadioBase> pListRadio = g_mapRadioGroup[RadioGroup];

		for (int i = 0; i < pListRadio.Count; i++)
		{
			if (pListRadio[i] != this)
			{
				pListRadio[i].ImportButtonRadioFocusOff();
			}
		}
	}
}
