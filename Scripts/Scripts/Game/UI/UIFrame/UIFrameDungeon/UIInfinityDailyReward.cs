using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInfinityDailyReward : MonoBehaviour
{
    [SerializeField] private ZText TitleText;
    [SerializeField] private ZText DescriptionText;
    [SerializeField] private ZText ReceiveButtonText;
    [SerializeField] private ZText TodayRewardTitleText;
    [SerializeField] private ZText TodayRewardItemNameText;
    [SerializeField] private ZImage TodayRewardIcon;
    [SerializeField] private ZImage TodayRewardIconCheck;
    [SerializeField] private ZButton ReceiveButton;
    [SerializeField] private GameObject RewardItemParent;
    [SerializeField] private UIInfinityTowerInfo TowerInfo;
    
    private List<InfinityDungeon_Table> DungeonTable = new List<InfinityDungeon_Table>();
    private List<UIInfinityRewardItemList> ItemList = new List<UIInfinityRewardItemList>();
    
    public void Initialize(List<InfinityDungeon_Table> tableList)
    {
        DungeonTable = tableList;

        InitString();
        InitDailyRewardItemSlot();

        InitDailyRewardList();
        UpdateUI();
    }

    private void InitString()
    {
        TitleText.text = DBLocale.GetText("Infinity_DailyReward_Name_01");
        DescriptionText.text = DBLocale.GetText("InfinityDungeon_Rule");
        ReceiveButtonText.text = DBLocale.GetText("Infinity_DailyReward_Get_Btn");
        TodayRewardTitleText.text = DBLocale.GetText("Infinity_DailyReward_Name_01");
    }

    private void InitDailyRewardItemSlot()
    {
        InfinityDungeon_Table CurrentTable;

        CurrentTable = DungeonTable.Find(a => a.DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId);

        if (DBItem.GetItem(DungeonTable[0].DayRewardItemID[0], out Item_Table table))
        {
            TodayRewardItemNameText.text = DBLocale.GetText(table.ItemTextID) + " " + (CurrentTable?.DayRewardItemIDCnt[0] ?? 0).ToString();
        }

        TodayRewardIcon.sprite = UICommon.GetItemIconSprite(DungeonTable[0].DayRewardItemID[0]);
    }

    private void UpdateUI()
    {
        if (TimeHelper.IsGivenDtToday(ZNet.Data.Me.CurUserData.InfinityDungeonRewardTime, 5))
        {
            TodayRewardIconCheck.gameObject.SetActive(true);
            ReceiveButton.interactable = false;
        }
        else
        {
            TodayRewardIconCheck.gameObject.SetActive(false);

            if (ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId == 0 || ZNet.Data.Me.CurUserData.InfinityDungeonScheduleId != ZNet.Data.Me.CurCharData.InfinityTowerContainer.CurrentSchedule.InfinityScheduleID)
            {
                ReceiveButton.interactable = false;
            }
            else
            {
                ReceiveButton.interactable = true;
            }
        }

        TowerInfo.DailyRewardScrollAdapter.UpdateScrollItem();
        //for (int i = 0; i < ItemList.Count; i++)
        //{
        //    ItemList[i].DailyRewardedCheck();
        //}
    }

    private void InitDailyRewardList()
    {
        TowerInfo.DailyRewardScrollAdapter.Refresh(DungeonTable);
        TowerInfo.DailyRewardScrollAdapter.SetPosition();
    }

    public void ClickReceiveDailyReward()
    {
        ZWebManager.Instance.WebGame.REQ_InfinityDungeonDailyReward(delegate
        {
            ZNet.Data.Me.CurCharData.InfinityTowerContainer.LastDailyRewardedDungeonId = ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId;
            UpdateUI();
            TowerInfo.DailyRewardCheck();
        });
    }

    public void CloseDailyRewardPopup()
    {
        this.gameObject.SetActive(false);
    }
}
