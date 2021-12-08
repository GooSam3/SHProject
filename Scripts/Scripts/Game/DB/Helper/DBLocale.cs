using GameDB;
using System.Collections.Generic;
using UnityEngine;

[UnityEngine.Scripting.Preserve]
public class DBLocale : IGameDBHelper
{
    private const string GRADE_COLOR_FORMAT = "<color={0}>{1}</color>";
    private const string GRADE_FORMAT = "Item_Tier{0}_Text";
    private const string GRADE_COLOR_DEFAULT = "#FFFFFF";

    public static Dictionary<string, GameDB.Locale_Table> LocaleData
    {
        get { return defaultData; }
    }
    static Dictionary<string, GameDB.Locale_Table> defaultData = new Dictionary<string, GameDB.Locale_Table>();

    public static void DefaultLoad()
    {
        try
        {
            List<string> cachePath = new List<string>();
            Caching.GetAllCachePaths(cachePath);
            if (cachePath.Count > 0 && System.IO.File.Exists(cachePath[0] + "/bin/Locale_Table.bin"))
            {
                var readBytes = System.IO.File.ReadAllBytes(cachePath[0] + "/bin/Locale_Table.bin");

                defaultData = GameDB.Locale_Table.Deserialize(ref readBytes);

                ZLog.LogWarn( ZLogChannel.Default, "Load Caching Folder : "+ cachePath[0] +" , " + +defaultData.Count);
            }
            else
            {
                var readBytes = ((TextAsset)Resources.Load("GameDB/Locale_Table", typeof(TextAsset))).bytes;

                defaultData = GameDB.Locale_Table.Deserialize(ref readBytes);

                ZLog.LogWarn(ZLogChannel.Default, "Load Resource Folder : " + defaultData.Count);
            }
        }
        catch
        {
            var readBytes = ((TextAsset)Resources.Load("GameDB/Locale_Table", typeof(TextAsset))).bytes;

            defaultData = GameDB.Locale_Table.Deserialize(ref readBytes);

            ZLog.LogWarn(ZLogChannel.Default, "Load Resource Folder : " + defaultData.Count);
        }
    }

    public void OnReadyData()
    {
        defaultData = GameDBManager.Container.Locale_Table_data;
    }

    public static bool TryGet(string _tid, out Locale_Table outTable)
    {
        return LocaleData.TryGetValue(_tid, out outTable);
    }

    public static Locale_Table Get(string _tid)
    {
        if (LocaleData.TryGetValue(_tid, out var foundTable))
        {
            return foundTable;
        }

        return null;
    }

    /// <summary> 문자열 키를 받아서 해당하는 Locale을 반환해준다. </summary>
    public static string GetText(string _tid, params object[] parameters)
    {
        if (!string.IsNullOrEmpty(_tid) && LocaleData.ContainsKey(_tid))
        {
            if (parameters != null && parameters.Length > 0)
                return string.Format(LocaleData[_tid].Text.Replace("\\n", "\n"), parameters);
            else
                return LocaleData[_tid].Text.Replace("\\n", "\n");
        }

        return _tid;
    }

    public static string GetTextSimple(string _tid)
	{
        string simpleText = _tid;
        if (LocaleData.ContainsKey(_tid))
		{
            simpleText = LocaleData[_tid].Text;
		}
        return simpleText;
	}

    public static string GetItemTypeText(E_ItemType type)
    {
        return GetText(string.Format($"ItemType_{type.ToString()}"));
    }

