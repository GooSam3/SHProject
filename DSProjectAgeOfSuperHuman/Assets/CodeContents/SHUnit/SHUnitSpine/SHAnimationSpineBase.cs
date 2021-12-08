using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NSkill;
public abstract class SHAnimationSpineBase : CAnimationBase
{
	public enum ESpineMaterialType
	{
		None,
		GayScale,
		FillColor,
	}

	//------------------------------------------------------------
	public void DoAnimationSpineChangeMaterial(ESpineMaterialType eSpineMatType)
	{
		ProtAnimationMaterialChangeByPreSet((int)eSpineMatType);
	}

	public void DoAnimationSpineOrderInLayer(int iLayer)
	{
		IEnumerator<List<CAniControllerBase>> it = m_mapAniController.value.GetEnumerator();
		while (it.MoveNext())
		{
			List<CAniControllerBase> pList = it.Current;
			for (int i = 0; i < pList.Count; i++)
			{
				SHAniControllerSpine pSpineController = pList[i] as SHAniControllerSpine;
				pSpineController.SetAnimationSpineLayerOrder(iLayer);
			}
		}
	}
}
