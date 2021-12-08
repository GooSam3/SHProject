using Com.TheFallenGames.OSA.Core;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ChatDataHolder : BaseItemViewsHolder
{
    private UIChattingListItem item;

    public ContentSizeFitter CSF { get; set; }

    public override void CollectViews()
    {
        item = root.GetComponent<UIChattingListItem>();
        CSF = root.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        CSF.enabled = false;

        base.CollectViews();
    }

    public override void MarkForRebuild()
    {
        base.MarkForRebuild();
        if (CSF)
            CSF.enabled = true;
    }

    public override void UnmarkForRebuild()
    {
        if (CSF)
            CSF.enabled = false;
        base.UnmarkForRebuild();
    }

    public void SetSlotItem(ScrollChatData data, Action<ScrollChatData> _onClick)
    {
        item.SetSlot(data, _onClick);
    }
}


/// <summary>
/// <color=#ffffff>USERNAME</color> : HelloWorld!
/// </summary>
public class UIChattingListItem : MonoBehaviour
{
    [SerializeField] private Text txtChatting;

    private Action<ScrollChatData> onClick;

    private ScrollChatData data;

    public void SetSlot(ScrollChatData _data, Action<ScrollChatData> _onClick)
    {
        onClick = _onClick;
        data = _data;

        txtChatting.raycastTarget = !_data.isHudMode;
        txtChatting.text = _data.chatData.Message;
        txtChatting.color = ZWebChatData.GetChatViewColor(_data.chatData.type);
    }

    public void OnClickChatSlot()
    {
        onClick?.Invoke(data);
    }
}
