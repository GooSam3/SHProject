using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

interface IVirtualDBServer
{
	void DoLoginDBServer(string ClientID, UnityAction<int> FinishWork);
}

abstract public class CVirtualDBServerBase : CManagerTemplateBase<CVirtualDBServerBase>, IVirtualDBServer
{
	//--------------------------------------------------------------
	public void DoLoginDBServer(string ClientID, UnityAction<int> FinishWork)
	{
		OnLoginDBServer(ClientID, FinishWork);	
	}
	//------------------------------------------------------------------
	protected virtual void OnLoginDBServer(string ClientID, UnityAction<int> FinishWork) { }

}
