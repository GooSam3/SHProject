using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 주신의 탑
/// </summary>
public sealed class UIInfinityTowerInfo : MonoBehaviour, ITabContents
{
    [SerializeField] private ZButton EnterDungeonButton;
    [SerializeField] private ZButton ResetButton;
    [SerializeField] private ZButton AccumulateDailyRewardButton;
    [SerializeField] private ZText EnterDungeonButtonText;
    [SerializeField] private ZText BuffText;
    [SerializeField] private ZText FloorText;
    [SerializeField] private ZText DescriptionText;
    [SerializeField] private ZText ClearRewardText;
    [SerializeField] private ZText DailyRewardText;
    [SerializeField] private ZText ResetButtonText;
    [SerializeField] private UIInfinityDailyReward DailyRewardPopup;
    [SerializeField] private Transform FloorParent;
    [SerializeField] private ZImage DailyRewardIcon;
    [SerializeField] private GameObject DailyRewardedCheck;
    [SerializeField] private ZText DailyRewardItemCount;
    [SerializeField] private GameObject DailyRewardRedDot;
    [SerializeField] private UIInfinityClearRewardItemSlot ItemSlot;
    [SerializeField] private Transform BuffListParent;
    [SerializeField] private UIInfinitySelectBuff SelectBuff;
    [SerializeField] private UIInfinityAccumBuffListPopup AccumBuffListPopup;
    [SerializeField] public UIInfinityTowerDailyRewardScrollAdapter DailyRewardScrollAdapter;
    [SerializeField] private UIInfinityRewardItemList DailyRewardListItem;
    [SerializeField] private UIInfinityFloorScrollAdapter FloorScrollAdapter;
    [SerializeField] private UIInfinityTowerListItem FloorListItem;

    public int Index { get; set; }

    private bool isSpawnLoad = false;
    private Action loadEvent;
    private Stage_Table CurrentStage = null;
    private InfinityDungeon_Table ChallengeDungeonStage = null;
    private List<UIInfinityTowerListItem> FloorList = new List<UIInfinityTowerListItem>();
    private List<AbilityAction_Table> AbilityActionList = new List<AbilityAction_Table>();
    private InfinityTowerContainer Container;
    
