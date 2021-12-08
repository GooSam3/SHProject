using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
public class SHUIWindowHeroUpgradeTraining : SHUIWindowHeroUpgradeBase
{
	[SerializeField]
	private CText RemainStat = null;
	[SerializeField]
	private CButton ButtonConfirm = null;
	[SerializeField]
	private CButton ButtonReset = null;

	private int m_iPointRemain = 0;
	private int m_iPointOrigin = 0;
	//--------------------------------------------------------------------
	protected override void OnUIWindowHeroRefresh(uint hHeroID)
	{
		base.OnUIWindowHeroRefresh(hHeroID);
		SPacketHeroStatUpgrade pDBStatUpgrade = SHManagerGameDB.Instance.GetGameDBHeroStatUpgrade(hHeroID);
		if (pDBStatUpgrade != null)
		{
			m_iPointOrigin = pDBStatUpgrade.RemainStat;
			PrivHeroUpgradeTrainingValue(pDBStatUpgrade.RemainStat);
			SetMonoActive(true);
		}
		else
		{
			SetMonoActive(false);
		}
	}

	protected override bool OnHeroUpgradePlus(long iPlusPoint)
	{
		return PrivHeroUpgradeTrainingPlus();
	}

	protected override void OnHeroUpgradeConfirm()
	{
		base.OnHeroUpgradeConfirm();
		UIManager.Instance.DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "시스템", "단련 상태를 저장 하시겠습니까?", () =>
		{
			ProtHeroUpgradeTrainingSendPacket(true);
		}, ()=> {
			OnUIWindowHeroRefresh(pHeroID);
		});
	}

	protected override void OnHeroUpgradeReset()
	{
		base.OnHeroUpgradeReset();

		UIManager.Instance.DoUIMgrMessagePopup(SHUIFrameMessagePopup.EMessagePopupType.CancleOk, "시스템", "스텟을 초기화 하시겠습니까?\n(다이아 100 소모)", () =>
		{

		});
	}

	//---------------------------------------------------------------------
	private void PrivHeroUpgradeTrainingValue(int iPoint)
	{
		m_iPointRemain = iPoint;
		RemainStat.text = iPoint.ToString();
		ButtonReset.interactable = true;

		if (m_iPointOrigin == 0)
		{
			ButtonConfirm.interactable = false;
		}
		else
		{
			ButtonConfirm.interactable = true;
		}
	}

	private bool PrivHeroUpgradeTrainingPlus()
	{
		bool bPlus = false;
		if (m_iPointRemain > 0)
		{
			bPlus = true;
			m_iPointRemain--;
			PrivHeroUpgradeTrainingValue(m_iPointRemain);
		}
		return bPlus;
	}
}
