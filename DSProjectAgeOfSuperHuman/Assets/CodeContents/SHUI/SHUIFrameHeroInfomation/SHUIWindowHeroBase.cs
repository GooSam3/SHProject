using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SHUIWindowHeroBase : CUIWidgetDialogBase
{
	private uint m_hHeroID = 0; public uint pHeroID { get { return m_hHeroID; } }
	//---------------------------------------------------
	public void DoWindowHeroRefresh(uint hHeroID)
	{
		m_hHeroID = hHeroID;
		DoUIWidgetShowHide(true);
		OnUIWindowHeroRefresh(hHeroID);
	}

	public bool DoHeroUpgradePlus(long iPlusPoint)
	{
		return OnHeroUpgradePlus(iPlusPoint);
	}

	//------------------------------------------------------------------------------
	public void HandleHeroUpgradeConfirm()
	{
		OnHeroUpgradeConfirm();
	}

	public void HandleHeroUpgradeReset()
	{
		OnHeroUpgradeReset();
	}

	//-------------------------------------------------
	protected virtual void OnUIWindowHeroRefresh(uint hHeroID) { }
	protected virtual bool OnHeroUpgradePlus(long iPlusPoint) { return false; }
	protected virtual void OnHeroUpgradeConfirm() { }
	protected virtual void OnHeroUpgradeReset() { }
}