    public void Initialize()
    {
        EnterDungeonButtonText.text = DBLocale.GetText( "InfinityDungeon_Enter_Btn" );
        BuffText.text = DBLocale.GetText( "InfiBuff_Buff_Title" );
        DescriptionText.text = DBLocale.GetText( "Infinity_DailyReward_Des" );
        ClearRewardText.text = DBLocale.GetText( "Infinity_ClearReward_Name" );
        DailyRewardText.text = DBLocale.GetText( "Infinity_DailyReward_Name_01" );
        ResetButtonText.text = DBLocale.GetText( "InfinityDungeon_Reset_Btn" );

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIInfinityBuffListItem), delegate
        {
            ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIInfinityAccumBuffListItem), delegate
            {
                isSpawnLoad = true;
                InitScrollAdapter();
                loadEvent?.Invoke();
                loadEvent = null;
            }, 0, 1, false);
        }, 0, 1, false);
    }

    private void InitScrollAdapter()
	{
        DailyRewardScrollAdapter.Parameters.ItemPrefab = DailyRewardListItem.GetComponent<RectTransform>();
        var pf = DailyRewardScrollAdapter.Parameters.ItemPrefab;
        pf.SetParent(transform);
        pf.localScale = Vector2.one;
        pf.localPosition = Vector3.zero;
        pf.gameObject.SetActive(false);
        DailyRewardScrollAdapter.Initialize();

        FloorScrollAdapter.Parameters.ItemPrefab = FloorListItem.GetComponent<RectTransform>();
        var floorPf = FloorScrollAdapter.Parameters.ItemPrefab;
        floorPf.SetParent(transform);
        floorPf.localScale = Vector2.one;
        floorPf.localPosition = Vector3.zero;
        floorPf.gameObject.SetActive(false);
        FloorScrollAdapter.Initialize();
    }

    public void Open()
    {
        Container = ZNet.Data.Me.CurCharData.InfinityTowerContainer;

        if(ZGameModeManager.Instance.CurrentGameModeType == E_GameModeType.Infinity)
        {
            EnterDungeonButton.interactable = false;
            ResetButton.interactable = false;
        }
        else
        {
            EnterDungeonButton.interactable = true;
            ResetButton.interactable = true;
        }

        this.gameObject.SetActive(true);

        Container.DungeonStageList.Clear();

        foreach (var table in GameDBManager.Container.InfinitySchedule_Table_data.Values)
        {
            if(TimeManager.NowMs >= table.Start && TimeManager.NowMs < table.End)
            {
                Container.CurrentSchedule = table;
                break;
            }
        }
        
        if(Container.CurrentSchedule == null)
        {
            ZLog.LogError(ZLogChannel.Default, "해당하는 주신의 탑 테마가 없습니다.");
            return;
        }

        foreach(var table in GameDBManager.Container.InfinityDungeon_Table_data.Values)
        {
            if(table.InfinityDungeonGroupID == Container.CurrentSchedule.InfinityDungeonGroupID)
            {
                Container.DungeonStageList.Add(table);
            }
        }

        CurrentStage = DBStage.Get(Container.DungeonStageList[0].StageID);

        SetCurrentDungeonInfo();
        SetDailyReward();

        if (isSpawnLoad == false)
        {
            loadEvent += Init;
        }
        else
        {
            Init();
        }
    }

    public void Refresh()
	{
	}

    public void Close()
    {
        DailyRewardPopup.CloseDailyRewardPopup();
        CloseClearRewardItemPopup();
        CloseAccumBuffListPopup();
        this.gameObject.SetActive(false);
    }

    private void Init()
    {
        InitString();
        InitFloorInfo();
        InitBuffList();
        InitClearRewardList();
    }

    private void SetCurrentDungeonInfo()
    {
        if (Container.DungeonStageList.Count <= 0)
        {
            ZLog.LogError(ZLogChannel.Default, "스케줄에 맞는 주신의 탑 스테이지가 없습니다.");
            return;
        }
        
        if (ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId == 0 || ZNet.Data.Me.CurUserData.InfinityDungeonScheduleId != Container.CurrentSchedule.InfinityScheduleID)
        {
            Container.CurrentDungeonStage = null;
            ChallengeDungeonStage = Container.DungeonStageList[0];
        }
        else
        {
            foreach (var stage in Container.DungeonStageList)
            {
                if (stage.DungeonID == ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId)
                {
                    Container.CurrentDungeonStage = stage;

                    if (IsCompleteLastStage())
                    {
                        ChallengeDungeonStage = Container.CurrentDungeonStage;
                    }
                    else
                    {
                        ChallengeDungeonStage = Container.DungeonStageList.Find(a => a.StageLevel == Container.CurrentDungeonStage.StageLevel + 1);
                    }
                    break;
                }
            }
        }

        Container.ChallengeDungeonStage = ChallengeDungeonStage;
    }

    private void SetDailyReward()
    {
        DailyRewardIcon.sprite = UICommon.GetItemIconSprite(Container.CurrentDungeonStage?.DayRewardItemID[0] ?? Container.DungeonStageList[0].DayRewardItemID[0]);
        DailyRewardItemCount.text = Container.CurrentDungeonStage?.DayRewardItemIDCnt[0].ToString() ?? "0";

        DailyRewardCheck();
    }

    public void DailyRewardCheck()
    {
        bool isRewarded = TimeHelper.IsGivenDtToday(ZNet.Data.Me.CurUserData.InfinityDungeonRewardTime, 5);

        DailyRewardedCheck.SetActive(isRewarded);
        DailyRewardRedDot.SetActive(Container.CurrentDungeonStage != null && !isRewarded);
    }

    public void InitString()
    {
        FloorText.text = string.Format(DBLocale.GetText("InfinityDungeon_Name_02"), ChallengeDungeonStage.StageLevel);
    }

    private void InitFloorInfo()
    {
        FloorScrollAdapter.Refresh(Container.DungeonStageList);
        FloorScrollAdapter.SetPosition();

        if (Container.CurrentDungeonStage == null)
        {
            ResetButton.interactable = false;
        }
        else if(ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Infinity)
        {
            ResetButton.interactable = true;
        }
    }

    public void InitBuffList()
    {
        AbilityActionList.Clear();

        for (int i = 0; i < BuffListParent.childCount; i++)
        {
            Destroy(BuffListParent.GetChild(i).gameObject);
        }

        ZWebManager.Instance.WebGame.REQ_GetInfinityDungeonBuffList(delegate
        {
            for (int i = 0; i < Container.InfinityBuffList.Count; i++)
            {
                if(DBAbilityAction.TryGet(Container.InfinityBuffList[i].AbilityActionID, out AbilityAction_Table action))
                {
                    AbilityActionList.Add(action);
                }
            }

            for(int i = 0; i < AbilityActionList.Count; i++)
            {
                UIInfinityBuffListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIInfinityBuffListItem)).GetComponent<UIInfinityBuffListItem>();

                obj.transform.SetParent(BuffListParent);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;

                obj.Init(AbilityActionList[i].BuffIconString, this);
            }
        });
    }

    private void InitClearRewardList()
    {
        ItemSlot.Initialize(ChallengeDungeonStage.ClearRewardItemID[0], ChallengeDungeonStage.ClearRewardItemIDCnt[0]);
    }

    public void CloseClearRewardItemPopup()
    {
        ItemSlot?.CloseInfoPopup();
    }

    private void InitButton()
    {
        EnterDungeonButton.gameObject.SetActive(!IsCompleteLastStage());
    }

    public void OpenDailyRewardPopup()
    {
        CloseClearRewardItemPopup();
        DailyRewardPopup.gameObject.SetActive(true);
        DailyRewardPopup.Initialize(Container.DungeonStageList);
    }

    public void ClickEnterDungeonButton()
    {
        bool IsGetBuff = false;
        
        for (int i = 0; i < Container.InfinityBuffList.Count; i++)
        {
            if(ChallengeDungeonStage.InfiBuffGroupID == Container.InfinityBuffList[i].InfiBuffGroupID)
            {
                IsGetBuff = true;
                break;
            }
        }

        if(ChallengeDungeonStage.InfiBuffGroupID <= 0)
        {
            IsGetBuff = true;
        }

        if (!IsGetBuff)
        {
            ZWebManager.Instance.WebGame.REQ_GetInfinityDungeonSelectBuffList((recv, recvPacket) =>
            {
                CloseClearRewardItemPopup();
                SelectBuff.gameObject.SetActive(true);
                SelectBuff.Init(recvPacket);
            });
            return;
        }

        if (ZGameModeManager.Instance.CurrentGameModeType != E_GameModeType.Field)
        {
            UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
            {
                _popup.Open(string.Empty, "현재 진행중인 던전이 초기화됩니다. 입장하시겠습니까?",
                    new string[]
                    {
                    ZUIString.LOCALE_CANCEL_BUTTON,
                    ZUIString.LOCALE_OK_BUTTON
                    },
                    new Action[]
                    {
                    delegate { _popup.Close(); },
                    delegate { EnterDungeon(); _popup.Close(); }
                    });
            });
        }
        else
        {
            EnterDungeon();
        }
    }

    private void EnterDungeon()
    {
        var portalTable = DBPortal.Get(CurrentStage.DefaultPortal);

        if (portalTable == null)
        {
            ZLog.LogError(ZLogChannel.Default, $"PortalTable is null (DefaultPortal : {CurrentStage.DefaultPortal}");
            return;
        }

        AudioManager.Instance.PlaySFX(30004);
        ZGameManager.Instance.TryEnterStage(portalTable.PortalID, false, 0, 0);

        UIManager.Instance.Close<UIFrameDungeon>();
    }

    public void ClickResetButton()
    {
        UIMessagePopup.ShowCostPopup(string.Empty, DBLocale.GetText("InfiBuff_Reset_Des"), DBConfig.Infinity_Dungeon_Reset_ItemID, DBConfig.Infinity_Dungeon_Reset_ItemCnt, FloorReset);
    }

    private void FloorReset()
    {
        ZWebManager.Instance.WebGame.REQ_InfinityDungeonReset(delegate
        {
            ZNet.Data.Me.CurUserData.CurrentInfinityDungeonId = 0;
            Container.CurrentDungeonStage = null;
            Container.ChallengeDungeonStage = ChallengeDungeonStage = Container.DungeonStageList[0];
            FloorScrollAdapter.UpdateScrollItem();
            ResetButton.interactable = false;
            InitBuffList();
            DailyRewardCheck();
            SetDailyReward();
            FloorText.text = string.Format(DBLocale.GetText("InfinityDungeon_Name_02"), ChallengeDungeonStage.StageLevel);
        });
    }

    public void OpenAccumBuffListPopup()
    {
        CloseClearRewardItemPopup();
        AccumBuffListPopup.gameObject.SetActive(true);

        AccumBuffListPopup.Init(AbilityActionList);
    }

    public void CloseAccumBuffListPopup()
	{
        if(AccumBuffListPopup.gameObject.activeSelf)
            AccumBuffListPopup.gameObject.SetActive(false);
	}

    private bool IsCompleteLastStage()
    {
        return Container.CurrentDungeonStage.StageLevel == Container.DungeonStageList[Container.DungeonStageList.Count - 1].StageLevel;
    }
}
