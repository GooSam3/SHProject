using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;

public class SHUIWindowHeroUpgradeEnhance : SHUIWindowHeroUpgradeBase
{
	[SerializeField]
	private CUIWidgetNumberTextChart TotalCost = null;
	[SerializeField]
	private CUIWidgetNumberTextChart CurrentCost = null;
	[SerializeField]
	private CButton ButtonConfirm = null;

	private long m_iTotalCost = 0;
	private uint m_iCurrency = 0;
	private bool m_bOverCost = false;
	//------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
	}

	protected override void OnHeroUpgradeReset()
	{
		base.OnHeroUpgradeReset();
		PrivHeroUpgradeCostReset();
	}

	protected override void OnHeroUpgradeConfirm()
	{
		base.OnHeroUpgradeConfirm();
		UIManager.Instance.DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "시스템", "강화 상태를 저장 하시겠습니까?", () =>
		{
			ProtHeroUpgradeTrainingSendPacket(false);
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

	protected override void OnUIWindowHeroRefresh(uint hHeroID)
	{
		base.OnUIWindowHeroRefresh(hHeroID);
		PrivHeroUpgradeCostReset();
	}

	//-------------------------------------------------------------------
	private void PrivHeroUpgradeCost(long iPlusPoint)
	{
		m_iTotalCost += iPlusPoint;
		TotalCost.DoTextNumber(m_iTotalCost);
		TotalCost.SetTextColor(Color.white);
		ButtonConfirm.interactable = true;
	}

	private void PrivHeroUpgradeCostOver(long iPlusPoint)
	{
		m_iTotalCost += iPlusPoint;
		TotalCost.DoTextNumber(m_iTotalCost);
		Color rColor;
		ColorUtility.TryParseHtmlString("#FF3D3D", out rColor);
		TotalCost.SetTextColor(rColor);
		ButtonConfirm.interactable = false;
	}

	private void PrivHeroUpgradeCostReset()
	{
		m_bOverCost = false;
		m_iTotalCost = 0;
		TotalCost.DoTextNumber(m_iTotalCost);
		TotalCost.SetTextColor(Color.white);
		m_iCurrency = SHManagerGameDB.Instance.GetGameDBCurrency(ECurrencyType.Gold);
		CurrentCost.DoTextNumber(m_iCurrency);
	}

}
