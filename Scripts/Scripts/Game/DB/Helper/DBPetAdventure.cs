using GameDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZNet.Data;

[UnityEngine.Scripting.Preserve]
class DBPetAdventure : IGameDBHelper
{
    private static List<PetAdventure_Table> listPetAdventure = new List<PetAdventure_Table>();

    public void OnReadyData()
    {
        listPetAdventure.Clear();

        foreach (var iter in GameDBManager.Container.PetAdventure_Table_data.Values)
        {
            listPetAdventure.Add(iter);
        }

        listPetAdventure.Sort((x, y) => x.AdventureID.CompareTo(y.AdventureID));
    }

    public static List<PetAdventure_Table> GetList() => listPetAdventure;

    public static bool Get(uint adventureTid, out PetAdventure_Table table)
    {
        return GameDBManager.Container.PetAdventure_Table_data.TryGetValue(adventureTid, out table);
    }

    public static List<PetAdventureData> GetPossibleAdventurePetList()
    {
        List<PetAdventureData> listPetData = new List<PetAdventureData>();

        foreach (var iter in ZNet.Data.Me.CurCharData.GetPetDataList())
        {
            if (iter.PetTid == ZNet.Data.Me.CurCharData.MainPet)
                continue;

            if (iter.AdvId > 0)
                continue;

            if (DBPet.TryGet(iter.PetTid, out var table) == false)
                continue;

            listPetData.Add(new PetAdventureData(iter.PetTid, iter.Exp, table.Grade, GetPetAdventurePower(iter)));
        }

        listPetData.Sort((x, y) =>
        {
            if (x.adventurePower > y.adventurePower)
                return -1;

            if (x.adventurePower < y.adventurePower)
                return 1;

            if (x.grade > y.grade)
                return -1;

            if (x.grade < y.grade)
                return 1;

            if (x.exp > y.exp)
                return -1;

            if (x.exp < y.exp)
                return 1;

            if (x.tid > y.tid)
                return -1;
            else
                return 1;
        });

        return listPetData;
    }

    public static uint GetPetAdventurePower(uint tid)
    {
        var petData = Me.CurCharData.GetPetData(tid);

        return GetPetAdventurePower(petData);
    }

    public static uint GetPetAdventurePower(ZDefine.PetData data)
    {
        uint power = 0;

        if (data == null)
            return 0;

        if (DBPet.TryGet(data.PetTid, out var table) == false)
            return 0;

        // 펫파워
        power = (uint)(table.Grade * 40) + (uint)(DBPetLevel.GetLevel(table.PetExpGroup, data.Exp) * 10);

        var listEquip = Me.CurCharData.GetEquipRuneList(data.PetTid);

        foreach (var iter in listEquip)
        {
            if (DBItem.GetItem(iter.RuneTid, out var equip) == false)
                continue;

            if (DBRune.GetRuneEnchantTable(iter.BaseEnchantTid, out var enchant) == false)
                continue;

            power += (uint)(equip.Grade * 10) + (uint)(enchant.EnchantStep * 8);
        }

        return power;
    }
}

// 펫 모험파워 포함된 데이터
public struct PetAdventureData
{
    public uint tid;
    public byte grade;
    public ulong exp;
    public uint adventurePower;// 모험능력

    public PetAdventureData(uint _tid, ulong _exp, byte _grade, uint _power)
    {
        tid = _tid;
        exp = _exp;
        grade = _grade;
        adventurePower = _power;
    }
}
