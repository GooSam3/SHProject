using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;

public class SHUIWidgetHeroInfomationView : CUIWidgetBase
{
	[System.Serializable]
	public class SHeroInfo
	{
		public EHeroType HeroID = 0;
		public SHUIWidgetSpineHero HeroMotion = null;
	}

	[SerializeField]
	private CUIWidgetNumberTextChart HeroPower = null;
	[SerializeField]
	private CText HeroLevel = null;
	[SerializeField]
	private CButton ButtonPurchase = null;
	[SerializeField]
	private GameObject EquipTab = null;
	[SerializeField]
	private SHUIIconEquipmentSlot EquipMain = null;
	[SerializeField]
	private SHUIIconEquipmentSlot EquipSub1 = null;
	[SerializeField]
	private SHUIIconEquipmentSlot EquipSub2 = null;

	[SerializeField]
	private List<SHeroInfo> HeroMotion = null;

	private bool m_bPurchase = false;
	private uint m_hHeroID = 0;
	private SHUIWidgetSpineHero m_pHeroMotionCurrent = null;
	private SHUIIconBase m_pEquipIconCurrent = null;
	//--------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		EquipTab.SetActive(false);
	}

	//------------------------------------------------------
	public void DoHeroInfoView(uint hHeroID)
	{
		m_hHeroID = hHeroID;

		SPacketHeroStatUpgrade pStatDB = SHManagerGameDB.Instance.GetGameDBHeroStatUpgrade(hHeroID);
		if (pStatDB != null)
		{
			PrivHeroInfoViewRefresh(pStatDB);
		}
		else
		{
			PrivHeroInfoViewPurchase();
		}
		PrivHeroInfoViewHeroMotion(hHeroID);
		EquipTab.SetActive(false);
	}

	public void DoHeroInfoEquipment(uint hHeroID)
	{
		if (m_bPurchase == false)
		{
			EquipTab.SetActive(true);
			PrivHeroInfoViewEquipment(hHeroID);
		}
	}

	//----------------------------------------------------------
	private void PrivHeroInfoViewRefresh(SPacketHeroStatUpgrade pStatDB)
	{
		m_bPurchase = false;
		ButtonPurchase.gameObject.SetActive(false);
		HeroPower.DoTextNumber(SHManagerGameDB.Instance.GetGameDBHeroPower(pStatDB.HeroID));
		HeroLevel.text = string.Format("Lv {0}", pStatDB.HeroLevel);
	}

	private void PrivHeroInfoViewPurchase()
	{
		m_bPurchase = true;
		ButtonPurchase.gameObject.SetActive(true);
		HeroPower.DoTextNumber(0);
		HeroLevel.text = string.Format("Lv {0}", 1);
	}

	private void PrivHeroInfoViewHeroMotion(uint hHeroID)
	{
		SHUIWidgetSpineHero pSpineHero = FindHeroInfoHeroMotion(hHeroID);
		if (pSpineHero != null)
		{
			if (m_pHeroMotionCurrent != pSpineHero)
			{
				if (m_pHeroMotionCurrent != null)
				{
					m_pHeroMotionCurrent.DoUIWidgetShowHide(false);
				}
				m_pHeroMotionCurrent = pSpineHero;
				m_pHeroMotionCurrent.DoUIWidgetShowHide(true);
			}
		}
		else
		{

		}
	}

	private SHUIWidgetSpineHero FindHeroInfoHeroMotion(uint hHeroID)
	{
		SHUIWidgetSpineHero pFindHero = null;
		for (int i = 0; i < HeroMotion.Count; i++)
		{
			if ((uint)HeroMotion[i].HeroID == hHeroID)
			{
				pFindHero = HeroMotion[i].HeroMotion;
				break;
			}
		}

		return pFindHero;
	}

	private void PrivHeroInfoViewEquipment(uint hHeroID)
	{
		SPacketHero  pDBHero = SHManagerGameDB.Instance.GetGameDBHeroInfo(hHeroID);
		if (pDBHero != null)
		{
			SItemData pItemData = SHManagerGameDB.Instance.GetGameDBHeroEquip(hHeroID, pDBHero.EquipMain);
			EquipMain.DoIconItemData(pItemData, HandleHeroInfoEquipmentClick);

			pItemData = SHManagerGameDB.Instance.GetGameDBHeroEquip(hHeroID, pDBHero.EquipSub1);
			EquipSub1.DoIconItemData(pItemData, HandleHeroInfoEquipmentClick);

			pItemData = SHManagerGameDB.Instance.GetGameDBHeroEquip(hHeroID, pDBHero.EquipSub2);
			EquipSub2.DoIconItemData(pItemData, HandleHeroInfoEquipmentClick);
		}
	}

	//--------------------------------------------------------------------------------------
	private void HandleHeroInfoEquipmentClick(SHUIIconBase pClickIcon)
	{
		if (m_pEquipIconCurrent != null)
		{
			m_pEquipIconCurrent.DoUIIconFocus(false);
		}
		m_pEquipIconCurrent = pClickIcon;
		m_pEquipIconCurrent.DoUIIconFocus(true);
		SHUIIconEquipment pIconEquip = pClickIcon as SHUIIconEquipment;
		SHUIFrameHeroInfomation pHeroInfo = UIManager.Instance.DoUIMgrFind<SHUIFrameHeroInfomation>();
		pHeroInfo.DoHeroEquipView(pIconEquip.GetIconEquipItemData());
	} 
}
