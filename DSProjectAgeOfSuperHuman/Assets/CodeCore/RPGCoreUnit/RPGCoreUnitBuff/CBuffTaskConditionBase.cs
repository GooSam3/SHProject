using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffTaskConditionBase 
{
    private List<CBuffTaskBase> m_listBuffTaskInstance = new List<CBuffTaskBase>();
    //-------------------------------------------------------
    public void DoBuffTaskCondition(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, params object[] aParams)
	{
        if (OnBuffTaskCondition(pBuff, pBuffOwner, pBuffOrigin, aParams))
		{
            for (int i = 0; i < m_listBuffTaskInstance.Count; i++)
			{
                CBuffTaskBase pBuffTask = m_listBuffTaskInstance[i];
                pBuffTask.DoBuffTask(pBuff, pBuffOwner, pBuffOrigin);
                OnBuffTaskExcute(pBuffTask);
			}
		}
	}

    public void DoBuffTaskEnd(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
		for (int i = 0; i < m_listBuffTaskInstance.Count; i++)
		{
			CBuffTaskBase pBuffTask = m_listBuffTaskInstance[i];
			pBuffTask.DoBuffTaskEnd(pBuff, pBuffOwner, pBuffOrigin);
			OnBuffTaskEnd(pBuffTask);
		}
	}

    public void ImportBuffTaskAdd(CBuffTaskBase pTask)
	{
        m_listBuffTaskInstance.Add(pTask);
	}

    //--------------------------------------------------------
    protected virtual bool OnBuffTaskCondition(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin, params object[] aParams) { return true; }
    protected virtual void OnBuffTaskExcute(CBuffTaskBase pBuffTask) { }
	protected virtual void OnBuffTaskEnd(CBuffTaskBase pBuffTask) { }
}
