using UnityEngine;

public class UITempleRewardListItem : MonoBehaviour
{
    //uint _ObjectTableId;
    public ZImage gradeBoardBG;
    public ZImage itemIcon;
    public GameObject clearEffect;
    bool _isOpen;
    public void DoInit(uint gachaGroupId, bool isOpen)
    {
        _isOpen = isOpen;
        //_ObjectTableId = ObjectTableId;
        gameObject.SetActive(true);
        itemIcon.gameObject.SetActive(true);
        DoUpdate();
    }

    public void DoUpdate()
    {
        //if (DBObject.TryGet( _ObjectTableId, out var objectTable))
        //{
        //    var openString = _isOpen == true ? "open" : "close";
        //    var iconString = $"{objectTable.Icon}_{openString}";
        //    itemIcon.sprite = ZManagerUIPreset.Instance.GetUIManagerSpriteFromAtlas(iconString);
        //}
    }

    public void UITempleRewardListItemClick()
    {
        ZLog.Log(ZLogChannel.Default, "UITempleRewardListItemClick");

    }
}
