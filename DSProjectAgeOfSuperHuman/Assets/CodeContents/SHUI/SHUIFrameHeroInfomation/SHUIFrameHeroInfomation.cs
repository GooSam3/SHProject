using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameHeroInfomation : SHUIFrameBase
{
	[SerializeField]
	private SHUIWidgetHeroInfomationView	HeroView = null;
	[SerializeField]
	private SHUIWindowHeroInfomation		HeroInfo = null;
	[SerializeField]
	private SHUIWindowHeroUpgradeTraining	HeroTraining = null;
	[SerializeField]
	private SHUIWindowHeroUpgradeEnhance	HeroEnhance = null;
	[SerializeField]
	private SHUIWindowHeroUpgradeSkill		HeroSkill = null;
	[SerializeField]
	private SHUIWindowHeroEquipScroll		HeroEquipScroll = null;
	[SerializeField]
	private SHUIWidgetHeroEquipInfo			HeroEquipInfo = null;

	private uint m_hHeroID = 0;
	private SHUIWindowHeroBase m_pSelectedWindow = null;
	
	//-----------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
		m_pSelectedWindow = HeroInfo;
		HeroView.DoUIWidgetShowHide(true);
		HeroEquipInfo.DoUIWidgetShowHide(false);
	}

	protected override void OnSHUIFrameCloseSelf()
	{
		UIManager.Instance.DoUIMgrHide<SHUIFrameNavigationBar>();
	}
	//------------------------------------------------------------------------------
	public void DoHeroInfomationStat(uint hHeroID)
	{
		m_pSelectedWindow = HeroInfo;
		PrivHeroInfoRefresh(hHeroID);
	}

	public void DoHeroInfomationTraining(uint hHeroID)
	{
		m_pSelectedWindow = HeroTraining;
		PrivHeroInfoRefresh(hHeroID);
	}

	public void DoHeroInfomationEnhance(uint hHeroID)
	{
		m_pSelectedWindow = HeroEnhance;
		PrivHeroInfoRefresh(hHeroID);
	}

	public void DoHeroInfomationSkill(uint hHeroID)
	{
		m_pSelectedWindow = HeroSkill;
		PrivHeroInfoRefresh(hHeroID);
	}

	public void DoHeroInfomationEquipment(uint hHeroID)
	{
		m_pSelectedWindow = HeroEquipScroll;
		PrivHeroInfoRefresh(hHeroID);
	}

	public void DoHeroInfomationRefresh()
	{
		PrivHeroInfoRefresh(m_hHeroID);
	}

	public void DoHeroEquipView(SItemData pItemData)
	{
		HeroEquipInfo.DoUIWidgetShowHide(true);
		HeroEquipInfo.DoHeroEquipViewItem(pItemData, m_hHeroID);
	}

	//-------------------------------------------------------------------------------
	//public void IHeroListSlotSelect(uint hHeroID)
	//{
	//	PrivHeroInfoRefresh(hHeroID);
	//	HeroEquipInfo.DoUIWidgetShowHide(false);
	//}

	//-------------------------------------------------------------------------------
	private void PrivHeroInfoRefresh(uint hHeroID)
	{
		m_hHeroID = hHeroID;
		m_pSelectedWindow.DoWindowHeroRefresh(hHeroID);
		HeroView.DoHeroInfoView(hHeroID);
		if (m_pSelectedWindow == HeroEquipScroll)
		{
			HeroView.DoHeroInfoEquipment(hHeroID);
		}
		HeroEquipInfo.DoHeroEquipViewItemRefresh();
	}

	//-------------------------------------------------------------------------------
	public void HandleHeroInfoPurchase()
	{

	}
}
