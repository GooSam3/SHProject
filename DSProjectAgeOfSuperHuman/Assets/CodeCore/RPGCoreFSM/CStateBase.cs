using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public abstract class CStateBase
{
    protected CFiniteStateMachineBase mFSMOwnr = null;
	//------------------------------------------------------------------------
    internal void ImportStateInitialize(CFiniteStateMachineBase pFSMOwner)
	{
        mFSMOwnr = pFSMOwner;
        OnStateInitialize(pFSMOwner);
    }
    internal void ImportStateEnterAnother(CStateBase pStatePrev)
	{
        OnStateEnterAnother(pStatePrev);
    }
    internal void ImportStateEnter(CStateBase pStatePrev)
	{
        OnStateEnter(pStatePrev);
	}
    internal int ImportStateCanEnter(CStateBase pStatePrev)
	{
        return OnStateCanEnter(pStatePrev);
	}

    internal void ImportStateLeave()
	{
        OnStateLeave();
	}
    internal void ImportStateLeaveForce(CStateBase pStatePrev)
	{
        OnStateLeaveForce(pStatePrev);
	}
    internal void ImportStateInterrupted(CStateBase pStateInterrupt)
	{
        OnStateInterrupted(pStateInterrupt);
    }
    internal void ImportStateInterruptedResume(CStateBase pStateInterrupt)
	{
        OnStateInterruptResume(pStateInterrupt);
    }

    //--------------------------------------------------------------------------
    protected void ProtStateSelfEnd()
	{
        mFSMOwnr?.ImportStateLeave(this);
	}

	//--------------------------------------------------------------------------
	#region StateEventHandle
    protected virtual void OnStateInitialize(CFiniteStateMachineBase pFSMOwner) {}
    protected virtual void OnStateEnterAnother(CStateBase pStatePrev) {}
	protected virtual void OnStateEnter(CStateBase pStatePrev) {}
    protected virtual void OnStateLeaveForce(CStateBase pStatePrev) { }
    protected virtual void OnStateLeave() {}	
    protected virtual void OnStateInterrupted(CStateBase pStateInterrupt) {}
	protected virtual void OnStateInterruptResume(CStateBase pStateInterrupt) { }
    protected virtual int  OnStateCanEnter(CStateBase pStatePrev) { return 0; }
    internal  virtual IEnumerator OnStateUpdate() { yield break; }
	#endregion
	//---------------------------------------------------------------------------
}
