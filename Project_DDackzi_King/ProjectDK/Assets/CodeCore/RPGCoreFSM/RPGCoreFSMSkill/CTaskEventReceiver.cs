using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTaskEventReceiver
{
	protected CMultiSortedDictionary<int, CTaskEventConditionBase> m_mapTaskInstance = new CMultiSortedDictionary<int, CTaskEventConditionBase>();
	public void DoTaskEvent(int hTaskType, CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg)
	{
		if (m_mapTaskInstance.ContainsKey(hTaskType) == false) return;

		List<CTaskEventConditionBase> pListSkillTask = m_mapTaskInstance[hTaskType];
		for (int i = 0; i < pListSkillTask.Count; i++)
		{
			pListSkillTask[i].DoSkillEventCondition(pOwnerState, pUsage, pSkillProcessor, aArg);
		}
	}

	public CMultiSortedDictionary<int, CTaskEventConditionBase> ExportTaskMap()
	{
		return m_mapTaskInstance;
	}
	//---------------------------------------------------------------------------
}

public abstract class CTaskEventConditionBase
{
	protected List<CSkillTaskBase> m_listSkillTask = new List<CSkillTaskBase>();

	public void DoSkillEventCondition(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg)
	{
		if (OnTaskEventCondition(pOwnerState, pUsage, pSkillProcessor, aArg))
		{
			for (int i = 0; i < m_listSkillTask.Count; i++)
			{
				m_listSkillTask[i].DoSkillTaskUse(pOwnerState, pUsage, pSkillProcessor);
			}
		}
	}

	public void SetSkillTask(CSkillTaskBase pTaskBase)
	{
		m_listSkillTask.Add(pTaskBase);
	}

	//------------------------------------------------------
	protected void ProtEventTaskExcute(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor)
	{
		for (int i = 0; i < m_listSkillTask.Count; i++)
		{
			m_listSkillTask[i].DoSkillTaskUse(pOwnerState, pUsage, pSkillProcessor);
		}
	}

	//------------------------------------------------------
	protected virtual bool OnTaskEventCondition(CStateSkillBase pOwnerState, CSkillUsage pUsage, ISkillProcessor pSkillProcessor, params object[] aArg) { return true; }
}

