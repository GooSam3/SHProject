using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uTools;
public class SHUIGaugeEnemy : CUIWidgetGaugeMultipleBase
{
	[SerializeField]
	private uTweenAlpha TweenAlpha = null;

	private bool m_bDeath = false;
	private SHUnitBase m_pUnit = null;
	//---------------------------------------------------------------

	protected override void OnUIWidgetGaugeUpdate()
	{
		base.OnUIWidgetGaugeUpdate();
		if (m_pUnit)
		{
			if ((m_pUnit.GetUnitState() == CUnitBase.EUnitState.DeathStart )&& m_bDeath == false)
			{
				m_bDeath = true;
				PrivUIGaugeTween(false);
			}

			Vector2 HP = m_pUnit.GetUnitHP();
			int iHP = (int)HP.x;
			ProtGaugeMultipleUpdate(HP.x);
		}
	}
	//---------------------------------------------------------------
	public void DoUIGaugeTarget(SHUnitBase pUnit)
	{
		if (pUnit == null)
		{
			m_pUnit = null;
			PrivUIGaugeTween(false);
		}
		else
		{
			m_pUnit = pUnit;
			m_bDeath = false;
			Vector2 HP = pUnit.GetUnitHP();
			uint iHPGaugeScale = pUnit.GetUnitHPGaugeScale();
			float fSectionValue = HP.y / iHPGaugeScale;
			ProtGaugeMultipleReset(HP.y, fSectionValue);
			PrivUIGaugeTween(true);
		}
	}
	
	public void DoUIGaugeDelay(float fDelay)
	{
		ProtGaugeDelaySet(fDelay);
	}

	//-----------------------------------------------------------------
	private void PrivUIGaugeTween(bool On)
	{
		TweenAlpha.Reset();
		if (On)
		{
			TweenAlpha.from = 0;
			TweenAlpha.to = 1;
			TweenAlpha.Play();
		}
		else
		{
			TweenAlpha.from = 1;
			TweenAlpha.to = 0;
			TweenAlpha.Play();
		}
	}
}
