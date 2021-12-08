using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHEffectCharacterMove : CEffectTransformBase
{
	
	//---------------------------------------------------------------
	protected override void OnEffectStartTransform(Transform pFollow, Vector3 vecDest, float fDuration, params object[] aParams) 
	{
		ProtEffectTransformRefreshInstance(0, pFollow, vecDest);
		base.OnEffectStartTransform(pFollow, vecDest, fDuration, aParams);
	}

	//----------------------------------------------------------------
	public void HandleEffectMoveTweenEnd()
	{
		DoEffectEnd();
	}


}
