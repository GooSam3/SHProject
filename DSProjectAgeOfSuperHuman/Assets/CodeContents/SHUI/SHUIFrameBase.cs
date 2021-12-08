using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHUIFrameBase : CUIFrameMovableBase
{
	//------------------------------------------------------------------
	public void CloseSelf()
	{
		UIManager.Instance.DoUIMgrHide(this);
		OnSHUIFrameCloseSelf();
	}
	//-------------------------------------------------------------------
	protected virtual void OnSHUIFrameCloseSelf() { }
}
