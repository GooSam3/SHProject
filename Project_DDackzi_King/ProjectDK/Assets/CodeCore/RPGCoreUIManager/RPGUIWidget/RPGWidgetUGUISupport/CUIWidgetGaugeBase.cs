using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class CUIWidgetGaugeBase : CUIWidgetBase
{
	[SerializeField]
	private float Speed = 1f;					// 0에서 1까지 1초가 걸린다.

	private Slider m_pSlider = null;
	private float m_fValueDest = 0;			// 실제 도달 수치 
	private float m_fValueCurrent = 0;          // 출력되는 수치 
	private float m_fValueMax = 0;

	private bool  m_bActive = false;		    
	//----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_pSlider = GetComponent<Slider>();
		m_pSlider.value = 1f;
	}

	private void Update()
	{
		OnUIWidgetGaugeUpdate();
		if (m_bActive)
		{
			PrivGaugeUpdate(Time.deltaTime);
		}
	}

	//----------------------------------------------------------
	protected void ProtGaugeValueUpdate(float fDestValue)
	{
		if (m_fValueDest != fDestValue)
		{
			m_fValueDest = fDestValue;
			m_bActive = true;
		}
	}
	
	protected void ProtGaugeReset(float fValueMax)
	{
		m_fValueMax = fValueMax;
		m_fValueCurrent = fValueMax;
		m_pSlider.value = 1f;
	}

	//-----------------------------------------------------------
	private void PrivGaugeUpdate(float fDelta)
	{
		if (m_fValueDest > m_fValueCurrent)  // + 증가
		{
			PrivGaugeUpdateValue(fDelta);
		}
		else if (m_fValueDest < m_fValueCurrent)
		{
			PrivGaugeUpdateValue(-fDelta);
		}
	}

	private void PrivGaugeUpdateValue(float fDeltaValue)
	{
		float fMoveValue = fDeltaValue * m_fValueMax * Speed;
		m_fValueCurrent += fMoveValue;

		if (fDeltaValue > 0)
		{
			if (m_fValueCurrent >= m_fValueDest)
			{
				PrivGaugeMoveEnd(m_fValueCurrent);
			}	
		}
		else if (fDeltaValue < 0)
		{
			if (m_fValueCurrent <= m_fValueDest)
			{
				PrivGaugeMoveEnd(m_fValueCurrent);
			}
		}

		float fSliderValue = 0;

		if (m_fValueCurrent != 0)
		{
			fSliderValue = m_fValueCurrent / m_fValueMax;
		}

		m_pSlider.value = fSliderValue;
	}

	private void PrivGaugeMoveEnd(float fValue)
	{
		m_fValueCurrent = fValue;
		m_bActive = false;
		OnUIWidgetGaugeMoveEnd(fValue);
	}

	//-----------------------------------------------------------
	protected virtual void OnUIWidgetGaugeUpdate() { }
	protected virtual void OnUIWidgetGaugeMoveEnd(float fValue) { }
}
