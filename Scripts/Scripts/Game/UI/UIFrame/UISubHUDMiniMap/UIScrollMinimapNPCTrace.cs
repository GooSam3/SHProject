using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;
public class UIScrollMinimapNPCTrace : CUGUIWidgetTemplateItemBase
{
	private class SNPCTrace
	{
		public uint		NpcID = 0;
		public Vector3	Position = Vector3.zero;
		public NPC_Table	NpcTable = null; 
	}

	[SerializeField] Transform ListParent = null;

	private Dictionary<E_JobType, SNPCTrace> m_dicNPCTrace = new Dictionary<E_JobType, SNPCTrace>();
    //---------------------------------------------------------------
    public void DoNPCTaceInitialize()
	{
		ProtUIScrollSlotItemReturnAll();
		m_dicNPCTrace.Clear();

		Vector3 myPosition = ZPawnManager.Instance.MyEntity.Position;

        for(int i = 0; i < ZGameModeManager.Instance.CurrentMapData.NpcInfos.Count; i++)
		{
			MapData.NpcInfo npcInfo = ZGameModeManager.Instance.CurrentMapData.NpcInfos[i];
			NPC_Table npcTable = null;
			DBNpc.DicNpc.TryGetValue(npcInfo.TableTID, out npcTable);

			if (npcTable == null) continue;

			if (CheckNPCTraceType(npcTable) == false) continue;

			uint npcID = npcTable.NPCID;
			SNPCTrace npcTrace = null;
			if (m_dicNPCTrace.ContainsKey(npcTable.JobType))
			{
				npcTrace = m_dicNPCTrace[npcTable.JobType];
				float distanceExist = (npcTrace.Position - myPosition).magnitude;
				float distanceNew = (npcInfo.Position - myPosition).magnitude;

				if (distanceNew < distanceExist)
				{
					npcTrace.NpcID = npcID;
					npcTrace.Position = npcInfo.Position;
					npcTrace.NpcTable = npcTable;
					m_dicNPCTrace[npcTable.JobType] = npcTrace;
				}
			}
			else
			{
				npcTrace = new SNPCTrace();
				npcTrace.NpcID = npcID;
				npcTrace.Position = npcInfo.Position;
				npcTrace.NpcTable = npcTable;
				m_dicNPCTrace[npcTable.JobType] = npcTrace;
			}
		}

		Dictionary<E_JobType, SNPCTrace>.Enumerator it = m_dicNPCTrace.GetEnumerator();
		while(it.MoveNext())
		{
			UIScrollMinimapNPCTraceItem item = ProtUIScrollSlotItemRequest() as UIScrollMinimapNPCTraceItem;
			item.gameObject.transform.SetParent(ListParent);
			item.DoMinimapNpcTraceInfo(it.Current.Value.NpcTable, HandleNPCTraceItem);
		}
	}

	//--------------------------------------------------------------
	private void HandleNPCTraceItem(uint _npcID)
	{
		Dictionary<E_JobType, SNPCTrace>.Enumerator it = m_dicNPCTrace.GetEnumerator();
		while (it.MoveNext())
		{
			if (it.Current.Value.NpcID == _npcID)
			{
				SNPCTrace npcTrace = it.Current.Value;
				ZPawnManager.Instance.MyEntity.StopAI(E_PawnAIType.TalkNpc);
				ZPawnManager.Instance.MyEntity.StartAIForTalkNpc(ZGameModeManager.Instance.StageTid, npcTrace.Position, npcTrace.NpcID);
			}
		}
		DoUIWidgetShowHide(false);
	}

	private bool CheckNPCTraceType(NPC_Table _npcTable)
	{
		bool traceType = false;

		if (_npcTable.JobType == E_JobType.Trade || _npcTable.JobType == E_JobType.Store || _npcTable.JobType == E_JobType.Storage || _npcTable.JobType == E_JobType.SkillStore || _npcTable.JobType == E_JobType.Cleric)
		{
			traceType = true;
		}
		return traceType;
	}
}

