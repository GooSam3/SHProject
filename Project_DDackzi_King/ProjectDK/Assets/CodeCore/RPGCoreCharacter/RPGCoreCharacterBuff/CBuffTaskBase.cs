using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CBuffTaskBase 
{

    //----------------------------------------------------------------------
    public void DoBuffTask(CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin,  float fBuffPower, int iTickCount)
	{
        OnBuffTask(pBuffOwner, pBuffOrigin, fBuffPower, iTickCount);
	}

    //-----------------------------------------------------------------------
    protected virtual void OnBuffTask(CBuffProcessorBase pBuffOwner, CBuffProcessorBase pBuffOrigin, float fBuffPower, int iTickCount) { }

}
