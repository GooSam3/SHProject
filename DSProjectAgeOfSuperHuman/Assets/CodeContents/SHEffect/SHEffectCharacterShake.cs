using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHEffectCharacterShake : CEffectTransformBase
{
	private SHUnitEnemy m_pEnemy = null;
	//-----------------------------------------------------------------
	protected override void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams)
	{
		m_pEnemy = pFollow.gameObject.GetComponent<SHUnitEnemy>();
		if (m_pEnemy)
		{
			CEffectBase pShakeEffect = m_pEnemy.GetUnitShakeEffect();

			if (pShakeEffect is SHEffectNone || pShakeEffect == null)
			{
				m_pEnemy.SetUnitShakeEffect(this);
				ProtEffectTransformRefreshInstance(0, pFollow, vecDest);
				base.OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);
			}
		}
	}
	protected override void OnEffectEnd(bool bForce)
	{
		base.OnEffectEnd(bForce);
		if (m_pEnemy != null)
		{
			m_pEnemy.SetUnitShakeEffect(null);
			m_pEnemy = null;
		}
	}

	//-------------------------------------------------------------------
	public void HandleEffectCharacterShakeEnd()
	{
		DoEffectEnd();
	}
}

