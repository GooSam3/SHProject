using MmoNet;
using System;
using System.Collections.Generic;
using GameDB;

/// <summary> Entity Data 및 Entity Object 관련 Container </summary>
public class ZPawnContainer
{
	private Action<uint, ZPawn> mEventCreateEntity;
	private Action<uint> mEventRemoveEntity;

	public Dictionary<uint, ZPawn>.ValueCollection DicEntity => m_DicEntity.Values;

	/// <summary> 모든 Entity의 데이터 </summary>
	private Dictionary<uint, EntityDataBase> m_DicEntityData = new Dictionary<uint, EntityDataBase>();
	/// <summary> 모든 Entity Object </summary>
	private Dictionary<uint, ZPawn> m_DicEntity = new Dictionary<uint, ZPawn>();
	/// <summary> 생성할 Entity Object 리스트 </summary>
	private List<uint> m_listInstantiateEntity = new List<uint>();
	/// <summary> 한 Tick에 생성할 Entity Ojbect Count </summary>
	private const int MAX_CREATINGCOUNT_PER_FRAME = 10;

	/// <summary> 관리되는 Entity 추가 </summary>
	public void DoAdd(S2C_AddCharInfo info)
	{
		uint entityId = info.Objectid;
		if (m_DicEntityData.ContainsKey(entityId))
		{
			ZLog.Log(ZLogChannel.Entity, ZLogLevel.Warning, $"{entityId} 의 Endity가 이미 존재함. 삭제 후 재생성함");
			DoRemove(entityId);
		}

		EntityDataBase data = EntityFactory.CreateEntityData(info);
		DoAdd(entityId, data);
	}

	public void DoAdd(S2C_AddMonsterInfo info)
	{
		uint entityId = info.Objectid;
		if (m_DicEntityData.ContainsKey(entityId))
		{
			ZLog.Log(ZLogChannel.Entity, ZLogLevel.Warning, $"{entityId} 의 Endity가 이미 존재함. 삭제 후 재생성함");
			DoRemove(entityId);
		}

		EntityDataBase data = EntityFactory.CreateEntityData(info);
		DoAdd(entityId, data);
	}

	public void DoAdd(S2C_AddNPCInfo info)
	{
		uint entityId = info.Objectid;
		if (m_DicEntityData.ContainsKey(entityId))
		{
			ZLog.Log(ZLogChannel.Entity, ZLogLevel.Warning, $"{entityId} 의 Endity가 이미 존재함. 삭제 후 재생성함");
			DoRemove(entityId);
		}

		EntityDataBase data = EntityFactory.CreateEntityData(info);
		DoAdd(entityId, data);
	}

	public void DoAdd(S2C_AddGatherObj info)
	{
		uint entityId = info.Objectid;
		if (m_DicEntityData.ContainsKey(entityId))
		{
			ZLog.Log(ZLogChannel.Entity, ZLogLevel.Warning, $"{entityId} 의 Endity가 이미 존재함. 삭제 후 재생성함");
			DoRemove(entityId);
		}

		EntityDataBase data = EntityFactory.CreateEntityData(info);
		DoAdd(entityId, data);
	}


	private void DoAdd(uint entityId, EntityDataBase data)
	{
		m_DicEntityData.Add(entityId, data);

		//미리 추가해 둠.
		m_DicEntity.Add(entityId, null);

		m_listInstantiateEntity.Add(entityId);
	}

	/// <summary> 관리되는 Entity 제거 </summary>
	public void DoRemove(uint entityId)
	{
		if (m_DicEntityData.ContainsKey(entityId))
		{
			if (m_DicEntity.TryGetValue(entityId, out var entity))
			{
				if (null != entity)
				{
					mEventRemoveEntity?.Invoke(entityId);
					entity.DestroyEntity();
				}
			}
		}

		m_DicEntityData.Remove(entityId);
		m_DicEntity.Remove(entityId);
		m_listInstantiateEntity.Remove(entityId);
	}

	/// <summary> 해당 Entity Id List의 Entity 데이터 및 Entity Object 제거 </summary>
	public void DoRemoveList(List<uint> entityIdList)
	{
		foreach (uint entityId in entityIdList)
		{
			DoRemove(entityId);
		}
	}

	/// <summary> 모든 Entity 데이터 및 Entity Object 제거 </summary>
	public void DoClear()
	{
		ClearEntityDataList();
		ClearEntityList();
	}

	/// <summary> 모든 Entity 데이터 제거 </summary>
	private void ClearEntityDataList()
	{
		m_DicEntityData.Clear();
	}

