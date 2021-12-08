using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHBuffTaskEffectNormal : SHBuffTaskBase
{
	private EUnitSocket m_eUnitSocket = EUnitSocket.None;
	private string	m_strEffectName;
	private float		m_fDuration = 0;
	private Vector3	m_vecOffset = Vector3.zero;

	//-----------------------------------------------------------------------
	public void SetBuffTaskEffectNormal(EUnitSocket eUnitSocket, string strEffectName, float fDuration, Vector3 vecOffset)
	{
		m_eUnitSocket = eUnitSocket;
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;
		m_vecOffset = vecOffset;
	}
	
}

public class SHBuffTaskEffectAttachCamera : SHBuffTaskBase
{
	private string m_strEffectName;
	private float m_fDuration = 0;
	private SHEffectParticleNormal m_pAttachParticle = null;
	//-----------------------------------------------------------------------
	public void SetBuffTaskEffectAttachCamera(string strEffectName, float fDuration)
	{
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;

	}

	protected override void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		if (m_pAttachParticle != null)
		{
			m_pAttachParticle.DoEffectStart(null);
		}
		else
		{
			SHManagerEffect.Instance.DoMgrEffectRigist(m_strEffectName, (SHEffectParticleNormal pParticleNormal) =>
			{
				pParticleNormal.transform.SetParent(Camera.main.transform, false);
				pParticleNormal.DoEffectStart(null);
				m_pAttachParticle = pParticleNormal;
			});
		}
	}

	protected override void OnBuffTaskEnd(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		base.OnBuffTaskEnd(pBuff, pBuffOwner, pBuffOrigin);
		
		if (m_pAttachParticle != null)
		{
			m_pAttachParticle.DoEffectEnd();
			m_pAttachParticle = null;
		}
	}
}