using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class CCamera2DTopDownBase : CCameraControllerBase
{
	[SerializeField]
	private Vector3 Offset = Vector3.zero;

	private float mHeightOrigin = 0;
	//----------------------------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		mHeightOrigin = transform.position.y;
	}

	//-----------------------------------------------------------------------
	protected void ProtUpdateFollow(Transform pFollowUnit)
	{
		Vector3 vecCamera = transform.position;
		Vector3 vecUnit = pFollowUnit.position;
		vecCamera.x = vecUnit.x + Offset.x;
		vecCamera.z = vecUnit.z + Offset.z;

		transform.position = vecCamera;
	}
}
