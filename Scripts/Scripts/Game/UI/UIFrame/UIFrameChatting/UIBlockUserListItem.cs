using Com.TheFallenGames.OSA.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class BlockUserHolder : BaseItemViewsHolder
{
    private UIBlockUserListItem item;

    public override void CollectViews()
    {
        item = root.GetComponent<UIBlockUserListItem>();
        base.CollectViews();
    }

    public void SetSlotItem(ScrollBlockUserData data, Action<ScrollBlockUserData> _onClickSlot, Action<BlockCharacterData> _onClickUnblock)
    {
        item.SetSlot(data, _onClickSlot, _onClickUnblock);
    }
}

public class UIBlockUserListItem : MonoBehaviour
{
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtUserName;

    [SerializeField] private List<GameObject> listSelect;

    private ScrollBlockUserData data;

    private Action<ScrollBlockUserData> onClickSlot;
    private Action<BlockCharacterData> onClickUnblock;

    public void SetSlot(ScrollBlockUserData _data,Action<ScrollBlockUserData> _onClickSlot, Action<BlockCharacterData> _onClickUnblock )
    {
        onClickSlot = _onClickSlot;
        onClickUnblock = _onClickUnblock;

        data = _data;

        foreach(var iter in listSelect)
        {
            iter.SetActive(data.isSelected);
        }
        
        imgIcon.sprite = ZManagerUIPreset.Instance.GetSprite(DBCharacter.Get(data.blockCharData.CharTid).Icon);
        txtUserName.text = data.blockCharData.Nick;
    }

    public void OnClickSlot()
    {
        onClickSlot?.Invoke(data);
    }

    public void OnClickUnblock()
    {
        onClickUnblock?.Invoke(data.blockCharData);
    }
}
