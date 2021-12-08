using System.Collections.Generic;
using GameDB;

[UnityEngine.Scripting.Preserve]
public class DBMake : IGameDBHelper
{
    static Dictionary<E_MakeType, Dictionary<E_MakeTapType, List<Make_Table>>> makeTypeDic = new Dictionary<E_MakeType, Dictionary<E_MakeTapType, List<Make_Table>>>();

    static Dictionary<uint, List<Make_Table>> usingMatDic = new Dictionary<uint, List<Make_Table>>();
    static Dictionary<uint, List<Make_Table>> resultItem_makeDic = new Dictionary<uint, List<Make_Table>>();

    static List<Make_Table> useList = new List<Make_Table>();

    public void OnReadyData()
    {
        makeTypeDic.Clear();
        resultItem_makeDic.Clear();
        usingMatDic.Clear();
        useList.Clear();

        foreach (Make_Table makeData in GameDBManager.Container.Make_Table_data.Values)
        {
            if (makeData.UnusedType == E_UnusedType.Unuse)
                continue;

            useList.Add(makeData);

            if (!makeTypeDic.ContainsKey(makeData.MakeType))
                makeTypeDic.Add(makeData.MakeType,new Dictionary<E_MakeTapType, List<Make_Table>>());

            if (!makeTypeDic[makeData.MakeType].ContainsKey(makeData.MakeTapType))
                makeTypeDic[makeData.MakeType].Add(makeData.MakeTapType, new List<Make_Table>());

            makeTypeDic[makeData.MakeType][makeData.MakeTapType].Add(makeData);

            /*foreach (var itemTid in makeData.SuccessGetItemID)
            {
                resultItem_makeDic.Add(itemTid, makeData);
            }*/
            if(!resultItem_makeDic.ContainsKey(makeData.SuccessGetItemID))
                resultItem_makeDic.Add(makeData.SuccessGetItemID, new List<Make_Table>());
            resultItem_makeDic[makeData.SuccessGetItemID].Add(makeData);

            CheckMaterial(makeData.MaterialItemID_01, makeData);
            CheckMaterial(makeData.MaterialItemID_02, makeData);
            CheckMaterial(makeData.MaterialItemID_03, makeData);
            CheckMaterial(makeData.MaterialItemID_04, makeData);
            CheckMaterial(makeData.MaterialItemID_05, makeData);
        }
    }

    static void CheckMaterial(List<uint> matidList,Make_Table tableData)
    {
        for (int i = 0; i < matidList.Count; i++)
        {
            if (matidList[i] == 0)
                continue;

            if (!usingMatDic.ContainsKey(matidList[i]))
                usingMatDic.Add(matidList[i],new List<Make_Table>());

            usingMatDic[matidList[i]].Add(tableData);
        }
    }

    public static bool IsMakable(uint ItemTid)
    {
        return resultItem_makeDic.ContainsKey(ItemTid);
    }

    public static List<Make_Table> GetMakeDataByResultItem(uint ItemTid)
    {
        if(resultItem_makeDic.ContainsKey(ItemTid))
            return resultItem_makeDic[ItemTid];

        //ZLog.LogError("GetMakeDataByResultItem - Can't Find MakeData : resultItemTid - "+ItemTid);
        return null;
    }

    public static Dictionary<uint,Make_Table>.ValueCollection GetMakeDatas()
    {
        return GameDBManager.Container.Make_Table_data.Values;
    }

    public static Dictionary<E_MakeType, Dictionary<E_MakeTapType, List<Make_Table>>>.KeyCollection GetMakeTypes()
    {
        return makeTypeDic.Keys;
    }

    public static Dictionary<E_MakeTapType, List<Make_Table>>.KeyCollection GetMakeTabTypes(E_MakeType type)
    {
        if (makeTypeDic.ContainsKey(type))
            return makeTypeDic[type].Keys;

        //ZLog.LogError("GetMakeTabTypes - Can't Find MakeType : " + type);
        return null;
    }

    public static IList<Make_Table> GetMakeTypeDatas(E_MakeType type, E_MakeTapType tabType)
    {
        if (makeTypeDic.ContainsKey(type) && makeTypeDic[type].ContainsKey(tabType))
        {
            return makeTypeDic[type][tabType].AsReadOnly();
        }

       // ZLog.LogError("GetMakeTypeDatas - Can't Find MakeType : " + type+" , ItemType : "+ tabType);
        return null;
    }

    public static Make_Table GetMakeData(uint makeTid)
    {
        if (GameDBManager.Container.Make_Table_data.ContainsKey(makeTid))
            return GameDBManager.Container.Make_Table_data[makeTid];

       // ZLog.LogError("GetMakeData - Can't Find MakeData : "+makeTid);
        return null;
    }

    //public static string GetMakeName(uint makeTid)
    //{
    //    if (GameDBManager.Container.Make_Table_data.ContainsKey(makeTid))
    //    {
    //        var tableData = GameDBManager.Container.Make_Table_data[makeTid];

    //        return string.Format("{0}{1}",DBItem.GetItemFullName(tableData.SuccessGetItemID), tableData.SuccessGetItemCount > 1?string.Format("[{0}개]",MathHelper.CountString(tableData.SuccessGetItemCount)) :"");
    //    }

    //    //ZLog.LogError("GetMakeName - Can't Find MakeData : " + makeTid);
    //    return "";
    //}

    public static uint GetMakePositionNumber(uint makeTid)
    {
        if (GameDBManager.Container.Make_Table_data.ContainsKey(makeTid))
            return GameDBManager.Container.Make_Table_data[makeTid].PositionNumber;

        //ZLog.LogError("GetMakePositionNumber - Can't Find MakeData : " + makeTid);
        return 0;
    }

    public static uint GetMakeResultItemTid(uint makeTid)
    {
        if (GameDBManager.Container.Make_Table_data.ContainsKey(makeTid))
            return GameDBManager.Container.Make_Table_data[makeTid].SuccessGetItemID;

        //ZLog.LogError("GetMakeResultItemTid - Can't Find MakeData : " + makeTid);
        return 0;
    }

    public static uint GetMakeResultItemCnt(uint makeTid)
    {
        if (GameDBManager.Container.Make_Table_data.ContainsKey(makeTid))
            return GameDBManager.Container.Make_Table_data[makeTid].SuccessGetItemCount;

        //ZLog.LogError("GetMakeResultItemCnt - Can't Find MakeData : " + makeTid);
        return 0;
    }

    public static bool IsUsingMakeMaterial(uint MatTid)
    {
        return usingMatDic.ContainsKey(MatTid);
    }

    public static List<Make_Table> GetUsingMakesMaterial(uint MatTid)
    {
        return usingMatDic[MatTid];
    }

    public static List<Make_Table> GetUseList()
    {
        return useList;
    }
}