    public static string ParseAbilityTooltip(GameDB.AbilityAction_Table tableData, string showKey = "", string hideKey = "")
    {
        if (string.IsNullOrEmpty(tableData.ToolTip))
            return tableData.ToolTip;

        //parse rule
        List<string> parseList = new List<string>();
        parseList.Add(tableData.AbilityRate.ToString());
        parseList.Add(GetText(tableData.NameText));
        if (tableData.AbilityID_01 == GameDB.E_AbilityType.LINK_ABILITY_BUFF)
        {
            var linkData = DBAbility.GetAction(tableData.LinkAbilityActionID);

            parseList.Add(linkData.AbilityPoint_01_Min.ToString("0"));
            parseList.Add(linkData.AbilityPoint_01_Max.ToString("0"));

            string parseStr = "";
            foreach (var type in EnumHelper.Values<GameDB.E_InvokeTimingType>())
            {
                if (type == GameDB.E_InvokeTimingType.None)
                    continue;

                if (linkData.InvokeTimingType.HasFlag(type))
                {
                    if (parseStr.Length > 0)
                        parseStr += ",";

                    parseStr += DBLocale.GetText(string.Format("AbilityAction_{0}", type.ToString()));
                }
            }
            parseList.Add(parseStr);

            parseList.Add(DBLocale.GetText(DBAbility.GetAbilityName(linkData.AbilityID_01)));
            parseList.Add(linkData.AbilityRate.ToString());
        }
        else
        {
            parseList.Add("");
            parseList.Add("");
            parseList.Add("");
            parseList.Add("");
            parseList.Add("");
        }
        //ZLog.LogWarning(GetLocaleText(tableData.ToolTip));
        string Origin = string.Format(GetText(tableData.ToolTip), parseList.ToArray());

        string StartKeyFormat = "[${0}]";
        string EndKeyFormat = "[/${0}]";

        //foreach (string showKey in ShowKeys)
        if (!string.IsNullOrEmpty(showKey))
        {
            Origin = Origin.Replace(string.Format(StartKeyFormat, showKey), "");
            Origin = Origin.Replace(string.Format(EndKeyFormat, showKey), "");
        }


        //foreach (string showKey in HideKeys)
        if (!string.IsNullOrEmpty(hideKey))
        {
            int StartIndex = Origin.IndexOf(string.Format(StartKeyFormat, hideKey));
            if (StartIndex >= 0)
            {
                string endKey = string.Format(EndKeyFormat, hideKey);
                int EndIndex = Origin.IndexOf(endKey);
                if (EndIndex >= 0)
                {
                    Origin = Origin.Remove(StartIndex, (EndIndex + endKey.Length) - StartIndex);
                }
            }
        }

        return Origin;
    }

    public static string GetItemLocale(Item_Table itemTable)
    {
        uint enchantCnt = itemTable.Step;
        string enchant = enchantCnt <= 0 ? "" : $"+{enchantCnt}";

        string enchantedName = $"{Get(itemTable.ItemTextID).Text}{enchant}";

        return DBUIResouce.GetItemGradeFormat(enchantedName, itemTable.Grade);
    }

    public static string GetSkillLocale(Skill_Table skillTable)
    {
        return DBUIResouce.GetSkillGradeFormat(Get(skillTable.SkillTextID).Text, skillTable.Grade);
    }

    public static string GetStageName(uint stageTID)
    {
        if (DBStage.TryGet(stageTID, out Stage_Table table) == false)
        {
            ZLog.LogError(ZLogChannel.System, $"Can't Find StageTable TID : {stageTID}");
            return string.Empty;
        }

        return GetText(table.StageTextID);
    }

    public static string GetGradeLocale(uint grade)
    {
        return GetText(string.Format(GRADE_FORMAT, grade.ToString("D2")));
    }

    public static string GetRuneAbilityTypeName(E_RuneAbilityViewType type)
    {
        switch (type)
        {
            case E_RuneAbilityViewType.None:
                return DBLocale.GetText("TYPE_TOTAL");
            default:
                {
                    string localeKey = type.ToString();
                    string locale = GetText(localeKey);

                    if (localeKey.Contains("_PER"))
                    {
                        locale += "%";
                    }
                    return locale;
                }
        }
    }

    public static string GetItemUseCharacterTypeName(uint ItemTid)
    {
        if (!GameDBManager.Container.Item_Table_data.ContainsKey(ItemTid))
        {
            //ZLog.LogError("GetItemCharacterTypeName - Can't Find itemidx : " + ItemTid);
            return "";
        }

        return GetCharacterTypeName(GameDBManager.Container.Item_Table_data[ItemTid].UseCharacterType);
    }

    public static string GetCharacterTypeName(GameDB.E_CharacterType charType)
    {
        switch (charType)
        {
            case GameDB.E_CharacterType.All:
                return DBLocale.GetText("TYPE_TOTAL");
            case GameDB.E_CharacterType.Knight:
                return DBLocale.GetText("Character_Knight");
            case GameDB.E_CharacterType.Archer:
                return DBLocale.GetText("Character_Archer");
            case GameDB.E_CharacterType.Wizard:
                return DBLocale.GetText("Character_Wizard");
            case GameDB.E_CharacterType.Assassin:
                return DBLocale.GetText("Character_Assassin");
            case GameDB.E_CharacterType.None:
                return "없음";
        }

        return "";
    }
}
