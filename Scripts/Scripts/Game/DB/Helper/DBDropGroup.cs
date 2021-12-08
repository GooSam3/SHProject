using GameDB;
using System.Collections.Generic;
using System.Linq;
[UnityEngine.Scripting.Preserve]
public class DBDropGroup : IGameDBHelper
{
    static Dictionary<uint, List<DropGroup_Table>> dicDropGroup = new Dictionary<uint, List<DropGroup_Table>>();

    public void OnReadyData()
    {
        Dictionary<uint, DropGroup_Table>.Enumerator it = GameDBManager.Container.DropGroup_Table_data.GetEnumerator();
        while(it.MoveNext())
		{
            DropGroup_Table dropGroupTable = it.Current.Value;
            uint dropGroupID = dropGroupTable.DropGroupID;

            List<DropGroup_Table> dropList = FindOrAllocDropGroup(dropGroupID);
            dropList.Add(dropGroupTable);
		}
    } 

    //---------------------------------------------------------
    private static List<DropGroup_Table> FindOrAllocDropGroup(uint _dropGroupID)
	{
        List<DropGroup_Table> findList = null;
        if (dicDropGroup.ContainsKey(_dropGroupID))
        {
            findList = dicDropGroup[_dropGroupID];
        }
        else
		{
            findList = new List<DropGroup_Table>();
            dicDropGroup.Add(_dropGroupID, findList);
		}
        return findList;
    }

    //---------------------------------------------------------
    public static List<DropGroup_Table> GetDropGroupList(uint _dropGroupID)
	{   // 외부 사용시 원본의 훼손을 방지하기 위한 클론을 전달한다. GC됨을 주의.
        return FindOrAllocDropGroup(_dropGroupID).ToList();
    }

}
