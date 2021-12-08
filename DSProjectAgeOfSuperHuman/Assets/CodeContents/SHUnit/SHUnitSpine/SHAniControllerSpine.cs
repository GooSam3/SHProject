using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHAniControllerSpine : CAniControllerSpineBase
{
	public void SetAnimationSpineLayerOrder(int iLayerOrder)
	{
		ProtSpineAnimationOrderInLayer(iLayerOrder);
	}
}
