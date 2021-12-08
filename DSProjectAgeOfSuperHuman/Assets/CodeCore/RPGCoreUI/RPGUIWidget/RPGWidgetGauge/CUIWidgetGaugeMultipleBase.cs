using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUIWidgetGaugeMultipleBase : CUIWidgetGaugeBase
{
    [SerializeField]
    private float		SectionValue = 100000;  
	[SerializeField]
	private CText		CountText = null;
	[SerializeField]
	private CImage FillOver = null;
	[SerializeField]
	private CImage FillUnder = null;
	[SerializeField]
	private List<Color> FillGaugeColor = new List<Color>();

	private int		m_iFillCountCurrent = 0;
	private int      m_iFillGaugeCurrent = 0;
	private int		m_iFillCountMax = 0;
	private float		m_fFillCurrent = 0;

	//----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		
	}

	protected override void OnUIWidgetGaugeMoveEnd(float fValue)
	{
		base.OnUIWidgetGaugeMoveEnd(fValue);
	}

	protected override void OnUIWidgetGaugeUpdate()
	{
		base.OnUIWidgetGaugeUpdate();
		if (pValueDest == 0)
		{
			PrivGaugeMultipleRefresh();
		}
	}
	//------------------------------------------------------------------
	private void PrivGaugeMultipleSelectFillImage(int iGaugeColorCurrent, int iFillCountCurrent)
	{
		int iUnderColor = iGaugeColorCurrent - 1;

		if (iUnderColor < 0) iUnderColor = FillGaugeColor.Count - 1;

		FillOver.color = FillGaugeColor[iGaugeColorCurrent];
		FillUnder.color = FillGaugeColor[iUnderColor];
		FillOver.fillAmount = 1f;
		FillUnder.fillAmount = 1f;

		if (iFillCountCurrent == 0)
		{
			FillUnder.gameObject.SetActive(false);
		}
		else
		{
			FillUnder.gameObject.SetActive(true);
		}
	}

	private void PrivGaugeMultipleCount(int iCurrentCount)
	{
		if (CountText == null) return;

		if (iCurrentCount == 0)
		{
			CountText.gameObject.SetActive(false);
		}
		else
		{
			CountText.gameObject.SetActive(true);
			CountText.text = string.Format("x{0}", iCurrentCount);
		}
	}

	//---------------------------------------------------------------------
	protected void ProtGaugeMultipleReset(float fMaxValue, float fSectionValue = 0)
	{
		if (fSectionValue != 0) SectionValue = fSectionValue;
		
		SectionValue = fSectionValue;
		m_iFillCountMax = (int)(fMaxValue / SectionValue);
		m_iFillCountCurrent = m_iFillCountMax;
		m_fFillCurrent = fMaxValue;
		float fShowValue = fMaxValue % SectionValue;
		if (fShowValue == 0)
		{
			fShowValue = SectionValue;
			m_iFillCountCurrent--;
		}
		m_iFillGaugeCurrent = FillGaugeColor.Count - 1;
		PrivGaugeMultipleSelectFillImage(m_iFillGaugeCurrent, m_iFillCountCurrent);
		PrivGaugeMultipleCount(m_iFillCountCurrent);
		ProtGaugeReset(SectionValue);
		PrivGaugeMultipleFillOver(fShowValue);
		ProtGaugeValueUpdate(fShowValue, true);
	}

	protected void ProtGaugeMultipleUpdate(float fValue, bool bForce = false)
	{
		if (m_fFillCurrent == fValue && bForce == false) return;

		m_fFillCurrent = fValue;
		float fShowValue = fValue % SectionValue;
		int iFillCount = (int)(fValue / SectionValue);
		
		if (iFillCount > m_iFillCountCurrent)
		{
			fShowValue = SectionValue;
		}
		else if (iFillCount != m_iFillCountCurrent)
		{
			fShowValue = 0;
		}
		PrivGaugeMultipleFillOver(fShowValue);
		ProtGaugeValueUpdate(fShowValue);
	}

	//------------------------------------------------------------------------------
	private void PrivGaugeMultipleRefresh()
	{
		m_iFillCountCurrent--;
		m_iFillGaugeCurrent--;

		if (m_iFillGaugeCurrent < 0)
		{
			m_iFillGaugeCurrent = FillGaugeColor.Count - 1;
		}

		if (m_iFillCountCurrent < 0)
		{
			OnGaugeMultipleEnd();
		}
		else
		{
			PrivGaugeMultipleSelectFillImage(m_iFillGaugeCurrent, m_iFillCountCurrent);
			PrivGaugeMultipleCount(m_iFillCountCurrent);
			ProtGaugeReset(SectionValue);
			ProtGaugeMultipleUpdate(m_fFillCurrent, true);
		}
	}

	private void PrivGaugeMultipleFillOver(float fValue)
	{
		float fProgress = fValue / SectionValue;
		FillOver.fillAmount = fProgress;
	}
	//------------------------------------------------------------------------------
	protected virtual void OnGaugeMultipleEnd() { }
}
