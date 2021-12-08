using GameDB;
using UnityEngine;

public class UITempleClearListItem : MonoBehaviour
{
    private uint _rewardID;

    public GameObject checkBox;
    public ZImage gradeBoardBG;
    public ZImage itemIcon;
    public ZText itemCount;
    public GameObject RewardWhere;
    public ZText RewardWhereText;
    public GameObject clearEffect;

    private uint _rewardCount;

    public void DoInit(uint rewardID, uint rewardCount )
    {
        _rewardID = rewardID;
        _rewardCount = rewardCount;
        gameObject.SetActive(true);
        checkBox.SetActive(false);
        RewardWhere.SetActive(false);
        itemCount.gameObject.SetActive(true);
    }

    public void DoUpdate(ulong clearDts)
    {
        Item_Table itemTable =  DBItem.GetItem(_rewardID);
        ZLog.Log(ZLogChannel.Default, $"{itemTable.IconID}");
        itemIcon.sprite = ZManagerUIPreset.Instance.GetSprite(itemTable.IconID);
        itemCount.text = $"{_rewardCount}";
        checkBox.SetActive(clearDts > 0);
    }

    public void UITempleRewardListItemClick()
    {
        ZLog.Log(ZLogChannel.Default, "UITempleRewardListItemClick");
    }
}