	/// <summary> 모든 Entity Object 제거 </summary>
	private void ClearEntityList()
	{
		foreach (KeyValuePair<uint, ZPawn> entity in m_DicEntity)
		{
			if (entity.Key == ZPawnManager.Instance.MyEntityId)
				continue;


			if (null == entity.Value)
				continue;

			mEventRemoveEntity?.Invoke(entity.Key);

			entity.Value.DestroyEntity();
		}

		//내 캐릭터는 마지막에 지움
		if (null != ZPawnManager.Instance.MyEntity)
		{
			mEventRemoveEntity?.Invoke(ZPawnManager.Instance.MyEntityId);
			//ZPawnManager.Instance.MyEntity?.DestroyEntity();
		}

		m_DicEntity.Clear();
		m_listInstantiateEntity.Clear();
	}

	/// <summary> 해당 EntityId 의 Entity 데이터를 얻어온다. </summary>
	public EntityDataBase GetEntityData(uint entityId)
	{
		if (m_DicEntityData.TryGetValue(entityId, out EntityDataBase data))
		{
			return data;
		}

		return null;
	}

	/// <summary> 해당 EntityId 의 Entity 데이터를 얻어온다. </summary>
	public bool TryGetEntityData(uint entityId, out EntityDataBase data)
	{
		return m_DicEntityData.TryGetValue(entityId, out data);
	}

	/// <summary> 해당 EntityId 의 Entity Object를 얻어온다. </summary>
	public ZPawn GetEntity(uint entityId)
	{
		if (m_DicEntity.TryGetValue(entityId, out ZPawn entity))
		{
			return entity;
		}

		return null;
	}

	/// <summary> 해당 EntityId 의 Entity Object를 얻어온다. </summary>
	public bool TryGetEntity(uint entityId, out ZPawn entity)
	{
		m_DicEntity.TryGetValue(entityId, out entity);

		return null != entity;
	}

	/// <summary> charID로 EntityDataBase 를 얻는다, EntityObject를 얻고싶다면 EntityDataBase 의 EntityId로 다시 찾을것(안정성&성능)  </summary>
	public EntityDataBase FindEntityDataByCharID(ulong charId)
	{
		var it = m_DicEntityData.GetEnumerator();
		while (it.MoveNext())
		{
			if (it.Current.Value.CharacterId == charId)
			{
				return it.Current.Value;
			}
		}
		return null;
	}

	public EntityBase FindEntityByCharID(ulong charId)
	{
		var it = m_DicEntity.GetEnumerator();
		while (it.MoveNext())
		{
			if (it.Current.Value.EntityData.CharacterId == charId)
			{
				return it.Current.Value;
			}
		}
		return null;
	}

	public EntityBase FindEntityByTid(uint tid, E_UnitType entityType)
	{
		var it = m_DicEntity.GetEnumerator();
		while (it.MoveNext())
		{
			if (null == it.Current.Value || null == it.Current.Value.EntityData || it.Current.Value.EntityType != entityType)
				continue;

			if (it.Current.Value.EntityData.TableId == tid)
			{
				return it.Current.Value;
			}
		}
		return null;
	}

	public List<ZPawn> GetAllPawn(E_UnitType type, uint tid = 0)
	{
		if(type == E_UnitType.None)
		{
			return new List<ZPawn>(m_DicEntity.Values);
		}
		
		List<ZPawn> list = new List<ZPawn>();

		var it = m_DicEntity.GetEnumerator();
		while (it.MoveNext())
		{
			if (null == it.Current.Value || null == it.Current.Value.EntityData || it.Current.Value.EntityType != type)
				continue;

			if (0 < tid && it.Current.Value.EntityData.TableId != tid)
				continue;

			list.Add(it.Current.Value);
		}

		return list;
	}

	/// <summary> Entity Data를 토대로 Entity Object를 생성한다. </summary>
	public void DoUpdate()
	{
		int count = MAX_CREATINGCOUNT_PER_FRAME;
		while (0 < m_listInstantiateEntity.Count && 0 < count)
		{
			uint targetEntityID = m_listInstantiateEntity[0];

			if (TryGetEntityData(targetEntityID, out EntityDataBase data))
			{
				m_DicEntity[targetEntityID] = EntityFactory.CreatePawn(data);

				mEventCreateEntity?.Invoke(targetEntityID, m_DicEntity[targetEntityID]);
			}

			m_listInstantiateEntity.RemoveAt(0);
			--count;
		}
	}

	#region ::======== Delegate ========::
	public void DoAddEventCreateEntity(Action<uint, ZPawn> action)
	{
		DoRemoveEventCreateEntity(action);
		mEventCreateEntity += action;
	}

	public void DoRemoveEventCreateEntity(Action<uint, ZPawn> action)
	{
		mEventCreateEntity -= action;
	}

	public void DoAddEventRemoveEntity(Action<uint> action)
	{
		DoRemoveEventRemoveEntity(action);
		mEventRemoveEntity += action;
	}

	public void DoRemoveEventRemoveEntity(Action<uint> action)
	{
		mEventRemoveEntity -= action;
	}
	#endregion
}
