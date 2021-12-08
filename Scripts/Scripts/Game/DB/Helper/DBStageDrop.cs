using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// 스테이지 드랍
/// 스테이지 드랍 리스트
/// 테이블 관리
/// </summary>

[UnityEngine.Scripting.Preserve]
class DBStageDrop : IGameDBHelper
{
	// { stageDropID : StageDropTable }
	private static Dictionary<uint, StageDrop_Table> dicStageDrop = new Dictionary<uint, StageDrop_Table>();

	// { ListGroupID : List<StageDropList> }
	private static Dictionary<uint, List<StageDropList_Table>> dicStageDropList = new Dictionary<uint, List<StageDropList_Table>>();

	public void OnReadyData()
	{
		dicStageDrop.Clear();
		dicStageDropList.Clear();

		foreach(var iter in GameDBManager.Container.StageDrop_Table_data.Values)
		{
			if (dicStageDrop.ContainsKey(iter.StageDropID) == false)
				dicStageDrop.Add(iter.StageDropID, iter);
		}

		foreach(var iter in GameDBManager.Container.StageDropList_Table_data.Values)
		{
			if (dicStageDropList.ContainsKey(iter.ListGroupID) == false)
				dicStageDropList.Add(iter.ListGroupID, new List<StageDropList_Table>());

			dicStageDropList[iter.ListGroupID].Add(iter);
		}
	}

	public static bool GetStageDropData(uint stageDropID, out StageDrop_Table table)
	{
		return dicStageDrop.TryGetValue(stageDropID, out table);
	}

	public static bool GetStageDropListData(uint listGroupID, out List<StageDropList_Table> listTable)
	{
		return dicStageDropList.TryGetValue(listGroupID, out listTable);
	}

	// 첫번째 드랍 아이템을 가져옴
	public static bool GetDropItemFirst(uint stageDropID, out Item_Table itemTable)
	{
		itemTable = null;

		if (GetStageDropData(stageDropID, out var table) == false)
			return false;

		if (GetStageDropListData(table.DropListGroupID, out var listTable) == false)
			return false;

		uint itemId = listTable[0].DropItemID;

		return DBItem.GetItem(itemId, out itemTable);
	}
}
