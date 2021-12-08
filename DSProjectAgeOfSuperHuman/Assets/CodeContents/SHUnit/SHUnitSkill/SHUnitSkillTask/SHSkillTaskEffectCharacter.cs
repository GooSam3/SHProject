using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHSkillTaskEffectCharacter : SHSkillTaskBase
{
	private EUnitSocket m_eUnitSocket = EUnitSocket.None;
	private Vector3 m_vecOffset = Vector3.zero;
	private string m_strEffectName;
	private float m_fDuration = 0f;
	//----------------------------------------------------
	public void SetTaskEffectCharacter(EUnitSocket eUnitSocket, Vector3 vecOffset, string strEffectName, float fDuration)
	{
		m_eUnitSocket = eUnitSocket;
		m_vecOffset = vecOffset;
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;
		SHManagerEffect.Instance.DoMgrEffectPreLoadListAdd(m_strEffectName);
	}

	//-----------------------------------------------------
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillUsage.UsageTarget.IGetUnit().IsAlive == false) return;

		for (int i = 0; i < pListTarget.Count; i++)
		{
			SHUnitBase pUnit = pListTarget[i] as SHUnitBase;
			Transform pTransform = pUnit.GetUnitSocketTransform(m_eUnitSocket);

			if (pTransform)
			{
				Vector3 vecPosition = pTransform.position + m_vecOffset;
				SHManagerEffect.Instance.DoMgrEffectRigist(m_strEffectName, (SHEffectParticleNormal pEffect) =>
				{
					pEffect.DoEffectStart(vecPosition, null, m_fDuration);
				});
			}
		}
	}
}

public class SHSkillTaskEffectTransform : SHSkillTaskBase
{
	private Vector3 m_vecOffset = Vector3.zero;
	private string m_strEffectName;
	private float m_fDuration = 0f;
	//----------------------------------------------------
	public void SetTaskEffectTransform(Vector3 vecOffset, string strEffectName, float fDuration)
	{
		m_vecOffset = vecOffset;
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;
		SHManagerEffect.Instance.DoMgrEffectPreLoadListAdd(m_strEffectName);
	}

	//-----------------------------------------------------
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillUsage.UsageTarget.IGetUnit().IsAlive == false) return;
		if (pSkillOwner.IGetUnitRelationType() == CUnitBase.EUnitRelationType.Enemy) return;

		for (int i = 0; i < pListTarget.Count; i++)
		{
			CUnitBase pUnit = pListTarget[i];
			SHManagerEffect.Instance.DoMgrEffectRigist(m_strEffectName, (CEffectBase pEffect) => {
				pEffect.DoEffectStart(pUnit.transform, null, m_fDuration, m_vecOffset);
			});
		}
	}
}


public class SHSkillTaskEffectCharShake : SHSkillTaskBase
{
	private string m_strEffectName;
	private float m_fDuration = 0f;
	private float m_fStrength = 0f;
	private float m_fRightAngle = 0f;
	//----------------------------------------------------
	public void SetTaskEffectCharShake(string strEffectName, float fDuration, float fStrength, float fRightAngle)
	{	
		m_strEffectName = strEffectName;
		m_fDuration = fDuration;
		m_fStrength = fStrength;
		m_fRightAngle = fRightAngle;
		SHManagerEffect.Instance.DoMgrEffectPreLoadListAdd(m_strEffectName);
	}

	//-----------------------------------------------------
	protected override void OnSkillTaskUse(CStateSkillBase pOwnerState, CSkillUsage pSkillUsage, ISkillProcessor pSkillOwner, List<CUnitBase> pListTarget)
	{
		if (pSkillUsage.UsageTarget.IGetUnit().IsAlive == false) return;
		if (pSkillOwner.IGetUnitRelationType() == CUnitBase.EUnitRelationType.Enemy) return;

		SHUnitEnemy pUnitEnemy = pSkillUsage.UsageTarget.IGetUnit() as SHUnitEnemy;
		if (pUnitEnemy != null)
		{
			if (pUnitEnemy.GetUnitShakeEffect() == null)
			{
				SHManagerEffect.Instance.DoMgrEffectRigist(m_strEffectName, (SHEffectCharacterShake pCharShake) =>
				{
					Vector3 vecDirection = PrivTaskEffectCharShakeDest(m_fStrength);
					pCharShake.DoEffectStart(pUnitEnemy.transform, null, m_fDuration, vecDirection);
				});
			}
		}
	}

	private Vector3 PrivTaskEffectCharShakeDest(float fStrength)
	{
		float fAngle = Random.Range(-m_fRightAngle, m_fRightAngle);
		Vector3 vecDirection = Vector3.right;
		vecDirection = Quaternion.AngleAxis(fAngle, Vector3.forward) * vecDirection;
		vecDirection *= fStrength;
		return vecDirection;
	}
}
