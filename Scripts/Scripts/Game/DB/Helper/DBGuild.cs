using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBGuild : IGameDBHelper
{
    /// <summary>적대 길드 미니 마크 SpriteName</summary>
    //public const string EnemyGuilMark = "Icon_WGuild_Enemy";
    /// <summary>동맹 길드 미니 마크 SpriteName</summary>
    //public const string AllyGuilMark = "Icon_WGuild_Ally";

    public void OnReadyData()
    {

    }

    public static Dictionary<uint, Guild_Table>.ValueCollection GetGuildDatas()
    {
        return GameDBManager.Container.Guild_Table_data.Values;
    }

    public static Dictionary<byte, GuildMark_Table>.ValueCollection GetGuildMarkData()
    {
        return GameDBManager.Container.GuildMark_Table_data.Values;
    }

    public static Guild_Table GetGuildDataByLevel(uint level)
    {
        foreach (var data in GetGuildDatas())
        {
            if (data.GuildLevel == level)
                return data;
        }

        return null;
    }

    public static GuildMark_Table GetGuildMarkFirstData()
    {
        GuildMark_Table result = null;

        foreach (var t in GameDBManager.Container.GuildMark_Table_data)
        {
            result = t.Value;
            break;
        }

        return result;
    }

    public static uint GetGuildMarkOpenLevel(byte tid)
    {
        foreach (var mark in GameDBManager.Container.GuildMark_Table_data)
        {
            if(mark.Value.GuildMarkID == tid)
            {
                return mark.Value.GuildLevel;
            }
        }

        return 0;
    }

    public static string GetGuildMark(byte GuildMarkTid)
    {
        if (GameDBManager.Container.GuildMark_Table_data.ContainsKey(GuildMarkTid))
            return GameDBManager.Container.GuildMark_Table_data[GuildMarkTid].GuildMarkFile;

        //ZLog.LogWarning("GetGuildMark - Can't Find  MarkTid : " + GuildMarkTid);
        return "";
    }

    public static bool IsGuildMarkExist(byte GuildMarkTid)
    {
        return GameDBManager.Container.GuildMark_Table_data.ContainsKey(GuildMarkTid);
    }

    public static string GetGuildSmallMark(byte GuildMarkTid)
    {
        if (GameDBManager.Container.GuildMark_Table_data.ContainsKey(GuildMarkTid))
            return GameDBManager.Container.GuildMark_Table_data[GuildMarkTid].GuildMarkSmallFile;

        //ZLog.LogWarning("GetGuildSmallMark - Can't Find  MarkTid : " + GuildMarkTid);
        return "";
    }

    public static uint GetGuildLevel(ulong Exp)
    {
        Guild_Table returnData = null;
        foreach (var guildLvTable in GameDBManager.Container.Guild_Table_data.Values)
        {
            if (guildLvTable.LevelUpExp <= Exp)
            {
                if (returnData == null)
                    returnData = guildLvTable;
                else if (returnData.LevelUpExp < guildLvTable.LevelUpExp)
                    returnData = guildLvTable;
            }
        }

        return returnData.GuildLevel;
    }

    public static uint GetLevel(ulong exp)
    {
        uint guildLevel = 0;
        foreach (var guildLvTable in GameDBManager.Container.Guild_Table_data.Values)
        {
            if (guildLvTable.LevelUpExp <= exp)
            {
                guildLevel = guildLvTable.GuildLevel;
            }
            else
            {
                // 테이블은 무조건 레벨 순서대로 돌기때문에, 해당 조건으로 오면 그냥 끝.
                break;
            }
        }

        return guildLevel;
    }

    public static Guild_Table GetGuildData(uint Level)
    {
        if (Level == 0)
            return null;

        foreach (var table in GameDBManager.Container.Guild_Table_data.Values)
        {
            if (table.GuildLevel == Level)
                return table;
        }

        return null;
    }

    public static ulong GetGuildLvMaxExp(ulong Exp)
    {
        Guild_Table returnData = null;
        foreach (var guildLvTable in GameDBManager.Container.Guild_Table_data.Values)
        {
            if (guildLvTable.LevelUpExp > Exp)
            {
                if (returnData == null)
                    returnData = guildLvTable;
                else if (returnData.LevelUpExp > guildLvTable.LevelUpExp)
                    returnData = guildLvTable;
            }
        }

        return returnData != null ? returnData.LevelUpExp : 0;
    }

    public static Dictionary<byte, GuildMark_Table>.ValueCollection GetGuildMarks()
    {
        return GameDBManager.Container.GuildMark_Table_data.Values;
    }

    //public static Dictionary<uint, ColoGuildReward_Table>.ValueCollection GetPointRewardDatas()
    //{
    //    return GameDBManager.Container.ColoGuildReward_Table_data.Values;
    //}

    #region Buff 
    public static Dictionary<uint , GuildBuff_Table> GetGuildBuffDic()
    {
        return GameDBManager.Container.GuildBuff_Table_data;
    }

    public static GuildBuff_Table GetGuildBuffData(uint guildBuffID)
    {
        var dic = GameDBManager.Container.GuildBuff_Table_data;
        GuildBuff_Table result = null;

        if (dic != null)
        {
            foreach (var keyPair in dic)
            {
                uint id = keyPair.Key;

                if (id == guildBuffID)
                {
                    result = keyPair.Value;
                    break;
                }
            }
        }

        return result;
    }
    #endregion
}
