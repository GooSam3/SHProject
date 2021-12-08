using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapObject_Portal), true)]
[CanEditMultipleObjects()]
public class MapObject_PortalEditor : MapObject_MovableBaseEditor<MapObject_Portal>
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (null == MapToolEditor.instance)
			return;

		GUI.enabled = MapToolEditor.IsLoadedGameDB;

		EditorGUILayout.Separator();
		if (GUILayout.Button("테이블에서 TID선택", GUILayout.Height(40)))
		{
			var npcTables = MapToolEditor.instance.GetPortalTables(true);

			List<ItemSelectorWindow.Item> itemList = new List<ItemSelectorWindow.Item>();
			foreach (var portalTable in npcTables)
			{
				itemList.Add(new ItemSelectorWindow.Item()
				{
					KeyName = $"<color=red>{portalTable.PortalID.ToString()}</color> | PortalType: {portalTable.PortalType}",
					Data = portalTable
				});
			}

			uint curStageTID = null != MapToolEditor.instance.CurSceneInfo.TableData ? MapToolEditor.instance.CurSceneInfo.TableData.StageID : 0;

			ItemSelectorWindow.Show($"Npc | Stage[{curStageTID.ToString()}]", itemList, (selItem) =>
			{
				mObject.TableTID = (selItem.Data as GameDB.Portal_Table).PortalID;
				mObject.OnValidate();
			});
		}

		GUI.enabled = true;
	}

	protected override void OnSceneGUI()
	{
		base.OnSceneGUI();

		
		Handles.color = new Color(1, 0, 1, 0.2f);
		Handles.DrawSolidDisc(mObject.transform.position, Vector3.up, mObject.Radius);

		Handles.color = Color.white;
		mObject.Radius = Handles.ScaleValueHandle(
			mObject.Radius,
			mObject.transform.position,
			Quaternion.Euler(90, 0, 0),
			4f,
			Handles.CircleHandleCap,
			1f);
	}
}