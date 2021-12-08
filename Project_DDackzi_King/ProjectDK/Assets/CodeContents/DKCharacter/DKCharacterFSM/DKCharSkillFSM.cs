using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
public class DKCharSkillFSM : CFiniteStateMachineSkillBase
{
	[System.Serializable]
    public class SSkillTable
	{
		public EActiveSkillType SkillType = EActiveSkillType.None; 
		public uint TableID = 0;
		public int Level = 0;
		public List<uint> SkillID = new List<uint>(); 
	}

	[SerializeField]
	private List<SSkillTable> SkillTableActive = new List<SSkillTable>();
	[SerializeField]
	private List<SSkillTable> SkillTablePassive = new List<SSkillTable>();

	private IDKSkillProcessor	    m_pSkillOnwer = null;
	private Dictionary<uint, uint> m_mapSkillTable = new Dictionary<uint, uint>(); // 테이블 스킬 (외부노출)의 실제 작동 스킬 ID 맵핑
	//--------------------------------------------------------------------------
	protected override void OnFSMSkillInitialize(ISkillProcessor pSkillOnwer)
	{
		base.OnFSMSkillInitialize(pSkillOnwer);
		m_pSkillOnwer = pSkillOnwer as IDKSkillProcessor;		
		PrivSkillLoadActive();
	}

	//----------------------------------------------------------------------------
	public void EventAnimationTaskEvent(EAnimEventType eAmimEventType, int iIndex)
	{
		ProtFSMSkillTaskEvent((int)ETaskEventType.TaskEvent_Animation, eAmimEventType, iIndex);
	}

	public void EventAnimationEnd()
	{
		ProtFSMSkillTaskEvent((int)ETaskEventType.TaskEvent_AnimationEnd);
	}

	public uint ExtractReadySkillTableID(EActiveSkillType eSkillType)
	{
		uint iReadySkill = 0;
		List<uint> pListSkillData = FindSkillDataFromActiveType(eSkillType);
		for (int i = 0; i < pListSkillData.Count; i++)
		{
			int Result = DoCharSkillTry(pListSkillData[i]);
			if (Result == 0)
			{
				iReadySkill = pListSkillData[i];
				break;
			}
		}
		return iReadySkill;
	}
	
	public int DoCharSkillPlay(uint hSkillTableID, CSkillUsage pUsage)
	{
		int Result = 0;
		if (m_mapSkillTable.ContainsKey(hSkillTableID))
		{
			Result = ProtFSMSkillUseTry(m_mapSkillTable[hSkillTableID], pUsage);
		}
		else
		{
			Result = (int)ESkillConditionResult.Invalid;
		}
		return Result;
	}

	public int DoCharSkillTry(uint hSkillTableID)
	{
		int Result = 0;
		if (m_mapSkillTable.ContainsKey(hSkillTableID))
		{
			Result = ProtFSMSkillTry(m_mapSkillTable[hSkillTableID]);
		}
		else
		{
			Result = (int)ESkillConditionResult.Invalid;
		}
		return Result;
	}

	//-----------------------------------------------------------------------------
	private void PrivSkillLoadActive()
	{
		for (int i = 0; i < SkillTableActive.Count; i++)
		{
			PrivSkillLoadTableActive(SkillTableActive[i]);
		}
	}

	private void PrivSkillLoadTableActive(SSkillTable pSkillTable)
	{
		if (pSkillTable.Level >= pSkillTable.SkillID.Count) return;
		uint hSkillID = pSkillTable.SkillID[pSkillTable.Level];
		m_mapSkillTable[pSkillTable.TableID] = hSkillID;
		DKSkillDataActive pActive = DKManagerScriptData.Instance.DoLoadSkillActive(hSkillID);
		if (pActive != null)
		{
			SetFSMSkillData(pActive);
		}
	}

	private List<uint> FindSkillDataFromActiveType(EActiveSkillType eSkillType)
	{
		List<uint> pListDataActive = new List<uint>();

		for (int i = 0; i < SkillTableActive.Count; i++)
		{
			if (SkillTableActive[i].SkillType == eSkillType)
			{
				pListDataActive.Add(SkillTableActive[i].TableID);
			}
		}

		return pListDataActive;
	}
}
