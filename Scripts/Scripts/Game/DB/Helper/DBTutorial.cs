using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]

public class DBTutorial : IGameDBHelper
{   
    public void OnReadyData()
    {        

    }

    public static bool TryGet(uint tutorialTid, out Tutorial_Table table)
    {
        return GameDBManager.Container.Tutorial_Table_data.TryGetValue(tutorialTid, out table);
    }
}