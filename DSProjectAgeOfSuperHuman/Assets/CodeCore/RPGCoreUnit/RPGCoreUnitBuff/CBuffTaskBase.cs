using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffTaskBase 
{
    private bool m_bBuffTaskActive = false;
    private CBuffComponentBase m_pBuffOwner = null;
    private CBuffComponentBase m_pBuffOrigin = null;
    //----------------------------------------------------------------------
    public void DoBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
        m_bBuffTaskActive = true;
        m_pBuffOwner = pBuffOwner;
        m_pBuffOrigin = pBuffOrigin;
        OnBuffTask(pBuff, pBuffOwner, pBuffOrigin);
	}

    public void DoBuffTaskEnd(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin)
	{
        if (m_bBuffTaskActive)
		{
            OnBuffTaskEnd(pBuff, pBuffOwner, pBuffOrigin);
		}
	}
    //-----------------------------------------------------------------------
    protected virtual void OnBuffTask(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin) { }
    protected virtual void OnBuffTaskEnd(CBuffBase pBuff, CBuffComponentBase pBuffOwner, CBuffComponentBase pBuffOrigin) { }
}
