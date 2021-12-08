using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStateSkillBase : CStateBase
{
    private List<CSkillResourceBase>    m_listSkillResource = new List<CSkillResourceBase>();
    private CSkillUsage               mSkillUsage = null;
    private ISkillProcessor            mSkillProcessor = null;
    private CTaskEventReceiver             m_pTaskEvent = null;
	//---------------------------------------------------------
	protected override int OnStateCanEnter(CStateBase pStatePrev)
	{
		int Result = 0;
		for (int i = 0; i < m_listSkillResource.Count; i++)
		{
			Result = m_listSkillResource[i].DoCheckSkillResource(mSkillUsage, mSkillProcessor);
			if (Result != 0) break;
		}
		return Result;
	}

    public void DoStateInitialize(CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor)
	{
        mSkillUsage = pSkillUsage;
        mSkillProcessor = pSkillProcessor;
        OnStateInitialize(pSkillUsage, pSkillProcessor);
    }
    
    public void DoStateTaskEvent(int iEventType, params object [] aArg)
	{
        if (m_pTaskEvent != null)
		{
			m_pTaskEvent.DoTaskEvent(iEventType, this, mSkillUsage, mSkillProcessor, aArg);
		}

		OnStateTaskEvent(iEventType, aArg);
    }

    public void ImportTaskEvent(CTaskEventReceiver pTaskEvent) { m_pTaskEvent = pTaskEvent;}
    public List<CSkillResourceBase> ExportSkilResource() { return m_listSkillResource; }
    //----------------------------------------------------------
    protected virtual void OnStateInitialize(CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor) { }
    protected virtual void OnStateTaskEvent(int iEventType, params object[] aArg) { }
   
}
