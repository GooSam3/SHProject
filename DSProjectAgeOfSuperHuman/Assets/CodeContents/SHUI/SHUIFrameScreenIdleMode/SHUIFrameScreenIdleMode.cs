using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameScreenIdleMode : SHUIFrameBase
{
	
	//----------------------------------------------------
	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		PrivScreenIdleStart();
	}

	protected override void OnUIFrameHide()
	{
		base.OnUIFrameHide();
		PrivScreenIdleEnd();
	}

	protected override void OnUIFrameUpdate()
	{
		base.OnUIFrameUpdate();

		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		if (pHero.GetUnitAIEnable() == false)
		{
			pHero.DoUnitAIEnable(true);
		}
	}

	//-----------------------------------------------------
	private void PrivScreenIdleStart()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		if (pHero == null)
		{
			UIManager.Instance.DoUIMgrHide(this);
			return;
		}
		pHero.DoUnitAIEnable(true);
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatScreenIdle(this, true);
		UIManager.Instance.DoUIMgrInputRefresh(true);
	}

	private void PrivScreenIdleEnd()
	{
		SHUnitHero pHero = SHManagerUnit.Instance.GetUnitHero();
		pHero.DoUnitAIEnable(false);
		UIManager.Instance.DoUIMgrFind<SHUIFrameCombatHero>().DoUIFrameCombatScreenIdle(this, false);
		UIManager.Instance.DoUIMgrInputRefresh(false);
	}

	//-----------------------------------------------------
	public void HandleScreenIdleOff()
	{
		UIManager.Instance.DoUIMgrScreenIdle(false);
	}
}
