using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DKUnitEnemyBase : DKUnitBase
{
	//-------------------------------------------------
	protected override void OnUnityAwake()
	{
		base.OnUnityAwake();
		m_eUnitControlType = EUnitControlType.EnemyAI;
	}

	protected override void OnUnitInitialize()
	{
		base.OnUnitInitialize();
		DKUIManager.Instance.DoUIFrameNameTagRegist(this, DKUIFrameNameTag.ENameTagType.HPBarEnemy);
	}

	protected override void OnUnitDeathEnd()
	{
		base.OnUnitDeathEnd();
		DKUIManager.Instance.DoUIFrameNameTagUnRegist(this);
	}
}
