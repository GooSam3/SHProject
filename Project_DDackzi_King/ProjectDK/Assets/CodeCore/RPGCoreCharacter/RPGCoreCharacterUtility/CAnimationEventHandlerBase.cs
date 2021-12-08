using System.Collections;
using System.Collections.Generic;
using UnityEngine;


abstract public class CAnimationEventHandlerBase : CMonoBase
{
	private CUnitBase mAnimationOwner = null;
	//-----------------------------------------------------
	internal void ImportEventHandlerOwner(CUnitBase pOwner)
	{
		mAnimationOwner = pOwner;
		OnEventHandlerOwner(pOwner);
	}
	//-------------------------------------------------------
	protected virtual void OnEventHandlerOwner(CUnitBase pOwner) { }
}
