using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUIWidgetGauge : CUIWidgetGaugeBase
{
	public enum EGaugeType
	{
		None,
		HP,
		Energy,
	}
	[SerializeField]
	private EGaugeType GaugeType = EGaugeType.None;

	private DKUnitBase m_pTargetUnit = null;
	//-----------------------------------------------------------------
	public void DoGaugeInitialize(DKUnitBase  pTargetUnit)
	{
		m_pTargetUnit = pTargetUnit;
		if (pTargetUnit != null)
		{
			float fValue = pTargetUnit.IAIGetUnitStat(EDKStatType.MaxHealthPoint);
			ProtGaugeReset(fValue);
		}
	}

	//-------------------------------------------------------------------
	protected override void OnUIWidgetGaugeUpdate()
	{
		if (GaugeType == EGaugeType.HP)
		{
			ProtGaugeValueUpdate(m_pTargetUnit.GetDKUnitHP());
		}
		else if (GaugeType == EGaugeType.Energy)
		{
			ProtGaugeValueUpdate(m_pTargetUnit.GetDKUnitEnergy());
		}
	}

}
