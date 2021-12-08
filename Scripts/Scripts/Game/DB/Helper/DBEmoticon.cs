using GameDB;
using System.Collections.Generic;

[UnityEngine.Scripting.Preserve]
public class DBEmoticon : IGameDBHelper
{
    private static Dictionary<string, string> dicEmoticon = new Dictionary<string, string>();

    public void OnReadyData()
    {
        dicEmoticon.Clear();

        foreach(var iter in GameDBManager.Container.Emoticon_Table_data.Values)
        {
            var parseStr = DBLocale.GetText(iter.EmoticonTextID);
            if (!dicEmoticon.ContainsKey(parseStr))
                dicEmoticon.Add(parseStr, iter.EmoticonFile);
        }
    }

    public static bool Get(uint tid, out Emoticon_Table table)
    {
        return GameDBManager.Container.Emoticon_Table_data.TryGetValue(tid,out table);
    }

    // 채팅에서 넘어온 로케일아이디 -> 이모티콘파일 이름
    public static string GetEmoticonIcon(string textID)
    {
        return dicEmoticon[textID];
    }

    public static bool IsEmoticonMsg(string msg)
    {
        return dicEmoticon.ContainsKey(msg);
    }
}
