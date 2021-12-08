using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UISubChatSetting : MonoBehaviour
{
    [Serializable]
    public class CFilterToggle
    {
        public ChatFilter filter;
        public ZToggle toggle;

        [HideInInspector]
        public bool state;// 상태

        public void SetFilterState(bool _state)
        {
            state = _state;
            toggle.SetIsOnWithoutNotify(state);
        }
    }

    [SerializeField] private List<CFilterToggle> listFilterToggle;

    // 길드 인사말
    [SerializeField] private Text guildGreeting;

    private Dictionary<ChatFilter, CFilterToggle> dicFilter = new Dictionary<ChatFilter, CFilterToggle>();

    public void Initialize()
    {
        foreach (var iter in listFilterToggle)
        {
            dicFilter.Add(iter.filter, iter);
        }
    }

    public void Open()
    {
        OptionInfo info = Me.CurCharData.GetOptionInfo(WebNet.E_CharacterOptionKey.GUILD_GREETING);

        if(info==null)
        {
            guildGreeting.text = DBLocale.GetText("WChat_Macro_Default1");
        }
        else
        {
            guildGreeting.text = info.OptionValue;
        }

        RefreshFilter();

        gameObject.SetActive(true);
    }

    private void RefreshFilter()
    {
        ChatFilter myFilter = Me.CurCharData.chatFilter;

        foreach (var iter in listFilterToggle)
        {
            iter.SetFilterState(EnumHelper.CheckFlag(myFilter, iter.filter));
        }
    }

    private void RefreshGuildGreeting()
    {

    }

    public void OnClickGuildGreeting()
    {
        UIMessagePopup.ShowInputPopup(DBLocale.GetText("Chatting_Guild_Greeting"), DBLocale.GetText("Chatting_Guild_Greeting_Desc"), (targetName) =>
        {
            // 같다면 패수~
            if (guildGreeting.text.Equals(targetName))
                return;

            // 글자 길이
            if (targetName.Length > DBConfig.Chatting_GuildGreeting_Char_Limit)
            {
                UIMessagePopup.ShowPopupOk(string.Format(DBLocale.GetText("Chatting_Error_Message_ToLong"), DBConfig.Chatting_GuildGreeting_Char_Limit));
                return;
            }

            ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.GUILD_GREETING, targetName, (recvPacket, recvMsgPacket) =>
            {
                guildGreeting.text = Me.CurCharData.GetOptionInfo(WebNet.E_CharacterOptionKey.GUILD_GREETING).OptionValue;
            });
        });
    }

    public void OnFilterToggleChanged(int filter)
    {
        if (dicFilter.TryGetValue((ChatFilter)filter, out CFilterToggle toggle) == false)
            return;

        toggle.SetFilterState(toggle.toggle.isOn);
    }

    private void SaveResultFilter()
    {
        ChatFilter filter = ChatFilter.TYPE_NONE;

        foreach (var iter in listFilterToggle)
        {
            if (iter.state)
                filter |= iter.filter;
        }

        ZWebManager.Instance.WebGame.REQ_SetCharacterOption(WebNet.E_CharacterOptionKey.CHAT_CHANNEL,
                                                       ((int)filter).ToString(),
                                                       (recvPacket, recvMsgPacket) =>
                                                       {
                                                           OnClickClose();
                                                       });
    }

    public void OnClickConfirm()
    {
        SaveResultFilter();
    }

    public void OnClickClose()
    {
        gameObject.SetActive(false);
    }
}
