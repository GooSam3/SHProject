using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public abstract class CUIWidgetGaugeBase : CUIWidgetBase
{
	[SerializeField]
	private float Duration = 0.5f;	protected float GetGaugeDuration() { return Duration; }

	[SerializeField]
	private float Delay = 0f;

	[SerializeField]
	private float MinValue = 0;
	

	private Slider	m_pSlider = null;
	private float		m_fValueDest = 0;		public float pValueDest { get { return m_fValueDest; } }// 실제 도달 수치 
	private float		m_fValueCurrent = 0;	public float pValueCurrent { get { return m_fValueCurrent; } }// 출력되는 수치 
	private float		m_fDelayCurrent = 0;
	private float		m_fValueMax = 0;
	private float		m_fSpeed = 0;
	private float		m_fDelay = 0;
	private bool		m_bActive = false;	public bool pActive { get { return m_bActive; } }
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
			PrivGaugeUpdateValue(Time.deltaTime);
		}
	}

	//----------------------------------------------------------
	protected void ProtGaugeValueUpdate(float fDestValue, bool bForce = false)
	{
		if (fDestValue < 0) fDestValue = 0;
		if (fDestValue > m_fValueMax) fDestValue = m_fValueMax;

		if (m_fValueCurrent == fDestValue)
		{
			m_bActive = false;
			return;
		}

		if (bForce)
		{
			PrivGaugeMoveEnd(fDestValue);
		}
		else
		{
			PrivGaugeStart(fDestValue);
		}
	}
	
	protected void ProtGaugeReset(float fValueMax)
	{
		m_fValueMax = fValueMax;
		m_fValueCurrent = fValueMax;
		m_fValueDest = fValueMax;
		m_fDelayCurrent = 0;
		m_pSlider.value = 1f;
		m_fDelay = Delay;
		ProtGaugeDelaySet(0);
	}

	protected void ProtGaugeColor(Color rColor)
	{
		CImage pImage = m_pSlider.fillRect.gameObject.GetComponent<CImage>();
		if (pImage)
		{
			pImage.color = rColor;
		}
	}

	protected RectTransform GetGaugeFillRect()
	{
		return m_pSlider.fillRect;
	}

	protected void ProtGaugeDelaySet(float fDelay)
	{
		Delay = fDelay;
		m_fDelay = fDelay;
		m_fDelayCurrent = 0f;
	}

	protected float ProtGaugeDelayGet()
	{
		return m_fDelay;
	}

	//-----------------------------------------------------------
	private void PrivGaugeStart(float fDestValue)
	{
		m_fSpeed = (m_fValueCurrent - fDestValue) / Duration;
		
		m_fValueDest = fDestValue;
		if (m_fValueDest < 0) m_fValueDest = 0;
		if (m_fValueDest > m_fValueMax) m_fValueDest = m_fValueMax;
		m_fDelayCurrent = 0;
		m_bActive = true;
	}

	private void PrivGaugeUpdateValue(float fDelta)
	{
		if (m_fDelayCurrent < m_fDelay)
		{
			m_fDelayCurrent += fDelta;
			return;
		}

		m_fValueCurrent -= (m_fSpeed * fDelta);


		if (m_fSpeed > 0)
		{
			if (m_fValueCurrent <= m_fValueDest)
			{
				PrivGaugeMoveEnd(m_fValueDest);
			}
			else
			{
				PrivGaugeApplySliderValue(m_fValueCurrent);
			}
		}
		else
		{
			if (m_fValueCurrent >= m_fValueDest)
			{
				PrivGaugeMoveEnd(m_fValueDest);
			}
			else
			{
				PrivGaugeApplySliderValue(m_fValueCurrent);
			}
		}

	}

	private void PrivGaugeMoveEnd(float fValue)
	{
		if (fValue < 0) fValue = 0;

		PrivGaugeApplySliderValue(fValue);
		m_fValueCurrent = fValue;
		m_fValueDest = fValue;
		m_fDelayCurrent = 0;
		m_bActive = false;

		OnUIWidgetGaugeMoveEnd(fValue);
	}

	private void PrivGaugeApplySliderValue(float fValue)
	{
		if (fValue != 0)
		{
			float fSliderValue = fValue / m_fValueMax;
			if (fSliderValue <= MinValue)
			{
				fSliderValue = 0;
			}

			m_pSlider.value = fSliderValue;
		}
		else
		{
			m_pSlider.value = 0;
		}
	}

	//-----------------------------------------------------------
	protected virtual void OnUIWidgetGaugeUpdate() { }
	protected virtual void OnUIWidgetGaugeMoveEnd(float fValue) { }
}
