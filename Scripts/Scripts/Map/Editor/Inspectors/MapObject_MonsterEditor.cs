using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[CustomEditor(typeof(MapObject_Monster), true)]
[CanEditMultipleObjects()]
public class MapObject_MonsterEditor : MapObject_MovableBaseEditor<MapObject_Monster>
{
	static GUIContent selTableLabel = EditorGUIUtility.TrTextContent("테이블에서 TID선택", "");
	static GUIContent multiSelTableLabel = EditorGUIUtility.TrTextContent("테이블에서 TID선택 (선택된 모든 객체에 적용)", "");

	protected override void OnEnable()
	{
		base.OnEnable();

		if (null != mObject)
		{
			SetupMovementData(mObject);

			mObject.TidChanged = OnTidChanged;

			// 아 먼가....애매하다..
			if (MapToolEditor.IsLoadedGameDB)
			{
				if (MapToolEditor.instance.TableDB.MonsterTable.DicTable.TryGetValue(mObject.TableTID, out var table))
				{
					mObject.SpawnType = table.SpawnType;
				}
			}
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
				mObject.SetModel(MapToolEditor.instance.CreateMonsterModel(newTID));
		};
	}

	protected override void OnSceneGUI()
	{
		base.OnSceneGUI();

		if (MapToolEditor.IsLoadedGameDB)
		{
			if (MapToolEditor.instance.TableDB.MonsterTable.DicTable.TryGetValue(mObject.TableTID, out var table))
			{
				Handles.color = new Color(1, 1, 1, 0.05f);
				Handles.DrawSolidDisc(mObject.transform.position, Vector3.up, table.SearchRange);
				//Handles.color = Color.white;
			}
		}
	}

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		if (null == MapToolEditor.instance)
			return;

		GUI.enabled = MapToolEditor.IsLoadedGameDB;

		bool isMultiSelected = Selection.gameObjects.Length > 1;

		EditorGUILayout.Separator();
		if (GUILayout.Button(isMultiSelected ? multiSelTableLabel : selTableLabel , GUILayout.Height(40)))
		{
			var monTables = MapToolEditor.instance.GetMonsterTables(true);

			List<ItemSelectorWindow.Item> itemList = new List<ItemSelectorWindow.Item>();
			foreach (var monTable in monTables)
			{
				itemList.Add(new ItemSelectorWindow.Item()
				{
					KeyName = $"<color=red>{monTable.MonsterID.ToString()}</color> | MonType: {monTable.MonsterType} | SpawnType: {monTable.SpawnType}",
					Data = monTable
				});
			}

			uint curStageTID = null != MapToolEditor.instance.CurSceneInfo.TableData ? MapToolEditor.instance.CurSceneInfo.TableData.StageID : 0;

			ItemSelectorWindow.Show($"Monster | Stage[{curStageTID.ToString()}]", itemList, (selItem) => 
			{
				foreach (var selGO in Selection.gameObjects)
				{
					var monObj = selGO.GetComponent<MapObject_Monster>();
					if (null != monObj)
					{
						monObj.TableTID = (selItem.Data as GameDB.Monster_Table).MonsterID;
						monObj.SpawnType = (selItem.Data as GameDB.Monster_Table).SpawnType;
						monObj.OnValidate();
					}
				}

				mObject.TableTID = (selItem.Data as GameDB.Monster_Table).MonsterID;
				mObject.SpawnType = (selItem.Data as GameDB.Monster_Table).SpawnType;
				mObject.OnValidate();

				SceneView.RepaintAll();
			});
		}

		EditorGUILayout.Space();
		DrawMovableSelectorUI(mObject);

		//if (isMultiSelected)
		//{
		//	GUILayout.Space(5f);
		//	ZGUIStyles.Separator();

		//	if (GUILayout.Button("그룹 만들기", ZGUIStyles.BlueLabelButton, GUILayout.Height(35f)))
		//	{
		//		foreach (var selGO in Selection.gameObjects)
		//		{
		//			var monObj = selGO.GetComponent<MapObject_Monster>();
		//			if (null != monObj)
		//			{
		//				monObj.GroupID = (uint)Random.Range(0, int.MaxValue);
		//				monObj.OnValidate();
		//			}
		//		}
		//	}
		//}

		GUI.enabled = true;
	}
}