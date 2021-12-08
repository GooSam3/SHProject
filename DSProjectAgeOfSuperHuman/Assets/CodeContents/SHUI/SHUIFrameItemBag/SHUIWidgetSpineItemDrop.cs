using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHUIWidgetSpineItemDrop : CUIWidgetSpineController
{
	[SerializeField]
	private string AnimationName = "drop1";
	[SerializeField]
	private bool Loop = false;

	private UnityAction m_delFinish = null;
	//------------------------------------------------------
	public void DoSpineItemDropStart(UnityAction delFinish)
	{
		m_delFinish = delFinish;
		ProtSpineControllerAnimation(AnimationName, Loop);
	}

	//---------------------------------------------------------
	protected override void OnSpineControllerAnimationEnd(string strAniName) 
	{
		m_delFinish?.Invoke();
	}
}
