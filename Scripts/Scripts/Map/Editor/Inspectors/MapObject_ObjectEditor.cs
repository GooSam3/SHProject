using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapObject_Object), true)]
[CanEditMultipleObjects()]
public class MapObject_ObjectEditor : MapObject_MovableBaseEditor<MapObject_Object>
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
				mObject.SetModel(MapToolEditor.instance.CreateObjectModel(newTID));
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
			var dicObjectTable = MapToolEditor.instance.TableDB.ObjectTable.DicTable;

			List<ItemSelectorWindow.Item> itemList = new List<ItemSelectorWindow.Item>();
			foreach (var objectTable in dicObjectTable.Values)
			{
				itemList.Add(new ItemSelectorWindow.Item()
				{
					KeyName = $"<color=red>{objectTable.ObjectID.ToString()}</color> | {objectTable.ObjectTextID} | {objectTable.ObjectType}",
					Data = objectTable
                });
			}

			uint curStageTID = null != MapToolEditor.instance.CurSceneInfo.TableData ? MapToolEditor.instance.CurSceneInfo.TableData.StageID : 0;

			ItemSelectorWindow.Show($"Object | Stage[{curStageTID.ToString()}]", itemList, (selItem) =>
			{
				mObject.TableTID = (selItem.Data as GameDB.Object_Table).ObjectID;
				mObject.OnValidate();
			});
		}

		DrawMovableSelectorUI(mObject);

		GUI.enabled = true;
	}
}