using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public abstract class CUIWidgetButtonToggleBase : CUIWidgetButtonAnimationBase
{
	[SerializeField][Header("[Button Toggle]")]
	private MaskableGraphic ToggleImageOn = null;
	[SerializeField]
	private MaskableGraphic ToggleImageOff = null;
	[SerializeField]
	private Color ToggleColorOn = Color.white;
	[SerializeField]
	private Color ToggleColorOff = Color.white;
	[SerializeField]
	private bool	FirstOn = false;

	[SerializeField]
	private UnityEvent ToggleEventOn = null;
	[SerializeField]
	private UnityEvent ToggleEventOff = null;

	protected bool m_bToggleOn = false;
	private List<MaskableGraphic> m_listGraphicToggleOn = new List<MaskableGraphic>();
	private List<MaskableGraphic> m_listGraphicToggleOff = new List<MaskableGraphic>();
	//----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);

		if (ToggleImageOn)
		{
			ToggleImageOn.gameObject.GetComponentsInChildren(true, m_listGraphicToggleOn);
		}

		if (ToggleImageOff)
		{
			ToggleImageOff.gameObject.GetComponentsInChildren(true, m_listGraphicToggleOff);
		}
	}

	protected override void OnUIWidgetInitializePost(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitializePost(pParentFrame);
		if (FirstOn)
		{
			DoButtonToggleOn(false);
		}
		else
		{
			DoButtonToggleOff(false);
		}
	}

	protected override void OnButtonClick()
	{
		base.OnButtonClick();

		if (m_bToggleOn)
		{
			DoButtonToggleOff();
		}
		else
		{
			DoButtonToggleOn();
		}
	}

	//---------------------------------------------------------
	public void DoButtonToggleOn(bool bEventFire = true) // 관련 로직 만들때 무한 루프가 걸리는 경우가 많으니 조심할것
	{
		PrivButtonToggleOn();
		if (bEventFire)
		{
			ToggleEventOn?.Invoke();
			OnButtonToggleEventFire(true);
		}
		OnButtonToggleOn();
	}

	public void DoButtonToggleOff(bool bEventFire = true)
	{
		PrivButtonToggleOff();
		if (bEventFire)
		{
			ToggleEventOff?.Invoke();
			OnButtonToggleEventFire(false);
		}
		OnButtonToggleOff();
	}

	//----------------------------------------------------------
	private void PrivButtonToggleOn()
	{
		m_bToggleOn = true;

		if (ToggleImageOff)
		{
			ToggleImageOff.gameObject.SetActive(false);
		}

		if (ToggleImageOn)
		{
			ToggleImageOn.gameObject.SetActive(true);
			PrivButtonToggleGraphicColor(m_listGraphicToggleOn, ToggleColorOn);
		}
	}

	private void PrivButtonToggleOff()
	{
		m_bToggleOn = false;
		if (ToggleImageOn)
		{
			ToggleImageOn.gameObject.SetActive(false);
		}

		if (ToggleImageOff)
		{
			ToggleImageOff.gameObject.SetActive(true);
			PrivButtonToggleGraphicColor(m_listGraphicToggleOn, ToggleColorOff);
		}
	}

	private void PrivButtonToggleGraphicColor(List<MaskableGraphic> pListGraphic, Color rColor)
	{
		for (int i = 0; i < pListGraphic.Count; i++)
		{
			pListGraphic[i].color = rColor;
		}
	}
	//---------------------------------------------------------

	protected virtual void OnButtonToggleOn() { }
	protected virtual void OnButtonToggleOff() { }
	protected virtual void OnButtonToggleEventFire(bool bOnEvent) { }
}
