using GameDB;
using System.Collections.Generic;
using System.Linq;

[UnityEngine.Scripting.Preserve]
public class DBStage : IGameDBHelper
{
	public void OnReadyData()
	{
	}

	public static bool TryGet(uint _tid, out GameDB.Stage_Table outTable)
	{
		return GameDBManager.Container.Stage_Table_data.TryGetValue(_tid, out outTable);
	}

    public static Dictionary<uint, Stage_Table>.ValueCollection GetAllStage()
    {
		return GameDBManager.Container.Stage_Table_data.Values;
	}

	public static GameDB.Stage_Table Get(uint _tid)
	{
		if (GameDBManager.Container.Stage_Table_data.TryGetValue(_tid, out var foundTable))
			return foundTable;

		ZLog.LogError(ZLogChannel.System, $"해당 StageTID[{_tid}]가 테이블에 존재하지 않습니다.");

		return null;
	}

	/// <summary> 대상 스테이지 정보가 사용가능한지 여부 </summary>
	public static bool IsStageUsable(uint _tid)
	{
		if (TryGet(_tid, out var table))
			return UnityEngine.Debug.isDebugBuild || table.UnusedType == E_UnusedType.Use;
		return false;
	}

	public static string GetQuestNpcName(uint _tid)
	{
		if (TryGet(_tid, out var table))
			return DBQuest.GetQuestNpcName(table.ClearQuest);

		return string.Empty;
    }


	public static System.Collections.Generic.List<Stage_Table> GetTempleTypeList(E_TempleType type)
	{
	    var stageTableList = GameDBManager.Container.Stage_Table_data.Values.ToList();
	    if (stageTableList == null)
	    {
	        ZLog.LogError(ZLogChannel.System, $"TempleTable.");
	        return null;
	    }

		System.Collections.Generic.List<Stage_Table> typeTempleTableList = new System.Collections.Generic.List<Stage_Table>();
		for( int i = 0; i< stageTableList.Count;i++)
		{
			if(DBTemple.TryGet( stageTableList[i].LinkTempleID, out var templeTable))
			{
				//RegistrationUI UI에 등록을 할것인지 말것인지
				//Debug.Log($"{templeTable.TempleID}::{templeTable.TempleType }::{type}");
				if (templeTable.TempleType == type && templeTable.HiddenUI== E_HiddenUI.None)
				{
					typeTempleTableList.Add(stageTableList[i]);
                }
			}
		}
	    return typeTempleTableList;
	}

	public static Stage_Table GetStageTableByTempleTid(uint templeTid)
    {
        if(DBTemple.TryGet(templeTid, out var templeTable))
        {
            if(DBPortal.TryGet(templeTable.EntrancePortalID, out var portalTable))
            {
                return Get(portalTable.StageID);
            }            
        }

        return null;
    }

	public static List<Stage_Table> GetStageList( E_StageType stageType )
	{
		var stageTableList = GameDBManager.Container.Stage_Table_data.Values.ToList();
		if( stageTableList == null ) {
			return null;
		}
		return stageTableList.FindAll( v => v.StageType == stageType );
	}

}
