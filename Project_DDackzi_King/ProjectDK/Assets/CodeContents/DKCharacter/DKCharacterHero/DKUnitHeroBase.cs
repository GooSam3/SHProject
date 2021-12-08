using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DKUnitHeroBase : DKUnitBase
{

	//-------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_eUnitControlType = EUnitControlType.PlayerAI;
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		DKUIManager.Instance.DoUIFrameNameTagRegist(this, DKUIFrameNameTag.ENameTagType.HPBarHero);
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
		DKUIManager.Instance.DoUIFrameNameTagUnRegist(this);
	}

}
