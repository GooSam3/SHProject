using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CStateSkillBase : CStateBase
{
	private List<CSkillResourceBase>		m_listSkillResource = new List<CSkillResourceBase>();  // ������Ʈ ���Խ� �ʿ��� �ڿ�
    private CSkillUsage					m_pSkillUsage = null;								  // ��ų ��ü�� ��� ����
	private CSkillPropertyListBase				m_pSkillPropertList = null;
    private ISkillProcessor				m_pSkillProcessor = null;							  // ��ų ó����
    private CSkillTaskEventList					m_pTaskEventReceiver = null;								  // �׽�ũ �ڵ鷯
	//---------------------------------------------------------
	protected override int OnStateCanEnter(CStateBase pStatePrev)
	{
		int Result = 0;
		for (int i = 0; i < m_listSkillResource.Count; i++)
		{
			Result = m_listSkillResource[i].DoCheckSkillResource(m_pSkillUsage, m_pSkillProcessor);
			if (Result != 0) break;
		}
		return Result;
	}

    internal void ImportStateInitialize(CSkillPropertyListBase pSkillPropertyList, CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor)
	{
		m_pSkillPropertList = pSkillPropertyList;
        m_pSkillUsage = pSkillUsage;
        m_pSkillProcessor = pSkillProcessor;
        OnStateInitialize(pSkillUsage, pSkillProcessor);
    }
    
	//-----------------------------------------------------------
    public void DoStateTaskEvent(int iEventType, params object [] aArg)
	{
        if (m_pTaskEventReceiver != null)
		{
			m_pTaskEventReceiver.DoTaskEvent(iEventType, this, m_pSkillUsage, m_pSkillProcessor, aArg);
		}

		OnStateTaskEvent(iEventType, aArg);
    }

	public void DoStateSelfEnd()
	{
		ProtStateSelfEnd();
	}

	//------------------------------------------------------------
	public float GetStatePropertyValue(string strPropertyName)
	{
		return m_pSkillPropertList.GetSkillPropertyValue(strPropertyName);
	}

	//------------------------------------------------------------
    public void ImportTaskEvent(CSkillTaskEventList pTaskEvent) { m_pTaskEventReceiver = pTaskEvent;}
    public List<CSkillResourceBase> ExportSkilResource() { return m_listSkillResource; }
    //----------------------------------------------------------
    protected virtual void OnStateInitialize(CSkillUsage pSkillUsage, ISkillProcessor pSkillProcessor) { }
    protected virtual void OnStateTaskEvent(int iEventType, params object[] aArg) { }
   
}
