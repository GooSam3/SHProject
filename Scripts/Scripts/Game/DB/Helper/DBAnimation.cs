using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBAnimation : IGameDBHelper
{
    public void OnReadyData()
    {
    }

    public static Dictionary<string, Animation_Table> DicAnimation
    {
        get { return GameDBManager.Container.Animation_Table_data; }
    }

    public static bool TryGet(string _tid, out Animation_Table outTable)
    {
        return GameDBManager.Container.Animation_Table_data.TryGetValue(_tid, out outTable);
    }

    public static Animation_Table Get(string _tid)
    {
        if (GameDBManager.Container.Animation_Table_data.TryGetValue(_tid, out var foundTable))
        {
            return foundTable;
        }
        return null;
    }

    public static float GetAnimLength(string _tid)
    {
        if(GameDBManager.Container.Animation_Table_data.TryGetValue(_tid, out Animation_Table outTable))
        {
            return outTable.AnimationLength;
        }

        return 1f;
    }
}
