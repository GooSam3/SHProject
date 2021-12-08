using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPacketData;
using System.Linq;

public partial class SHManagerGameDB : CManagerTemplateBase<SHManagerGameDB>
{

	//-----------------------------------------------------------------------------
	private void PrivGameDBLocalConfigRequire()
	{
		SPacketStageClear pStageInfo = new SPacketStageClear();
		pStageInfo.WorldIndex = 1;
		pStageInfo.StageIndex = 1;

		m_mapStageClearInfo.Add(1, pStageInfo);

		SPacketHero pHeroInfo = new SPacketHero();
		pHeroInfo.HeroID = 101;
		pHeroInfo.EquipMain = 101201;
		pHeroInfo.EquipSub1 = 101202;
		m_mapHeroInfo.Add(pHeroInfo.HeroID, pHeroInfo);
		pHeroInfo = new SPacketHero();
		pHeroInfo.HeroID = 102;
		m_mapHeroInfo.Add(pHeroInfo.HeroID, pHeroInfo);
		pHeroInfo = new SPacketHero();
		pHeroInfo.HeroID = 103;
		m_mapHeroInfo.Add(pHeroInfo.HeroID, pHeroInfo);
		// 덱설정 -----------------------------------------------------
		HeroDeckInfo.SelectedDeck = 0;
		SPacketHeroDeck.SDeckInfo pDeckInfo = new SPacketHeroDeck.SDeckInfo();
		pDeckInfo.DeckMember.Add(101);
		pDeckInfo.DeckMember.Add(102);
		pDeckInfo.DeckMember.Add(103);
		HeroDeckInfo.DeckList.Add(pDeckInfo);
		HeroDeckInfo.DeckList[HeroDeckInfo.SelectedDeck].ReaderID = 101;
		//----재화 설정-------------------------------------------------------
		PlayerCurrency.Gold = 12237000;
		PlayerCurrency.Diamond = 41200;
		PlayerCurrency.Soul = 93929112;
		//----인벤토리 설정-------------------------------------------------
		SPacketItem pItem = new SPacketItem();
		pItem.ItemTID = 101101;
		pItem.ItemLevel = 1;
		pItem.ItemCount = 4;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101102;
		pItem.ItemLevel = 321;
		pItem.ItemCount = 3;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101103;
		pItem.ItemLevel = 321;
		pItem.ItemCount = 10;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101104;
		pItem.ItemLevel = 42;
		pItem.ItemCount = 4;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101201;
		pItem.ItemLevel = 111;
		pItem.ItemCount = 20;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101202;
		pItem.ItemLevel = 12;
		pItem.ItemCount = 2;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101203;
		pItem.ItemLevel = 200;
		pItem.ItemCount = 0;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101204;
		pItem.ItemLevel = 345;
		pItem.ItemCount = 1;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101301;
		pItem.ItemLevel = 23;
		pItem.ItemCount = 5;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101302;
		pItem.ItemLevel = 878;
		pItem.ItemCount = 40;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101303;
		pItem.ItemLevel = 211;
		pItem.ItemCount = 12;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101304;
		pItem.ItemLevel = 112;
		pItem.ItemCount = 5;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101401;
		pItem.ItemLevel = 312;
		pItem.ItemCount = 8;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101402;
		pItem.ItemLevel = 88;
		pItem.ItemCount = 2;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101403;
		pItem.ItemLevel = 921;
		pItem.ItemCount = 30;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101404;
		pItem.ItemLevel = 531;
		pItem.ItemCount = 23;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101501;
		pItem.ItemLevel = 531;
		pItem.ItemCount = 23;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101502;
		pItem.ItemLevel = 556;
		pItem.ItemCount = 17;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101503;
		pItem.ItemLevel = 556;
		pItem.ItemCount = 17;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101504;
		pItem.ItemLevel = 556;
		pItem.ItemCount = 17;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101601;
		pItem.ItemLevel = 64;
		pItem.ItemCount = 4;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101602;
		pItem.ItemLevel = 134;
		pItem.ItemCount = 3;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101603;
		pItem.ItemLevel = 21;
		pItem.ItemCount = 8;
		InventoryEquipment.Add(101, pItem);

		pItem = new SPacketItem();
		pItem.ItemTID = 101604;
		pItem.ItemLevel = 74;
		pItem.ItemCount = 3;
		InventoryEquipment.Add(101, pItem);

		//----------------------------------------------------------
		SPacketHeroStatUpgrade pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 101;
		pHeroStat.HeroLevel = 1;
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
		pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 102;
		pHeroStat.HeroLevel = 1;
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
		pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 103;
		pHeroStat.HeroLevel = 1;
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
	}

	private void PrivGameDBLocalConfigOption()
	{
		SPacketHeroStatUpgrade pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 101;
		pHeroStat.HeroLevel = 140;
		pHeroStat.RemainStat = 50;
		pHeroStat.UpgradePoint.Add(new SIntPair(9101, 15));
		pHeroStat.UpgradePoint.Add(new SIntPair(9102, 22));
		pHeroStat.UpgradePoint.Add(new SIntPair(9103, 30));
		pHeroStat.UpgradePoint.Add(new SIntPair(9104, 43));
		pHeroStat.UpgradePoint.Add(new SIntPair(9201, 40));
		pHeroStat.UpgradePoint.Add(new SIntPair(9202, 50));
		pHeroStat.UpgradePoint.Add(new SIntPair(9203, 60));
		pHeroStat.UpgradePoint.Add(new SIntPair(9204, 300));
		pHeroStat.UpgradePoint.Add(new SIntPair(9205, 60));
		pHeroStat.UpgradePoint.Add(new SIntPair(9206, 70));
		pHeroStat.UpgradePoint.Add(new SIntPair(9207, 30));
		pHeroStat.UpgradePoint.Add(new SIntPair(9208, 40));
		pHeroStat.UpgradePoint.Add(new SIntPair(9209, 30));
		pHeroStat.UpgradePoint.Add(new SIntPair(91013, 9));
		pHeroStat.UpgradePoint.Add(new SIntPair(91014, 10));
		pHeroStat.UpgradePoint.Add(new SIntPair(91015, 11));
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
		pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 102;
		pHeroStat.HeroLevel = 2910;
		pHeroStat.RemainStat = 1200;
		pHeroStat.UpgradePoint.Add(new SIntPair(9101, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9102, 2));
		pHeroStat.UpgradePoint.Add(new SIntPair(9103, 7));
		pHeroStat.UpgradePoint.Add(new SIntPair(9104, 4));
		pHeroStat.UpgradePoint.Add(new SIntPair(9201, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9202, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9203, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9204, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9205, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9206, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9207, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9208, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9209, 1));
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
		pHeroStat = new SPacketHeroStatUpgrade();
		pHeroStat.HeroID = 103;
		pHeroStat.HeroLevel = 5610;
		pHeroStat.RemainStat = 5200;
		pHeroStat.UpgradePoint.Add(new SIntPair(9101, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9102, 2));
		pHeroStat.UpgradePoint.Add(new SIntPair(9103, 7));
		pHeroStat.UpgradePoint.Add(new SIntPair(9104, 4));
		pHeroStat.UpgradePoint.Add(new SIntPair(9201, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9202, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9203, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9204, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9205, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9206, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9207, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9208, 1));
		pHeroStat.UpgradePoint.Add(new SIntPair(9209, 1));
		m_mapHeroStatUpgradeInfo[pHeroStat.HeroID] = pHeroStat;
	}
}
