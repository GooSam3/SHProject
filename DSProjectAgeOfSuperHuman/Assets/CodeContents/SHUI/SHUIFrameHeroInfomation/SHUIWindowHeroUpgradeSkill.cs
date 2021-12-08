using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;

public class SHUIWindowHeroUpgradeSkill : SHUIWindowHeroBase
{
	[SerializeField]
	private CUIWidgetNumberTextChart TotalCost = null;
	[SerializeField]
	private CUIWidgetNumberTextChart CurrentGold = null;

	[SerializeField]
    private List<SHUIWidgetHeroSkillUpgradeItem> SkillUpgradeItem = null;
	[SerializeField]
	private SHUIWindowHeroSkillView SkillView = null;

	private long m_iTotalCost = 0;
	private uint m_iCurrency = 0;
	private bool m_bOverCost = false;
	//--------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		SkillView.DoUIWidgetShowHide(false);
	}

	protected override void OnUIWidgetShowHide(bool bShow)
	{
		base.OnUIWidgetShowHide(bShow);
		if (bShow == false)
		{
			SkillView.DoUIWidgetShowHide(false);
		}
	}
	//------------------------------------------------------------------
	protected override void OnUIWindowHeroRefresh(uint hHeroID) 
    {
		PrivHeroUpgradeCostReset();
		for (int i = 0; i < SkillUpgradeItem.Count; i++)
		{
            SkillUpgradeItem[i].DoHeroSkillUpgradeRefresh(hHeroID, this);
		}
		SkillView.DoUIWidgetShowHide(false);
    }

	protected override void OnHeroUpgradeReset()
	{
		base.OnHeroUpgradeReset();
		PrivHeroUpgradeCostReset();
	}

	protected override void OnHeroUpgradeConfirm()
	{
		base.OnHeroUpgradeConfirm();
		UIManager.Instance.DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "시스템", "기술 연마 상태를 저장 하시겠습니까?", () =>
		{
			PrivHeroUpgradeSendPacket();
		}, () =>
		{
			OnUIWindowHeroRefresh(pHeroID);
		});
	}

	protected override bool OnHeroUpgradePlus(long iPlusPoint)
	{
		bool bResult = false;

		if (iPlusPoint + m_iTotalCost <= m_iCurrency)
		{
			m_bOverCost = false;
			bResult = true;
			PrivHeroUpgradeCost(iPlusPoint);
		}
		else
		{
			if (m_bOverCost == false)
			{
				m_bOverCost = true;
				PrivHeroUpgradeCostOver(iPlusPoint);
			}
		}
		return bResult;
	}

	//----------------------------------------------------------------
	public void DoHeroUpgradeSkillView(uint hSkillID, uint iPoint, uint iTotalGold, bool bForceShow)
	{
		if (bForceShow)
		{
			SkillView.DoUIWidgetShowHide(true);
		}

		if (SkillView.gameObject.activeSelf)
		{
			SkillView.DoHeroSkillView(hSkillID, iPoint, iTotalGold, () =>
			{
				PrivHeroUpgradeUpgradeItem(hSkillID);
			});
		}
	}

	//-------------------------------------------------------------------
	private void PrivHeroUpgradeCost(long iPlusPoint)
	{
		m_iTotalCost += iPlusPoint;
		TotalCost.DoTextNumber(m_iTotalCost);
		TotalCost.SetTextColor(Color.white);
	}

	private void PrivHeroUpgradeCostOver(long iPlusPoint)
	{
		m_iTotalCost += iPlusPoint;
		TotalCost.DoTextNumber(m_iTotalCost);
		Color rColor;
		ColorUtility.TryParseHtmlString("#FF3D3D", out rColor);
		TotalCost.SetTextColor(rColor);
	}

	private void PrivHeroUpgradeCostReset()
	{
		m_bOverCost = false;
		m_iTotalCost = 0;
		TotalCost.DoTextNumber(m_iTotalCost);
		TotalCost.SetTextColor(Color.white);
		m_iCurrency = SHManagerGameDB.Instance.GetGameDBCurrency(ECurrencyType.Gold);
		CurrentGold.DoTextNumber(m_iCurrency);
	}

	private void PrivHeroUpgradeSendPacket()
	{
		List<SIntPair> pListUpgradeStat = new List<SIntPair>();
		for (int i = 0; i < SkillUpgradeItem.Count; i++)
		{
			uint iPoint = (uint)SkillUpgradeItem[i].GetHeroSkillConsumePoint();
			if (iPoint > 0)
			{
				SIntPair rPair = new SIntPair();
				rPair.Value1 = SkillUpgradeItem[i].GetHeroSkillUpgradeID();
				rPair.Value2 = iPoint;
				pListUpgradeStat.Add(rPair);
			}
		}
		SHManagerGameSession.Instance.RequestStatConfirmLevel(pHeroID, pListUpgradeStat);
	}	

	private void PrivHeroUpgradeUpgradeItem(uint hSkillID)
	{
		for (int i = 0; i < SkillUpgradeItem.Count; i++)
		{
			if (SkillUpgradeItem[i].GetHeroSkillUpgradeSkillID() == hSkillID)
			{
				SkillUpgradeItem[i].DoHeroSkillUpgradePlus();
			}
		}
	}
	//-----------------------------------------------------------------
	
}
