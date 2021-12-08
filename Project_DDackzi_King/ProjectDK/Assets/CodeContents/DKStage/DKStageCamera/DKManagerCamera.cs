using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKManagerCamera : CManagerCameraBase
{	public static new DKManagerCamera Instance { get { return CManagerCameraBase.Instance as DKManagerCamera; } }

    //---------------------------------------------------------
    public void DoCameraAttachUnit(DKUnitBase pAttachUnit)
	{
		if (pAttachUnit == null) return;

		if (mActiveController)
		{
			DKCameraFollowMainCharacter pCameraFollow = mActiveController as DKCameraFollowMainCharacter;
			if (pCameraFollow)
			{
				pCameraFollow.DoCameraFollowTarget(pAttachUnit);
			}
		}
	}
}
