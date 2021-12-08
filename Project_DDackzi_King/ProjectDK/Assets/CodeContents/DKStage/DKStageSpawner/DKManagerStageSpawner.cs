using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKManagerStageSpawner : CManagerStageSpawnerBase
{ public static new DKManagerStageSpawner Instance { get { return CManagerStageSpawnerBase.Instance as DKManagerStageSpawner; } }
	
	private bool m_bFormationMove = false;
	private float m_fEncountLength = 0;
	private DKFormationHero m_pFormationHero = new DKFormationHero();
	private DKFormationEnemy m_pFormationEnemy = new DKFormationEnemy();
	//--------------------------------------------------------------
	protected override void OnUnitUpdate()
	{
		base.OnUnitUpdate();
		if (m_bFormationMove)
		{
			UpdateMgrStageSpawnerEncounter();
		}

		UpdateMgrStageSpawnerFormation();
	}
	//--------------------------------------------------------------
	public void SetMgrStageSpawnerFormation(bool bHero, DKUnitBase pUnit, bool bReader)
	{
		if (bHero)
		{
			m_pFormationHero.SetFormationMember(pUnit, bReader);
		}
		else
		{
			m_pFormationEnemy.SetFormationMember(pUnit, bReader);
		}
	}

	public void DoMgrStageSpawnerMoveEnemyFormation(float fEncountLength)
	{
		m_fEncountLength = fEncountLength;
		m_bFormationMove = true;
		DKUnitBase pReaderEnemy = m_pFormationEnemy.GetFormationReader();
		DKUnitBase pReaderHero = m_pFormationHero.GetFormationReader();
		Vector3 vecDirection = pReaderEnemy.GetUnitPosition() - pReaderHero.GetUnitPosition();
		if (vecDirection.x >= 0)
		{
			m_pFormationHero.SetFormationDirection(true);
			m_pFormationEnemy.SetFormationDirection(false);
		}
		else
		{
			m_pFormationHero.SetFormationDirection(false);
			m_pFormationEnemy.SetFormationDirection(true);
		}

		m_pFormationHero.DoFormationMoveTarget(pReaderEnemy);
	}

	public void ExtractFormationUnit(bool bHero, List<DKFormationBase.SFomationMember> pListOutFormation)
	{
		if (bHero)
		{
			m_pFormationHero.ExtractFormationMember(pListOutFormation);
		}
		else
		{
			m_pFormationEnemy.ExtractFormationMember(pListOutFormation);
		}
	}

	public void DoMgrStageSpawnerDecreaseAggro(DKUnitBase pUnit)
	{
		m_pFormationHero.DoFormationDecreaseAggro(pUnit);
		m_pFormationEnemy.DoFormationDecreaseAggro(pUnit);
	}

	//----------------------------------------------------------------
	private void UpdateMgrStageSpawnerEncounter()
	{
		DKUnitBase pUnitHero = m_pFormationHero.GetFormationReader();
		DKUnitBase pUnitEnemy = m_pFormationEnemy.GetFormationReader();
		if (pUnitHero == null || pUnitEnemy == null)
		{
			m_bFormationMove = false;
			return;
		}

		Vector3 vecLength = pUnitEnemy.GetUnitPosition() - pUnitHero.GetUnitPosition();
		float fLength = vecLength.magnitude;

		if (fLength <= m_fEncountLength)
		{
			m_bFormationMove = false;
			PrivMgrStageSpawnerEncountEnemy();
		}
	}

	private void UpdateMgrStageSpawnerFormation()
	{
		m_pFormationHero.UpdateFormation();
		m_pFormationEnemy.UpdateFormation();
	}

	private void PrivMgrStageSpawnerEncountEnemy()
	{
		m_pFormationHero.DoFormationCombatStart();
		m_pFormationEnemy.DoFormationCombatStart();
	}
}
