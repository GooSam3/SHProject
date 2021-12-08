using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pawn에다가 Add해서 테스트해주세요.
/// </summary>
public class TestDropItem : MonoBehaviour
{
	public bool showGUI = true;

	Dictionary<(GameDB.E_DropModelType, byte), GameDB.Item_Table> dropItemTables = new Dictionary<(GameDB.E_DropModelType, byte), GameDB.Item_Table>();

	private void CachingDatas()
	{
		foreach (var value in GameDBManager.Container.Item_Table_data.Values)
		{
			var key = (value.DropModelType, value.Grade);
			if (dropItemTables.ContainsKey(key))
				continue;

			dropItemTables.Add(key, value);
		}
	}

	Vector2 scrolPos;
	private void OnGUI()
	{
		if (!showGUI)
			return;

		if (GameDBManager.Instance.LoadStatus != GameDBManager.E_LoadStatus.LoadingDone)
			return;

		if (dropItemTables.Count == 0)
		{
			CachingDatas();
		}

		scrolPos = GUILayout.BeginScrollView(scrolPos);
		foreach (var value in dropItemTables.Values)
		{
			if (GUILayout.Button($"ItemID : {value.ItemID}, DropType : {value.DropModelType}, Grade : {value.Grade}"))
			{
				float range = (float)DBConfig.ItemDropEffect_Max_Radius * ((float)1 / DBConfig.ItemDropEffect_Max_Standard_Cnt);
				range = Mathf.Max(range, 2f);
				Vector3 dropPos = VectorHelper.RandomCircleXZ(transform.position, range);

				DropItemSpawner.DropItem(transform, dropPos, range, value.ItemID, 1);
			}
		}
		GUILayout.EndScrollView();
	}
}
