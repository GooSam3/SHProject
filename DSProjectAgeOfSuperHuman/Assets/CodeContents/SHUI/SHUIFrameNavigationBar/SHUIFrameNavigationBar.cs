using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameNavigationBar : SHUIFrameBase
{
	public enum ENavigationHeroType
	{
		None,
		Infomation,
		Train,
		Enhance,
		Skill,
		Equipment,
		Incense,
		Potential,
	}

	public enum ENavigationShopType
	{
		None,
		Diamond,
		PackageGoods,
		Goods,
		Gold,
	}

	public enum ENavigationSummonType
	{
		None,
		SummonHero,
		SummonEquipment,
	}

	public enum ENavigationTap
	{
		Hero,
		Shop,
		Summon,
		Party,
	}
	[System.Serializable]
	public class SNavitationTab
	{
		public ENavigationTap TapType;
		public GameObject TabPivot;
	}
	[SerializeField]
	private List<SNavitationTab> TabInfo = null;

	private ENavigationTap				m_eNavitationTab = ENavigationTap.Hero;
	private ENavigationHeroType		m_eNavigationHeroType = ENavigationHeroType.Infomation;
	private ENavigationShopType		m_eNavigationShopType = ENavigationShopType.Diamond;
	private ENavigationSummonType		m_eNavigationSummonType = ENavigationSummonType.SummonHero;
	private uint m_hSelectedHero = 0;   public uint GetNavigationBarSeletHero() { return m_hSelectedHero; }
	//-----------------------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
	}

	protected override void OnUIFrameShow(int iOrder)
	{
		base.OnUIFrameShow(iOrder);
		if (m_hSelectedHero == 0)
		{
			m_hSelectedHero = SHManagerGameDB.Instance.GetGameDBHeroReaderID();
		}

		UIManager.Instance.DoUIMgrHide<SHUIFrameResource>();
		PrivNavigationRefresh(m_eNavigationHeroType);
	}

	//---------------------------------------------------------------------------------------
	public void DoUINavigationTabHero()
	{		
		PrivNavigationRefreshTab(ENavigationTap.Hero);
	}

	public void DoUINavigationTabShop()
	{
		UIManager.Instance.DoUIMgrHide<SHUIFrameHeroInfomation>();
		PrivNavigationRefreshTab(ENavigationTap.Shop);
	}

	public void DoUINavigationTabSummon()
	{
		UIManager.Instance.DoUIMgrHide<SHUIFrameHeroInfomation>();
		PrivNavigationRefreshTab(ENavigationTap.Summon);
	}

	public void DoUINavigationTabRefresh(uint hHeroID)
	{
		m_hSelectedHero = hHeroID;
		PrivNavigationRefreshTab(m_eNavitationTab);
	}

	//----------------------------------------------------------------------------------------
	private void PrivNavigationRefresh(ENavigationHeroType eNavigationType)
	{		
		switch(m_eNavigationHeroType)
		{
			case ENavigationHeroType.Infomation:
				HandleNavigationInfomation();
				break;
			case ENavigationHeroType.Train:
				HandleNavigationTrain();
				break;
			case ENavigationHeroType.Enhance:
				HandleNavigationEnhance();
				break;
			case ENavigationHeroType.Skill:
				HandleNavigationSkill();
				break;
			case ENavigationHeroType.Equipment:
				HandleNavigationEquip();
				break;
			case ENavigationHeroType.Potential:
				HandleNavigationPotential();
				break;
		}
	}

	private void PrivNavigationRefresh(ENavigationShopType eNavigationType)
	{
		switch(eNavigationType)
		{
			case ENavigationShopType.Diamond:
				break;
		}
	}

	private void PrivNavigationRefresh(ENavigationSummonType eNavigationType)
	{
		switch (eNavigationType)
		{
			case ENavigationSummonType.SummonHero:
				break;
		}
	}

	private void PrivNavigationRefreshTab(ENavigationTap eNavigationTap)
	{
		for (int i = 0; i < TabInfo.Count; i++)
		{
			if (TabInfo[i].TapType == eNavigationTap)
			{
				TabInfo[i].TabPivot.SetActive(true);
			}
			else
			{
				TabInfo[i].TabPivot.SetActive(false);
			}
		}

		if (eNavigationTap == ENavigationTap.Hero)
		{
			PrivNavigationRefresh(m_eNavigationHeroType);
		}
		else if (eNavigationTap == ENavigationTap.Shop)
		{
			PrivNavigationRefresh(m_eNavigationShopType);
		}
		else if (eNavigationTap == ENavigationTap.Summon)
		{
			PrivNavigationRefresh(m_eNavigationSummonType);
		}
	}
	
	//---------------------------------------------------------------------------------------
	public void HandleNavigationInfomation()
	{
		m_eNavigationHeroType = ENavigationHeroType.Infomation;
		UIManager.Instance.DoUIMgrHide<SHUIFramePotentialBingo>();
		UIManager.Instance.DoUIMgrShow<SHUIFrameHeroInfomation>().DoHeroInfomationStat(m_hSelectedHero);
	}

	public void HandleNavigationTrain()
	{
		m_eNavigationHeroType = ENavigationHeroType.Train;
		UIManager.Instance.DoUIMgrHide<SHUIFramePotentialBingo>();
		UIManager.Instance.DoUIMgrShow<SHUIFrameHeroInfomation>().DoHeroInfomationTraining(m_hSelectedHero);
	}

	public void HandleNavigationEnhance()
	{
		m_eNavigationHeroType = ENavigationHeroType.Enhance;
		UIManager.Instance.DoUIMgrHide<SHUIFramePotentialBingo>();
		UIManager.Instance.DoUIMgrShow<SHUIFrameHeroInfomation>().DoHeroInfomationEnhance(m_hSelectedHero);
	}

	public void HandleNavigationSkill()
	{
		m_eNavigationHeroType = ENavigationHeroType.Skill;
		UIManager.Instance.DoUIMgrHide<SHUIFramePotentialBingo>();
		UIManager.Instance.DoUIMgrShow<SHUIFrameHeroInfomation>().DoHeroInfomationSkill(m_hSelectedHero);
	}

	public void HandleNavigationEquip()
	{
		m_eNavigationHeroType = ENavigationHeroType.Equipment;
		UIManager.Instance.DoUIMgrHide<SHUIFramePotentialBingo>();
		UIManager.Instance.DoUIMgrShow<SHUIFrameHeroInfomation>().DoHeroInfomationEquipment(m_hSelectedHero);
	}

	public void HandleNavigationIncense()
	{
		m_eNavigationHeroType = ENavigationHeroType.Incense;
	}

	public void HandleNavigationPotential()
	{
		m_eNavigationHeroType = ENavigationHeroType.Potential;
		UIManager.Instance.DoUIMgrHide<SHUIFrameHeroInfomation>();
		UIManager.Instance.DoUIMgrShow<SHUIFramePotentialBingo>().DoUIFramePotentialBingo(m_hSelectedHero);
	}


}
