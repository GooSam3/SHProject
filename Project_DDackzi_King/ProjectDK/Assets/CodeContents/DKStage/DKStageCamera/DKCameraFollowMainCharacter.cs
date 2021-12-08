using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKCameraFollowMainCharacter : CCamera2DTopDownBase
{
	private DKUnitBase mFollowUnit = null;
	//--------------------------------------------------------
	protected override void OnUnityUpdate()
	{
		base.OnUnityUpdate();

		if (mFollowUnit)
		{
			ProtUpdateFollow(mFollowUnit.transform);
		}
	}

	//--------------------------------------------------------
	public void DoCameraFollowTarget(DKUnitBase pFollowUnit)
	{
		mFollowUnit = pFollowUnit;
	}
	

}
