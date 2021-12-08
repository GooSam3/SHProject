using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// [주의] 모든 객체의 행동은 아래 인터페이스를 통해야 하며 이는 
// 네트워크 게임으로 전환시 패킷 전송단위가 된다.
// 
public interface ISkillProcessor
{
	//---------------------------------------------------------
	public float IGetSkillCoolTime(string strCooltimeName);
	public float IGetSkillCoolTimeGlobal(string strGlobalName);
	public void	ISetSkillCoolTime(string strCoolTimeName, float fDuration);
	public void	ISetSkillCoolTimeReduce(string strCoolTimeName, float fReduceValue);
	public void	ISetSkillCoolTimeGlobal(string strCoolTimeName, float fDuration);
	
	public int	ISkillPlay(CSkillUsage pSkillUsage);
	public int	ISkillPlayInterrupt(CSkillDataActive pSkillData, CSkillUsage pSkillUsage);
	public int   ISkillCondition(uint hSkillID);
	public void	ISkillCancle();

	public void  ISkillBuffTo(CUnitBase pTarget, uint hBuffID, float fDuration, float fPower);
	public void  ISkillBuffFrom(uint hBuffID, CUnitBase pBuffOrigin, float fDuration, float fPower);
	public void  ISkillBuffEnd(uint hBuffID);

	public void  ISkillAnimation(ref CAnimationBase.SAnimationUsage rAnimUsage, UnityAction<string, bool> delAnimationEnd, UnityAction<string, int, float> delAnimationEvent);
	public void	ISkillAnimationReset();
	public void  ISkillMoveToTarget(CUnitBase pTarget, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish);
	public void	ISkillMoveToPosition(Vector3 vecDest, float fStopRange, UnityAction<CNavAgentBase.ENavAgentEvent, Vector3> delFinish);

	//---------------------------------------------------------
	public CUnitBase.EUnitRelationType	IGetUnitRelationType();
	public CUnitBase.EUnitControlType	IGetUnitControlType();
	public CUnitBase IGetUnit();
}

abstract public class CFiniteStateMachineSkillBase : CFiniteStateMachineBase
{
	private Dictionary<uint, CSkillDataBase> m_mapSkillData = new Dictionary<uint, CSkillDataBase>();
	private ISkillProcessor m_pSkillOwner = null;	
	private CSkillDataActive mSkillCurrent = null;
	private bool m_bSkillPlay = false;   public bool IsSkillPlay { get { return m_bSkillPlay; } }
	//------------------------------------------------------------------
	protected sealed override void OnFSMStateEmpty()
	{
		m_bSkillPlay = false;
		OnFSMSkillEmpty();
	}

	internal void ImportFSMInitialize(ISkillProcessor pSkillOnwer)
	{
		m_pSkillOwner = pSkillOnwer;
		OnFSMSkillInitialize(pSkillOnwer);
	}

	internal int ImportFSMSkillCondition(uint hSkillID)
	{
		int Result = 0;
		CSkillDataActive pSkillActive = FindSkillData(hSkillID) as CSkillDataActive;
		if (pSkillActive != null)
		{
			Result = PrivSkillConditionCheck(pSkillActive); // 스킬 사용조건을 체크
		}
		return Result;
	}

	//-------------------------------------------------------------------
	// 자신의 스킬을 사용할 때
	protected int ProtFSMSkillUseTry(CSkillUsage pSkillUsage) 
	{
		int Result = 0;
		CSkillDataBase pSkillData = FindSkillData(pSkillUsage.UsageSkillID);
		Result = PrivSkillUseInternal(pSkillData, pSkillUsage, EStateEnterType.Enter);
		return Result;
	}

	protected int ProtFSMSkillTry(uint hSkillID)
	{
		int Result = 0;
		CSkillDataBase pSkillData = FindSkillData(hSkillID);
		Result = PrivSkillTryInternal(pSkillData);
		return Result;
	}

	// 다른 사용자에 의해 각종 군중제어 스킬이 나에게 사용되었을때 / 추가타 등이 삽입 되었을때
	protected int ProtFSMSkillUseInterrupt(CSkillUsage pSkillUsage, CSkillDataActive pSkillData)
	{		
		return PrivSkillUseInternal(pSkillData, pSkillUsage, EStateEnterType.Interrupt);  // 면역이나 기타 결과가 통지
	}

	protected CSkillDataBase FindSkillData(uint hSkillID)
	{
		CSkillDataBase pFindSkillData = null;
		if (m_mapSkillData.ContainsKey(hSkillID))
		{
			pFindSkillData = m_mapSkillData[hSkillID];
		}
		return pFindSkillData;
	}

	protected void ProtFSMSkillDataLoad(CSkillDataBase pSkillData)
	{
		if (pSkillData == null) return;

		if (m_mapSkillData.ContainsKey(pSkillData.hSkillID))
		{
			Debug.LogError($"[SkillSystem] Duplication Skill ID {pSkillData.hSkillID}");
		}
		else
		{
			m_mapSkillData[pSkillData.hSkillID] = pSkillData;
		}
	}

	protected void ProtFSMSkillTaskEvent(int iTaskEvent, params object [] aParam)
	{
		CStateSkillBase pStateSkill = m_pStateCurrent as CStateSkillBase;
		if (pStateSkill == null) return;

		pStateSkill.DoStateTaskEvent(iTaskEvent, aParam);
	}
	//--------------------------------------------------------------------
	private int PrivSkillConditionCheck(CSkillDataActive pSkillData)
	{
		int Result = 0;
		if (pSkillData != null)
		{
			for (int i = 0; i < pSkillData.listCondition.Count; i++)
			{
				Result = pSkillData.listCondition[i].DoCheckCondition(m_pSkillOwner);
				if (Result != 0) break;
			}
		}
		else
		{
			//Error
		}
		return Result;
	}

	private int PrivSkillUseInternal(CSkillDataBase pSkillData, CSkillUsage pSkillUsage, EStateEnterType eStateEnterType)
	{
		int Result = 0;
		CSkillDataActive pSkillActive = pSkillData as CSkillDataActive;
		if (pSkillActive != null)
		{
			Result = PrivSkillConditionCheck(pSkillActive); // 스킬 사용조건을 체크

			if (Result == 0)
			{
				PrivSkillExecute(pSkillActive, pSkillUsage, eStateEnterType);
			}
		}
		else
		{
			//Error!
		}

		return Result;
	}

	private int PrivSkillTryInternal(CSkillDataBase pSkillData)
	{
		int Result = 0;
		CSkillDataActive pSkillActive = pSkillData as CSkillDataActive;
		if (pSkillActive != null)
		{
			Result = PrivSkillConditionCheck(pSkillActive);
		}

		return Result;
	}

	private void PrivSkillExecute(CSkillDataActive pSkillData, CSkillUsage pSkillUsage, EStateEnterType eStateEnterType)
	{
		for (int i = 0; i < pSkillData.listState.Count; i++)
		{
			pSkillData.listState[i].ImportStateInitialize(pSkillData.PropertyList, pSkillUsage, m_pSkillOwner);
			ProtStateAction(pSkillData.listState[i], eStateEnterType);
		}
		m_bSkillPlay = true;
		mSkillCurrent = pSkillData;
		OnFSMSkillExecute(pSkillData);
	}

	//-----------------------------------------------------------------------------
	protected virtual void OnFSMSkillExecute(CSkillDataActive pSkillData) { }
	protected virtual void OnFSMSkillEmpty() { }
	protected virtual void OnFSMSkillInitialize(ISkillProcessor pSkillOnwer) { }
	
}
