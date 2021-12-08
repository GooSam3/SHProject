using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapObject_Npc), true)]
[CanEditMultipleObjects()]
public class MapObject_NpcEditor : MapObject_MovableBaseEditor<MapObject_Npc>
{
	protected override void OnEnable()
	{
		base.OnEnable();

		if (null != mObject)
		{
			SetupMovementData(mObject);

			mObject.TidChanged = OnTidChanged;
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		mObject.TidChanged = null;
	}

	private void OnTidChanged(uint newTID)
	{
		EditorApplication.delayCall += () =>
		{
			if (null != MapToolEditor.instance)
				mObject.SetModel(MapToolEditor.instance.CreateNPCModel(newTID));
		};
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (null == MapToolEditor.instance)
			return;

		GUI.enabled = MapToolEditor.IsLoadedGameDB;

		EditorGUILayout.Separator();
		if (GUILayout.Button("테이블에서 TID선택", GUILayout.Height(40)))
		{
			var dicNpcTable = MapToolEditor.instance.TableDB.NPCTable.DicTable;

			List<ItemSelectorWindow.Item> itemList = new List<ItemSelectorWindow.Item>();
			foreach (var npcTable in dicNpcTable.Values)
			{
				itemList.Add(new ItemSelectorWindow.Item()
				{
					KeyName = $"<color=red>{npcTable.NPCID.ToString()}</color> | {npcTable.NPCTextID} | {npcTable.NPCType} | {npcTable.JobType}",
					Data = npcTable
				});
			}

			uint curStageTID = null != MapToolEditor.instance.CurSceneInfo.TableData ? MapToolEditor.instance.CurSceneInfo.TableData.StageID : 0;

			ItemSelectorWindow.Show($"Npc | Stage[{curStageTID.ToString()}]", itemList, (selItem) =>
			{
				mObject.TableTID = (selItem.Data as GameDB.NPC_Table).NPCID;
				mObject.OnValidate();
			});
		}

		DrawMovableSelectorUI(mObject);

		GUI.enabled = true;
	}
}