using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NPacketData;
public class SHManagerGameSession : CManagerSessionVirtualSessionBase
{	public static new SHManagerGameSession Instance { get { return CManagerSessionVirtualSessionBase.Instance as SHManagerGameSession; }}
	//-------------------------------------------------------------------------------
	public void RequestStageRewardClear(uint hStageID, uint hRewardID)
	{

	}

	public void RequestStageRewardEnemy(uint hStageID, uint hEnemyID, int pDamageLog = 0) // 검증용 데미지 정보체 전달 
	{
		List<SItemData> pListData = new List<SItemData>();
		SItemData pItemData = new SItemData();
		pItemData.ItemID = 10007;
		pItemData.ItemEquip = false;
		SPacketItem pItemPacket = new SPacketItem();
		pItemData.ItemIDB = pItemPacket;
		pItemPacket.ItemCount = 10;
		pItemPacket.ItemTID = pItemData.ItemID;
		pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(pItemData.ItemID);
		pListData.Add(pItemData);
		
		pItemData = new SItemData();
		pItemData.ItemID = 10008;
		pItemData.ItemEquip = false;
		pItemPacket = new SPacketItem();
		pItemData.ItemIDB = pItemPacket;
		pItemPacket.ItemCount = 1;
		pItemPacket.ItemTID = pItemData.ItemID;
		pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(pItemData.ItemID);
		pListData.Add(pItemData);

		pItemData = new SItemData();
		pItemData.ItemID = 10003;
		pItemData.ItemEquip = false;
		pItemPacket = new SPacketItem();
		pItemData.ItemIDB = pItemPacket;
		pItemPacket.ItemCount = 5;
		pItemPacket.ItemLevel = 230000;
		pItemPacket.ItemTID = pItemData.ItemID;
		pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(pItemData.ItemID);
		pListData.Add(pItemData);

		pItemData = new SItemData();
		pItemData.ItemID = 10006;
		pItemData.ItemEquip = false;
		pItemPacket = new SPacketItem();
		pItemData.ItemIDB = pItemPacket;
		pItemPacket.ItemCount = 3;
		pItemPacket.ItemTID = pItemData.ItemID;
		pItemData.ItemTable = SHManagerScriptData.Instance.ExtractTableItem().GetTableItem(pItemData.ItemID);
		pListData.Add(pItemData);

		SHManagerGameDB.Instance.ResponseRewardEnemy(pListData);
	}

	public void RequestStageClear(uint hStageID, UnityAction<uint> delFinish)
	{
		//fake
		SHScriptTableStage.SStageTable pTable = SHManagerScriptData.Instance.ExtractTableStage().FindTableStageNext(hStageID);
		SPacketStageClear pStageClear = new SPacketStageClear();
		pStageClear.WorldIndex = pTable.WorldIndex;
		pStageClear.StageIndex = pTable.StageIndex;

		SHManagerGameDB.Instance.ResponseStageClear(pStageClear);
		delFinish?.Invoke(pTable.StageID);
	}

	public void RequestStatConfirmLevel(uint hHeroID, List<SIntPair> pListStatUpgrade)
	{
		SHManagerGameDB.Instance.ResponseStatUpgradeLevel(hHeroID, 0, pListStatUpgrade);
	}

	public void RequestStatConfirmGold(uint hHeroID, List<SIntPair> pListStatUpgrade)
	{
		SHManagerGameDB.Instance.ResponseStatUpgradeGold(hHeroID, 0, pListStatUpgrade);
	}

	public void RequestEquipMount(uint hHeroID, uint hItemID)
	{
		SPacketHero pHeroInfo = SHManagerGameDB.Instance.GetGameDBHeroInfo(hHeroID);
		if (pHeroInfo.EquipMain == 0)
		{
			SHManagerGameDB.Instance.ResponseEquipMain(hHeroID, hItemID);
		}
		else if (pHeroInfo.EquipSub1 == 0)
		{
			SHManagerGameDB.Instance.ResponseEquipSub1(hHeroID, hItemID);
		}
		else if (pHeroInfo.EquipSub2 == 0)
		{
			SHManagerGameDB.Instance.ResponseEquipSub2(hHeroID, hItemID);
		}
	}

	public void RequestEquipUnMount(uint hHeroID, uint hItemID)
	{
		SHManagerGameDB.Instance.ResponseEquipUnMount(hHeroID, hItemID);
	}

	//--------------------------------------------------------------------------------
}
