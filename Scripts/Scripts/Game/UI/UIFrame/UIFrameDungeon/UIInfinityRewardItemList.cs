using GameDB;
using UnityEngine;

public class UIInfinityRewardItemList : MonoBehaviour
{
    [SerializeField] private GameObject BackEffect;
    [SerializeField] private GameObject ActiveLine;
    [SerializeField] private GameObject LineGradient;
    [SerializeField] private GameObject RewardCheck;
    [SerializeField] private CanvasGroup RewardCanvasGroup;
    [SerializeField] private ZImage RewardIcon;
    [SerializeField] private ZImage FloorIcon;
    [SerializeField] private ZText ItemCount;
    [SerializeField] private ZText FloorText;

    private uint CurrentFloor = 1;
    private InfinityDungeon_Table DungeonTable = null;

    public void SetData(InfinityDungeon_Table table)
    {
        gameObject.SetActive(true);

        this.DungeonTable = table;

        RewardIcon.sprite = UICommon.GetItemIconSprite(DungeonTable.DayRewardItemID[0]);
        ItemCount.text = DungeonTable.DayRewardItemIDCnt[0].ToString();
        FloorText.text = string.Format(DBLocale.GetText("Infinity_DailyReward_Name_02", DungeonTable.StageLevel.ToString()));

        InfinityDungeon_Table current = ZNet.Data.Me.CurCharData.InfinityTowerContainer.DungeonStageList.Find(a => a.DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId);

        if(current == null)
        {
            CurrentFloor = 1;
        }
        else
        {
            CurrentFloor = current.StageLevel;
        }

        ActiveLine.SetActive(DungeonTable.StageLevel < CurrentFloor);
        LineGradient.SetActive(DungeonTable.StageLevel == CurrentFloor - 1);

        if(DungeonTable.StageLevel <= CurrentFloor)
        {
            FloorIcon.color = Color.white;
        }
        else
        {
            FloorIcon.color = new Color32(83, 85, 95, 255);
        }

        if(DungeonTable.StageLevel == CurrentFloor)
        {
            RewardCanvasGroup.alpha = 1.0f;
            BackEffect.SetActive(true);
        }
        else
        {
            RewardCanvasGroup.alpha = 0.25f;
            BackEffect.SetActive(false);
        }

        DailyRewardedCheck();
    }

    public void DailyRewardedCheck()
    {
        bool isRewarded = false;

        float rewardedLevel = 0;
        ulong rewardedTime = ZNet.Data.Me.CurUserData.InfinityDungeonRewardTime;

        if (TimeHelper.IsGivenDtToday(rewardedTime, 5))
		{
            foreach (var table in GameDBManager.Container.InfinityDungeon_Table_data.Values)
            {
                if (table.DungeonID == ZNet.Data.Me.CurUserData.LastRewardedStageTid)
                {
                    rewardedLevel = table.StageLevel;
                    break;
                }
            }

            if(DungeonTable.StageLevel <= rewardedLevel)
			{
                isRewarded = true;
			}
        }

        RewardCheck.SetActive(isRewarded);
    }
}
