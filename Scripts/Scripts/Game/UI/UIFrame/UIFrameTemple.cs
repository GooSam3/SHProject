using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIFrameTemple : ZUIFrameBase
{
    //[SerializeField] private ZToggle[] tapAreas = new ZToggle[3];

    [SerializeField] private ScrollRect tampleAreasScrollRect;

    [SerializeField] private ScrollRect clearRewardItemScrollRect;

    [SerializeField] private ScrollRect templeRewardItemScrollRect;

    [SerializeField] private ScrollRect InfotextScrollRect;

    [SerializeField] private ZText templeTitle;
    [SerializeField] private ZText npcName;
    [SerializeField] private ZText areaNameText;
    [SerializeField] private ZText templeInfoText;
    [SerializeField] private ZText templeStartButtonText;
    [SerializeField] private ZButton templeStartButton;

    private List<UITempleListItem> TempleListItems = new List<UITempleListItem>();
    private List<UITempleClearListItem> TempleClearItems = new List<UITempleClearListItem>();
    private List<UITempleRewardListItem> TempleRewardListItems = new List<UITempleRewardListItem>();

    private GameObject UITempleListItemPrefab;
    private GameObject UITempleClearListItemPrefab;
    private GameObject UITempleRewardListItemPrefab;

    private uint selectStageTid;
    private E_TempleType selectTempleType = E_TempleType.God;
    private bool IsPrepared = false;

    public override bool IsBackable => true;

    private void Init(Action loadEvent = null)
    {
        templeInfoText.text = string.Empty;
        npcName.text = string.Empty;
        areaNameText.text = string.Empty;
        //  templeIcon.sprite = null;
        templeTitle.text = string.Empty;
        selectTempleType = E_TempleType.Normal; //TODO: 기획요청으로 임시 수정

        ZResourceManager.Instance.Load<GameObject>(nameof(UITempleListItem), (uiTempleListItemAssetName, uiTempleListItemPrefab) =>
        {
            ZResourceManager.Instance.Load<GameObject>(nameof(UITempleRewardListItem), (uITempleRewardListItemAssetName, uITempleRewardListItemPrefab) =>
            {
                ZResourceManager.Instance.Load<GameObject>(nameof(UITempleClearListItem), (uITempleClearListItemAssetName, uITempleClearListItemPrefab) =>
                {
                    UITempleListItemPrefab = uiTempleListItemPrefab;
                    UITempleRewardListItemPrefab = uITempleRewardListItemPrefab;
                    UITempleClearListItemPrefab = uITempleClearListItemPrefab;
                    if (loadEvent != null)
                    {
                        loadEvent();
                    }
                });
            });
        }); 
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        if(UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame(E_UIStyle.FullScreen);

        Init(() =>
        {
            IsPrepared = true;

            // 임시 코드 (강제로 3번째 탭 세팅)
            OnToggleValueChanged((int)E_TempleType.God);
        });
    }

    public void DoRefresh(bool isResetPos)
    {
        DoInfoUpdate(isResetPos);
        DoListUpdate(isResetPos);
        DoRewardItemUpdate(isResetPos);
        DoClearItemUpdate(isResetPos);
    }

    /// <summary>
    /// Temple정보 업데이트
    /// </summary>
    public void DoInfoUpdate(bool isResetPos)
    {
        if (selectStageTid != 0)
        {
            //   templeIcon.gameObject.SetActive(false);
            var stageTable = DBStage.Get(selectStageTid);
            var templeTable = DBTemple.Get(stageTable.LinkTempleID);

            if (templeTable == null)
            {
                ZLog.Log(ZLogChannel.Default, $"Temple StageTableID 오류입니다.:{ stageTable.LinkTempleID} ");
            }

            var templeList = ZNet.Data.Me.CurCharData.TempleInfo.GetStage(stageTable.StageID);
            templeTitle.text = DBLocale.GetText(stageTable.StageTextID);

            if (stageTable.StageDescID != null) templeInfoText.text = DBLocale.GetText(stageTable.StageDescID);
            if (templeTable.TempleAreaName != null) areaNameText.text = DBLocale.GetText(templeTable.TempleAreaName);
            // if (templeTable.Icon != null) templeIcon.sprite = ZManagerUIPreset.Instance.GetUIManagerSpriteFromAtlas(templeTable.Icon);

            npcName.text = DBStage.GetQuestNpcName(stageTable.StageID);

            // 버튼 Text
            var state = ZNet.Data.Me.CurCharData.TempleInfo.GetTempleStartType(stageTable.StageID);
            switch (state)
            {
                case E_TempleInfoState.Close: // 잠김중에는 입장 표시 근대로 단지 메세지 박스로 알려주기
                case E_TempleInfoState.Enter: // 입장
                    {
                        templeStartButtonText.text = DBLocale.GetText("Temple_Enter");
                        templeStartButton.interactable = true;
                    }
                    break;
                case E_TempleInfoState.Replay: //다시하기
                    {
                        templeStartButtonText.text = DBLocale.GetText("Temple_Replay");// "다시하기";
                        templeStartButton.interactable = true;
                    }
                    break;
                case E_TempleInfoState.Clear: // 클리어
                    {
                        templeStartButtonText.text = DBLocale.GetText("Temple_First_Clear_Reward"); // 클리어
                        templeStartButton.interactable = false;
                    }
                    break;
            }
        }
        else
        {
            templeTitle.text = string.Empty;
            templeInfoText.text = string.Empty;
            npcName.text = string.Empty;
            areaNameText.text = string.Empty;
            //  templeIcon.gameObject.SetActive(false);

            templeStartButton.interactable = false;
        }
    }
    /// <summary>
    ///  리스트정보 업데이트
    /// </summary>
    public void DoListUpdate(bool isResetPos)
    {
        var templeTypeList = DBStage.GetTempleTypeList(selectTempleType);
        int loadItemCount = templeTypeList.Count - TempleListItems.Count;

        for (int i = 0; i < TempleListItems.Count; i++)
        {
            TempleListItems[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < loadItemCount; i++)
        {
            UITempleListItem obj = GameObject.Instantiate(UITempleListItemPrefab).GetComponent<UITempleListItem>();
            obj.transform.SetParent(tampleAreasScrollRect.content.transform);
            obj.transform.localScale = Vector3.one;
            obj.transform.localPosition = Vector3.zero;

            TempleListItems.Add(obj);
            obj.gameObject.SetActive(false);
        }

        if (isResetPos == true)
        {
            tampleAreasScrollRect.content.anchoredPosition = Vector2.zero;
            tampleAreasScrollRect.velocity = Vector2.zero;
        }

        for (int i = 0; i < templeTypeList.Count; i++)
        {
            TempleListItems[i].DoInit(templeTypeList[i].StageID, OnTempleListItemSelectEvent);
            TempleListItems[i].DoUpdate(selectStageTid);
        }
    }
    /// <summary>
    /// 유적 보물 
    /// </summary>
    /// 

    public void DoRewardItemUpdate(bool isResetPos)
    {
        for (int i = 0; i < TempleRewardListItems.Count; i++)
        {
            TempleRewardListItems[i].gameObject.SetActive(false);
        }

        if (selectStageTid != 0)
        {
            var stageTableData = DBStage.Get(selectStageTid);
            if (DBTemple.TryGet(stageTableData.LinkTempleID, out var templeTableData))
            {

                ZLog.Log(ZLogChannel.Default, $"DoRewardItemUpdate::{templeTableData.GachaGroupID.Count}");

                int loadItemCount = templeTableData.GachaGroupID.Count - TempleRewardListItems.Count;

                for (int i = 0; i < loadItemCount; i++)
                {
                    UITempleRewardListItem obj = GameObject.Instantiate(UITempleRewardListItemPrefab).GetComponent<UITempleRewardListItem>();
                    obj.transform.SetParent(templeRewardItemScrollRect.content.transform);
                    obj.transform.localScale = Vector3.one;
                    obj.transform.localPosition = Vector3.zero;

                    TempleRewardListItems.Add(obj);
                    obj.gameObject.SetActive(false);
                }
                var templeList = ZNet.Data.Me.CurCharData.TempleInfo.GetStage(stageTableData.StageID);

                for (int i = 0; i < templeTableData.GachaGroupID.Count; i++)
                {
                  //  templeList.rewardGachaOpens.FindAll(templeTableData.GachaGroupID[i]);

                        //TempleRewardListItems[i].DoInit(templeTableData.GachaGroupID[i], false);
                    //TempleRewardListItems[i].DoUpdate(templeList == null ? 0 : templeList.ClearDts);
                }
            }
        }

    }

    /// <summary>
    /// 클리어 아이템 정보 업데이트
    /// </summary>
    public void DoClearItemUpdate(bool isResetPos)
    {
        for (int i = 0; i < TempleClearItems.Count; i++)
        {
            TempleClearItems[i].gameObject.SetActive(false);
        }
        if (DBStage.TryGet(selectStageTid, out var stageTableData))
        {
            int loadItemCount = stageTableData.ClearRewardID.Count - TempleClearItems.Count;
            var templeList = ZNet.Data.Me.CurCharData.TempleInfo.GetStage(stageTableData.StageID);
          
            for (int i = 0; i < loadItemCount; i++)
            {
                UITempleClearListItem obj = GameObject.Instantiate(UITempleClearListItemPrefab).GetComponent<UITempleClearListItem>();
                obj.transform.SetParent(clearRewardItemScrollRect.content.transform);
                obj.transform.localScale = Vector3.one;
                obj.transform.localPosition = Vector3.zero;

                TempleClearItems.Add(obj);
                obj.gameObject.SetActive(false);
            }

            for (int i = 0; i < stageTableData.ClearRewardID.Count; i++)
            {
                TempleClearItems[i].DoInit(stageTableData.ClearRewardID[i], stageTableData.ClearRewardCount[i]);
                TempleClearItems[i].DoUpdate(templeList == null ? 0 : templeList.ClearDts);
            }

        }
        else { }
    }

    public void OnTempleListItemSelectEvent(uint stageTid)
    {
        selectStageTid = stageTid;
        DoRefresh(false);
    }

    public void OnToggleValueChanged(int type)
    {
        switch (type)
        {
            case 1: { selectTempleType = E_TempleType.God; } break;
            case 2: { selectTempleType = E_TempleType.Quest; } break;
            case 3: { selectTempleType = E_TempleType.Normal; } break;
        }

        //if (tapAreas[type - 1].isOn)
        {
            var stageTypeList = DBStage.GetTempleTypeList(selectTempleType);
            if (stageTypeList.Count > 0)
            {
                selectStageTid = stageTypeList[0].StageID;
            }
            else
            {
                selectStageTid = 0;
            }
            DoRefresh(true);
        }
    }

    public void OnTempleOpen()
    {
        if (DBStage.TryGet(selectStageTid, out var stageTable))
        {
            if (DBTemple.TryGet(stageTable.LinkTempleID, out var templeTable))
            {
                ZLog.Log(ZLogChannel.WebSocket, $"{templeTable.EntrancePortalID}");
                ZGameManager.Instance.TryEnterStage(templeTable.EntrancePortalID, false, 0, 0);

               // UIManager.Instance.Find<UIFrameHUD>().SetSubHudFrame();
                OnClose();
                // ZManagerLoaderPrefab.Instance.DoSpawn(nameof(UIPopupTempleAlways), delegate
                // {
                //
                //     UIPopupTempleAlways messageBox = ZManagerLoaderPrefab.Instance.DoLoad(nameof(UIPopupTempleAlways)).GetComponent<UIPopupTempleAlways>();
                //     messageBox.Open(type: UIPopupMessageBox.E_MsgType.TWO, OkEvent: () =>
                //       {
                //           ZGameManager.Instance.TryEnterStage(templeTable.EntrancePortalID, 0, 0, 0);
                //           OnClose();
                //       });
                // }
                // );

                //시연후 풀어주자. 버튼 특성
               // switch (stageTable.StageOpenType)
               // {
               //     // Popup 창을 띠워서 메세지 뿌려주고 시작할수 있게 하기
               //     // 다시하기와, 일반 유적은 그냥 입장 팝업
               //     case E_StageOpenType.Always:
               //         {
               //             ZGameManager.Instance.TryEnterStage(templeTable.EntrancePortalID, 0, 0, 0);
               //         }
               //         break;
               //     // 아이템은 아이템 장착 팝업또는 자동으로 슬록 검색가능 기능?
               //     case E_StageOpenType.Item:
               //         {
               //             ZGameManager.Instance.TryEnterStage(templeTable.EntrancePortalID, 0, 0, 0);
               //         }
               //         break;
               // }
            }
        }
        else
        {
            ZLog.Log(ZLogChannel.Default, $"Temple 스테이지정보가 넘어오지 않았습니다. : {selectStageTid} ");
        }
    }
    public void OnClose()
    {
        if (false == IsPrepared)
            return;

        UIManager.Instance.Close<UIFrameTemple>(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
        RemoveFrameTemple();
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        RemoveFrameTemple();
    }

    private void RemoveFrameTemple()
    {
        if (UIManager.Instance.Find(out UIFrameHUD _hud))
            _hud.SetSubHudFrame();
    }
   
}
