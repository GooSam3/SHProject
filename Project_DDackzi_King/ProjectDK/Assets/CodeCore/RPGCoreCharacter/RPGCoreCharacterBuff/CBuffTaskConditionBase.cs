using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffTaskConditionBase 
{
    private List<CBuffTaskBase> m_listBuffTaskInstance = new List<CBuffTaskBase>();
    //-------------------------------------------------------
    public void DoBuffTaskCondition(CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin, float fPower, int iTickCount, params object[] aParams)
	{
        if (OnBuffTaskCondition(pBuffOwner, pBuffOrigin, aParams))
		{
            for (int i = 0; i < m_listBuffTaskInstance.Count; i++)
			{
                CBuffTaskBase pBuffTask = m_listBuffTaskInstance[i];
                pBuffTask.DoBuffTask(pBuffOwner, pBuffOrigin, fPower, iTickCount);
                OnBuffTaskExcute(pBuffTask);
			}
		}
	}

    //--------------------------------------------------------
    protected virtual bool OnBuffTaskCondition(CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin, params object[] aParams) { return true; }
    protected virtual void OnBuffTaskExcute(CBuffTaskBase pBuffTask) { }
}
