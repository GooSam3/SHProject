using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class CUIWidgetNumberText : CUIWidgetNumberBase
{
	private const int c_CommaCount = 3;
	private const double c_PercentResolution = 100; // 만분율 사용 10000 = 100%
	[SerializeField]
	private bool DisplayComma = true;
	
	protected CText m_pRefText = null;	
	protected StringBuilder m_pStringBuilder = new StringBuilder(64);
	//-------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pRefText = GetComponent<CText>();
	}
	protected override void OnUIWidgetNumber(long iNumber, List<long> pListDigit)
	{
		if (DisplayComma)
		{
			PrivNumberTextComma(pListDigit);
		}
		else 
		{
			PrivNumberTextPercent(iNumber);
		}
	}

	public void SetTextColor(Color rColor)
	{
		m_pRefText.color = rColor;
	}

	//---------------------------------------------------------
	private void PrivNumberTextComma(List<long> pListDigit)
	{
		m_pStringBuilder.Clear();

		int iCount = c_CommaCount - (pListDigit.Count % c_CommaCount);
		for (int i = 0; i < pListDigit.Count; i++)
		{
			if (iCount++ % c_CommaCount == 0 && i != 0)
			{
				m_pStringBuilder.Append(',');
			}
			m_pStringBuilder.Append(pListDigit[i]);
		}
	
		m_pRefText.text = m_pStringBuilder.ToString();
	}

	private void PrivNumberTextPercent(long iNumber)
	{
		double fValue = iNumber / c_PercentResolution;
		m_pRefText.text = string.Format("{0:0.#}%", fValue);
	}

}
