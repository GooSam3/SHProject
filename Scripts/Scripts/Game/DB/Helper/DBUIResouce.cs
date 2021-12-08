using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBUIResouce : IGameDBHelper
{
    static Dictionary<E_UIType, Dictionary<byte, UIResource_Table>> TypeDic = new Dictionary<E_UIType, Dictionary<byte, UIResource_Table>>();

    public void OnReadyData()
    {

        TypeDic.Clear();

        foreach (var table in GameDBManager.Container.UIResource_Table_data.Values)
        {
            if (!TypeDic.ContainsKey(table.UIType))
                TypeDic.Add(table.UIType, new Dictionary<byte, UIResource_Table>());

            if (!TypeDic[table.UIType].ContainsKey(table.Tier))
                TypeDic[table.UIType].Add(table.Tier, table);
            else
                TypeDic[table.UIType][table.Tier] = table;
        }
    }

    public static string GetTierText(byte tier)
    {
        if (TypeDic.ContainsKey(E_UIType.Item) == false)
            return "";

        var list = TypeDic[E_UIType.Item];

        if (list.ContainsKey(tier))
            return list[tier].TierText;
        else return "";
    }

    public static string GetBGByTier(byte tier)
    {
        if (TypeDic.ContainsKey(E_UIType.Item) == false)
            return "";

        var list = TypeDic[E_UIType.Item];

        if (list.ContainsKey(tier))
            return list[tier].BgImg;
        else return "";
    }

    public static string GetBGByTier(E_UIType type, byte tier)
    {
        if (TypeDic.ContainsKey(type) == false)
            return "";

        var list = TypeDic[type];

        if (list.ContainsKey(tier))
            return list[tier].BgImg;
        else return "";
    }

    public static UnityEngine.Color GetGradeColor(E_UIType type, byte tier)
    {
        if (TypeDic.TryGetValue(type, out var loadData) && loadData.TryGetValue(tier, out var tableData) && UnityEngine.ColorUtility.TryParseHtmlString("#"+tableData.TextColor, out var color))
            return color;
        else
            return UnityEngine.Color.white;
    }

    public static string GetGradeTextColor(E_UIType type, byte tier)
    {
        if (TypeDic.TryGetValue(type, out var loadData) && loadData.TryGetValue(tier, out var tableData))
            return tableData.TextColor;
        else
            return "FFFFFF";
    }

    // 강림파견 등 정의되있지 않은 컬러포맷, 임시로 아이템티어와 공유
    public static string GetGradeFormat(string str, byte tier)
	{
        return GetItemGradeFormat(str, tier);
	}

    public static string GetChangeGradeFormat(string str, byte tier)
	{
        string color = GetGradeTextColor(E_UIType.Change, tier);

        return $"<color=#{color}>{str}</color>";
    }

    public static string GetPetGradeFormat(string str, byte tier)
	{
        string color = GetGradeTextColor(E_UIType.Pet, tier);

        return $"<color=#{color}>{str}</color>";
    }

    public static string GetSkillGradeFormat(string str, byte tier)
    {
        string color = GetGradeTextColor(E_UIType.Skill, tier);

        return $"<color=#{color}>{str}</color>";
    }

    public static string GetItemGradeFormat(string str, byte tier)
    {
        string color = GetGradeTextColor(E_UIType.Item, tier);

        return $"<color=#{color}>{str}</color>";
    }
}
