using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBQuest : IGameDBHelper
{
    static Dictionary<E_QuestType, List<Quest_Table>> questDicByType = new Dictionary<E_QuestType, List<Quest_Table>>();

    public void OnReadyData()
    {
        questDicByType.Clear();

        foreach (var tableData in GameDBManager.Container.Quest_Table_data.Values)
        {
            if (!questDicByType.ContainsKey(tableData.QuestType))
                questDicByType.Add(tableData.QuestType, new List<Quest_Table>());

            questDicByType[tableData.QuestType].Add(tableData);
        }
    }

    public static E_QuestType GetQuestType(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid].QuestType;

      //  ZLog.LogError("GetQuestType - Can't Find Quest : "+QuestTid);
        return default;
    }

    public static E_QuestOpenType GetQuestOpenType(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid].QuestOpenType;

     //   ZLog.LogError("GetQuestOpenType - Can't Find Quest : " + QuestTid);
        return default;
    }

    public static E_CompleteCheck GetQuestCompleteType(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid].CompleteCheck;

     //   ZLog.LogError("GetQuestCompleteType - Can't Find Quest : " + QuestTid);
        return default;
    }

    public static List<Quest_Table> GetQuestList(E_QuestType questType)
    {
        if (questDicByType.ContainsKey(questType))
            return questDicByType[questType];

    //    ZLog.LogError("GetQuestList - Can't Find QuestType : "+questType);
        return null;
    }

    public static string GetQuestNpcName(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            if (DBNpc.TryGet(GameDBManager.Container.Quest_Table_data[QuestTid].QuestOpenNPCID, out var table))
                if (DBLocale.TryGet(table.NPCTextID, out var nameText))
                    return nameText.Text;
        
                    return default;
    }

    public static string GetQuestName(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid].QuestTextID;

   //     ZLog.LogError("GetQuestName - Can't Find Quest : " + QuestTid);
        return default;
    }

    public static Quest_Table GetQuestData(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid];

   //     ZLog.LogError("GetQuestData - Can't Find Quest : " + QuestTid);
        return default;
    }

	public static bool GetQuestData(uint QuestTid, out Quest_Table questTable)
	{
		return GameDBManager.Container.Quest_Table_data.TryGetValue(QuestTid, out questTable);
	}


    public static E_UIShortCut GetQuestUIShortCut(uint QuestTid)
    {
        if (GameDBManager.Container.Quest_Table_data.ContainsKey(QuestTid))
            return GameDBManager.Container.Quest_Table_data[QuestTid].UIShortCut;

   //     ZLog.LogError("GetQuestUIShortCut - Can't Find Quest : " + QuestTid);
        return 0;
    }
}
