using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;

public partial class SHManagerGameDB : CManagerTemplateBase<SHManagerGameDB>
{
	private SPacketCurrency		PlayerCurrency	= new SPacketCurrency();
	private SPacketPlayerAccount	PlayerAccount		= new SPacketPlayerAccount();

	private CMultiSortedDictionary<uint, SPacketItem> InventoryEquipment = new CMultiSortedDictionary<uint, SPacketItem>();
	//-----------------------------------------------------------------------------
	public float GetGameDBPowerDeck()
	{
		return ExtractGameDBPowerDeck(HeroDeckInfo.SelectedDeck);
	}

	public float GetGameDBPowerAllDeck()
	{
		float fAllDeck = 0;
		for (int i = 0; i < HeroDeckInfo.DeckList.Count; i++)
		{
			fAllDeck += ExtractGameDBPowerDeck(i);
		}
		return fAllDeck;
	}

	public string GetGameDBPlayerName()
	{
		return PlayerAccount.PlayerName;
	}

	public uint GetGameDBCurrency(ECurrencyType eCurrentType)
	{
		uint iValue = 0;
		switch(eCurrentType)
		{
			case ECurrencyType.Gold:
				iValue = PlayerCurrency.Gold;
				break;
			case ECurrencyType.Diamond:
				iValue = PlayerCurrency.Diamond;
				break;
		}

		return iValue;
	}

	public List<SPacketItem> GetGameDBHeroEquip(uint hHeroID)
	{
		List<SPacketItem> pListInventory = new List<SPacketItem>();
		if (InventoryEquipment.ContainsKey(hHeroID))
		{
			List<SPacketItem> pList = InventoryEquipment[hHeroID];

			for (int i = 0; i < pList.Count; i++)
			{
				pListInventory.Add(pList[i].CopyInstance<SPacketItem>());
			}
		}

		return pListInventory;
	}

	public SItemData GetGameDBHeroEquip(uint hHeroID, uint hItemTID)
	{
		SItemData pItemData = null;
		if (m_mapHeroInfo.ContainsKey(hHeroID) == false) { return null; }

		SPacketHero pHeroInfo = m_mapHeroInfo[hHeroID];

		if (InventoryEquipment.ContainsKey(hHeroID))
		{
			List<SPacketItem> pListPacketItem = InventoryEquipment[hHeroID];
			for(int i = 0; i < pListPacketItem.Count; i++)
			{
				if (pListPacketItem[i].ItemTID == hItemTID)
				{
					pItemData = new SItemData();
					pItemData.ItemID = pListPacketItem[i].ItemTID;
					pItemData.ItemIDB = pListPacketItem[i].CopyInstance<SPacketItem>();
					pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(hItemTID);

					if (pHeroInfo.EquipMain == hItemTID || pHeroInfo.EquipSub1 == hItemTID || pHeroInfo.EquipSub2 == hItemTID)
					{
						pItemData.ItemEquip = true;
					}

					break;
				}
			}
		}
		return pItemData;
	}

	//----------------------------------------------------------------------------
	private uint ExtractGameDBPowerDeck(int iIndex)
	{
		uint fTotalPower = 0;
		SPacketHeroDeck.SDeckInfo pDeckInfo = HeroDeckInfo.DeckList[iIndex];
		for(int i = 0; i < pDeckInfo.DeckMember.Count; i++)
		{
			fTotalPower += ExtractGameDBPowerHero(pDeckInfo.DeckMember[i]);
		}

		return fTotalPower;
	}

	private uint ExtractGameDBPowerHero(uint hHeroID)
	{
		float fTotalPower = 0;
		SPacketStatValue pStatValue = GetGameDBHeroStatCache(hHeroID);
		if (pStatValue == null)
		{
			Debug.LogErrorFormat("[StatCache] Invalid ID {0}", hHeroID);
			return 0;
		}

		// 기본 전투력 
		float fConst = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.PowerFactorOfBasic);
		float fValue = (pStatValue.LevelAttack + pStatValue.LevelDefense + pStatValue.LevelStamina) * fConst;
		fTotalPower += fValue;
		// 스킬 전투력 
		fConst = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.PowerFactorOfSkill);
		fValue = (pStatValue.LevelAttackSkill + pStatValue.LevelDefenseSkill) * fConst;
		fTotalPower += fValue;
		// 치명타 전투력 
		fConst = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.PowerFactorOfCritical);
		fValue = (pStatValue.LevelCritical + pStatValue.LevelCriticalAnti + pStatValue.LevelCriticalDamage + pStatValue.LevelCriticalDamageAnti) * fConst;
		fTotalPower += fValue;
		// 명중 전투력 
		fConst = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.PowerFactorOfHit);
		fValue = (pStatValue.LevelHit + pStatValue.LevelDodge) * fConst;
		fTotalPower += fValue;
		// 회복력등 
		fConst = SHManagerGameConfig.Instance.GetGameDBConfigFloat(SHManagerGameConfig.EGameConfigKey.PowerFactorOfRecover);
		fValue = (pStatValue.LevelRecoverPerSecond) * fConst;
		fTotalPower += fValue;

		return (uint)fTotalPower;
	}
}