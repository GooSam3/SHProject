using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DKFormationBase
{
	public class SFomationMember
	{
		public int AggroCount = 0;
		public DKUnitBase pMember = null;
	}

	protected DKUnitBase m_pReaderUnit = null;
	protected List<SFomationMember> m_listMember = new List<SFomationMember>();
	//----------------------------------------------------------------------
	public void SetFormationMember(DKUnitBase pMember, bool bReader)
	{
		if (bReader)
		{
			m_pReaderUnit = pMember;
			OnFormationReaderUnit(pMember);
		}
		SFomationMember pFomationMember = new SFomationMember();
		pFomationMember.pMember = pMember;
		m_listMember.Add(pFomationMember);
	}

	public void ClearFormation()
	{
		m_pReaderUnit = null;
		m_listMember.Clear();
	}

	public DKUnitBase GetFormationReader()
	{
		DKUnitBase pReaderUnit = null;


		if (m_pReaderUnit != null && m_pReaderUnit.IsAlive)
		{
			pReaderUnit = m_pReaderUnit;
		}
		else
		{
			for (int i = 0; i < m_listMember.Count; i++)
			{
				if (m_listMember[i].pMember.IsAlive)
				{
					pReaderUnit = m_listMember[i].pMember;
					m_pReaderUnit = pReaderUnit;
					OnFormationReaderUnit(m_pReaderUnit);
					break;
				}
			}
		}
		return pReaderUnit;
	}

	public void ExtractFormationMember(List<SFomationMember> pOutListMember)
	{
		for (int i = 0; i < m_listMember.Count; i++)
		{
			if (m_listMember[i].pMember.IsAlive)
			{
				pOutListMember.Add(m_listMember[i]);
			}
		}
	}

	public void DoFormationDecreaseAggro(DKUnitBase pUnit)
	{
		for (int i = 0; i < m_listMember.Count; i++)
		{
			if (m_listMember[i].pMember == pUnit)
			{
				m_listMember[i].AggroCount--;

				if (m_listMember[i].AggroCount < 0)
				{
					m_listMember[i].AggroCount = 0;
				}

				break;
			}
		}
	}

	//----------------------------------------------------------------------
	public void DoFormationMoveTarget(DKUnitBase pTarget)
	{
		DKUnitBase pReaderUnit = GetFormationReader();
		if (pReaderUnit == null) return;
		PrivFormationEnableAI(false);
		PrivFormationMoveTo(pReaderUnit, pTarget);
	}

	public void DoFormationCombatStart()
	{
		PrivFormationAggroReset();
		PrivFormationEnableAI(true);
	}

	public void SetFormationDirection(bool bLeft)
	{
		for (int i = 0; i < m_listMember.Count; i++)
		{
			m_listMember[i].pMember.SetDKUnitDirection(bLeft);
		}
	}

	public void UpdateFormation()
	{
		bool bAllDeath = true;
		for (int i = 0; i < m_listMember.Count; i++)
		{
			if (m_listMember[i].pMember.IsAlive)
			{
				bAllDeath = false;
				break;
			}
		}

		if (bAllDeath)  
		{
			OnFormationAllDeath();
		}
	}

	//-------------------------------------------------------------------------
	private void PrivFormationEnableAI(bool bEnable)
	{
		for (int i = 0; i < m_listMember.Count; i++)
		{
			m_listMember[i].pMember.SetUnitAIEnable(bEnable);
		}
	}

	private void PrivFormationAggroReset()
	{
		for (int i = 0; i < m_listMember.Count; i++)
		{
			m_listMember[i].AggroCount = 0;
		}

	}

	private void PrivFormationMoveTo(DKUnitBase pReaderUnit, DKUnitBase pTargetUnit)
	{
		Vector3 vecDirection = pTargetUnit.GetUnitPosition() - pReaderUnit.GetUnitPosition();

		for (int i = 0; i < m_listMember.Count; i++)
		{
			DKUnitBase pMember = m_listMember[i].pMember;
			Vector3 vecDest = pMember.GetUnitPosition() + vecDirection;
			m_listMember[i].pMember.DoUnitMoveTo(vecDest);
		}
	}

	//-----------------------------------------------------------------------------
	protected virtual void OnFormationReaderUnit(DKUnitBase pUnit) { }	
	protected virtual void OnFormationAllDeath() { }
}
