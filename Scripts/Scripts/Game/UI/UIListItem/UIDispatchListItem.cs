using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZDefine;
using UnityEngine.UI;

public class UIDispatchDataHolder : ZAdapterHolderBase<OSA_DispatchData>
{
    private UIDispatchListItem listItem;
    private Action<OSA_DispatchData> onClick;
    private Action<ChangeQuestData> onClickReward;

    public override void SetSlot(OSA_DispatchData data)
    {
        listItem.SetSlot(data);
    }

    public override void CollectViews()
    {
        base.CollectViews();
        listItem = root.GetComponent<UIDispatchListItem>();
    }

    public void SetAction(Action<OSA_DispatchData> _onClick, Action<ChangeQuestData> _onClickReward)
    {
        onClick = _onClick;
        onClickReward = _onClickReward;
        listItem.SetAction(onClick, onClickReward);
    }

    public void RefreshRemainTime()
    {
        listItem.RefreshRemainTime();
    }
}

public class UIDispatchListItem : MonoBehaviour
{
    private enum E_DispatchState
    {
        None = 0,
        InProgress = 1,// 진행중
        Complete = 2,// 완료(보상받기전)
    }

    [SerializeField] private UIItemSlot itemSlot;// 배ㅑ열임!

    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtState;

    [SerializeField] private List<Image> listGradeStar;

    [SerializeField] private GameObject objProgress;
    [SerializeField] private Slider sliderProgress;
    [SerializeField] private Text txtRemainTime;

    [SerializeField] private GameObject objComplete;

    [SerializeField] private GameObject objSelect;

    [SerializeField] private ZButton btnReward;

    private Action<OSA_DispatchData> onSelect;
    private Action<ChangeQuestData> onClickReward;

    private OSA_DispatchData data;

    public void SetSlot(OSA_DispatchData _data)
    {
        objSelect.SetActive(_data.isSelected);
        data = _data;

        if (DBChangeQuest.GetChangeQuest(data.data.QuestTid, out var table) == false)
        {
            ZLog.LogError(ZLogChannel.UI, $"퀘스트 정보가 없습ㄴ다!!!! TID : {data.data.QuestTid}");
            return;
        }

        itemSlot.SetItem(table.RewardItem[0], table.RewardItemCount[0]);

        txtTitle.text = DBUIResouce.GetGradeFormat(DBLocale.GetText(table.QuestTitle),(byte)table.QuestGrade);

        for (int i = 0; i < listGradeStar.Count; i++)
        {
            listGradeStar[i].sprite = UICommon.GetStarSprite(i < table.QuestGrade);
        }

        RefreshRemainTime();
    }

    public void SetAction(Action<OSA_DispatchData> _onSelect, Action<ChangeQuestData> _onClickReward)
    {
        onSelect = _onSelect;
        onClickReward = _onClickReward;
    }

    public void RefreshRemainTime()
    {
        E_DispatchState state = E_DispatchState.None;

        ulong now = TimeManager.NowSec;

        if (data.data.EndDt > 0 && data.data.EndDt <= now)
        {
            state = E_DispatchState.Complete;
        }
        else if (data.data.StartDt > 0)
        {
            state = E_DispatchState.InProgress;
        }

        switch (state)
        {
            case E_DispatchState.None:
                if (objProgress.activeSelf)
                    objProgress.SetActive(false);
                if (objComplete.activeSelf)
                    objComplete.SetActive(false);

                if(txtState.gameObject.activeSelf)
                    txtState.gameObject.SetActive(false);

                break;
            case E_DispatchState.InProgress:
                ulong total = data.data.EndDt - data.data.StartDt;
                ulong progress = total - (now - data.data.StartDt);

                if (objProgress.activeSelf == false)
                    objProgress.SetActive(true);
                if (objComplete.activeSelf)
                    objComplete.SetActive(false);

                sliderProgress.value = 1f -(float)progress / (float)total;
                txtRemainTime.text = TimeHelper.GetRemainTime(progress);

                if (txtState.gameObject.activeSelf==false)
                    txtState.gameObject.SetActive(true);

                txtState.text = DBLocale.GetText("Change_Quest_Sending");
                break;
            case E_DispatchState.Complete:
                if (objProgress.activeSelf)
                    objProgress.SetActive(false);
                if (objComplete.activeSelf == false)
                    objComplete.SetActive(true);
                if(btnReward.interactable == false)
                    btnReward.interactable = true;

                if (txtState.gameObject.activeSelf == false)
                    txtState.gameObject.SetActive(true);

                txtState.text = DBLocale.GetText("Change_Quest_SendEnd");
                break;
        }
    }

    public void OnClickItemSlot()
    {

    }

    public void OnClickComplete()
    {
        btnReward.interactable = false;
        onClickReward?.Invoke(data.data);
    }

    public void OnClickSlot()
    {
        onSelect?.Invoke(data);
    }
}
