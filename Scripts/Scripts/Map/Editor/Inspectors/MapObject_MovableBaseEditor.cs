using UnityEngine;
using UnityEditor;

public class MapObject_MovableBaseEditor<MAP_OBJECT_TYPE> : MapObjectBaseEditor<MAP_OBJECT_TYPE> where MAP_OBJECT_TYPE : MonoBehaviour, IMapObjectBase
{
	protected void SetupMovementData(MapObject_MovableBase _movableObj)
	{
		if (null == _movableObj.MovingData)
		{
			//_monster.MovingData = new MovementData();
			Debug.LogError($"MovementData is null", target);
		}
		else
		{
			//
			// 데이터를 에디터에서 편집할수 있도록 연결.
			//
			switch (_movableObj.MovingData.UsingType)
			{
				case MovementCollectionData.E_MovementType.Waypoint:
					{
						if (!_movableObj.gameObject.HasComponent<WaypointsComponent>())
						{
							var comp = _movableObj.gameObject.GetOrAddComponent<WaypointsComponent>();
							comp.Assign(_movableObj.MovingData.WaypointsData);
						}
					}
					break;

				case MovementCollectionData.E_MovementType.RandomRadius:
					{
						if (!_movableObj.gameObject.HasComponent<RandomRadiusComponent>())
						{
							var comp = _movableObj.gameObject.GetOrAddComponent<RandomRadiusComponent>();
							comp.Assign(_movableObj.MovingData.RandomRadiusData);
						}
					}
					break;
			}
		}
	}

	protected void DrawMovableSelectorUI(MapObject_MovableBase _movableObj)
	{
		if (null == _movableObj.MovingData)
		{
		}
		else
		{
			EditorGUI.BeginChangeCheck();
			_movableObj.MovingData.UsingType = (MovementCollectionData.E_MovementType)EditorGUILayout.EnumPopup($"이동 데이터", _movableObj.MovingData.UsingType);
			if (EditorGUI.EndChangeCheck())
			{
				mObject.gameObject.DestroyComponent<WaypointsComponent>();
				mObject.gameObject.DestroyComponent<RandomRadiusComponent>();

				switch (_movableObj.MovingData.UsingType)
				{
					case MovementCollectionData.E_MovementType.None:
						{
							_movableObj.MovingData = new MovementCollectionData();
						}
						break;
					case MovementCollectionData.E_MovementType.Waypoint:
						{
							var data = new MovementCollectionData(new MovementData_WaypointData());
							_movableObj.MovingData = data;

							var comp = mObject.gameObject.GetOrAddComponent<WaypointsComponent>();
							comp.Assign(data.WaypointsData);
						}
						break;
					case MovementCollectionData.E_MovementType.RandomRadius:
						{
							var data = new MovementCollectionData(new MovementData_RandomRadius());
							_movableObj.MovingData = data;

							var comp = mObject.gameObject.GetOrAddComponent<RandomRadiusComponent>();
							comp.Assign(data.RandomRadiusData);
						}
						break;
				}
			}
		}
	}
}
