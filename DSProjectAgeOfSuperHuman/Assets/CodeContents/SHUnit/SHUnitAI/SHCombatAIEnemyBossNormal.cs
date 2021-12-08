using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 다른 조건 없이 쿨타임이 되면 테그를 시도한다.
public class SHCombatAIEnemyBossNormal : SHCombatAIEnemyBase
{
	private static float g_fGlobalTagCoolTimeCurrent = 0;

	[SerializeField]
	private int TagCoolTimeMin = 5;
	[SerializeField]
	private int TagCoolTimeMax = 7;

	private float m_fTagCooltime = 0;
	private bool m_bRefreshOwner = false;
	//------------------------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();

		m_fTagCooltime = Random.Range(TagCoolTimeMin, TagCoolTimeMax);
	}
	protected override void OnCombatAIUpdate()
	{
		// 테그 오프일 경우 확인
		if (m_pSHUnit.GetUnitState() == CUnitBase.EUnitState.PhaseOut)
		{
			UpdateCombatAIRefreshOwner();
		}
		else
		{
			base.OnCombatAIUpdate();
		}
	}

	//-----------------------------------------------------------------------------------
	private void UpdateCombatAIRefreshOwner()
	{
		if (m_bRefreshOwner)
		{
			UpdateCombatAIRefresh();
		}
		else
		{
			if (g_fGlobalTagCoolTimeCurrent == 0)
			{
				m_bRefreshOwner = true;
				UpdateCombatAIRefresh();
			}
		}
	}

	private void UpdateCombatAIRefresh()
	{
		float fDelta = Time.deltaTime;
		g_fGlobalTagCoolTimeCurrent += fDelta;
		if (g_fGlobalTagCoolTimeCurrent >= m_fTagCooltime)
		{
			PrivCombatAITrayTagOn();
		}
	}

	private void PrivCombatAITrayTagOn()
	{
		SHUnitEnemy pReaderEnemy = SHManagerUnit.Instance.GetUnitEnemy();
		// 활성 상태인지
		if (pReaderEnemy.IsAlive)
		{
			if (NBuff.SHBuffEnum.HasCrowdControl(pReaderEnemy.GetUnitCrowdControll()) == false)
			{
				if (pReaderEnemy.IsUnitSkillPlay() == false)
				{
					PrivCombatAITagOn();
				}
			}
		}
		else
		{

		}
	}

	private void PrivCombatAITagOn()
	{
		ProtCombatAIReset();
		m_bRefreshOwner = false;
		g_fGlobalTagCoolTimeCurrent = 0;
		m_fTagCooltime = Random.Range(TagCoolTimeMin, TagCoolTimeMax);
		SHManagerUnit.Instance.DoMgrUnitEnemyTagOn(m_pSHUnit as SHUnitEnemy, null);
	}
}

