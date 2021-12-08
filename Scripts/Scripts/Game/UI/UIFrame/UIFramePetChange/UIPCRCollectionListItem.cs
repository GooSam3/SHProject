using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class PCRCollectionHolder : ZFittedAdapterHolderBase<ScrollPCRCollectionData>
{
    private UIPCRCollectionListItem listItem;

    public override void CollectViews()
    {
        listItem = root.GetComponent<UIPCRCollectionListItem>();
        listItem.Initilialize();
        base.CollectViews();
    }

    public override void SetSlot(ScrollPCRCollectionData data)
    {
        listItem.SetSlot(data);
    }

    public void SetEvent(Action<ScrollPCRCollectionData> _onClickSlotToggle, Action<ScrollPCRCollectionData, C_PetChangeData> _onClickSlotPCR)
    {
        listItem.SetEvent(_onClickSlotToggle, _onClickSlotPCR);
    }
}

public class UIPCRCollectionListItem : MonoBehaviour
{
    private const float ALPHA_IMAGE_ON = .785f;
    private const float ALPHA_IMAGE_OFF = .4f;

    private const float ALPHA_CANVAS_ON = 1f;
    private const float ALPHA_CANVAS_OFF = .5f;

    private const float ALPHA_ICON_OFF = 0.2509804f;
    private const float ALPHA_ICON_ON = 1f;

    [SerializeField] Color COLOR_TITLE_PROGRESS;
    [SerializeField] Color COLOR_TITLE_COMPELTE;

    [SerializeField] private GameObject objDetail;

    [SerializeField] private GameObject objToggle;

    [SerializeField] private GameObject objComplete;

    [SerializeField] private Text txtTitle;

    [SerializeField] private Text txtProgress;

    [SerializeField] private List<UIPetChangeListItem> listSlot;

    [SerializeField] private Text txtAbility;

    [SerializeField] private Image imgBackground;

    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Image imgIconCollection;

    private Action<ScrollPCRCollectionData> onClickSlotToggle;// open toggle -> refresh
    private Action<ScrollPCRCollectionData, C_PetChangeData> onClickSlotPCR; // click pcr -> model

    private ScrollPCRCollectionData scrollData;

    public void Initilialize()
    {
        foreach (var iter in listSlot)
        {
            iter.Initilaize();
            iter.onClickSlot = OnClickSlotPCR;
        }

        SetDefault();
    }

    public void SetEvent(Action<ScrollPCRCollectionData> _onClickSlotToggle, Action<ScrollPCRCollectionData, C_PetChangeData> _onClickSlotPCR)
    {
        onClickSlotToggle = _onClickSlotToggle;
        onClickSlotPCR = _onClickSlotPCR;
    }

    private void SetDefault()
    {
        foreach (var iter in listSlot)
            iter.gameObject.SetActive(false);
    }

    public void SetSlot(ScrollPCRCollectionData data)
    {
        scrollData = data;

        SetDefault();

        SetToggle(data.IsOpened);

        switch (data.ViewType)
        {
            case E_PetChangeViewType.Change:
                SetChangeSlot(DBChangeCollect.GetChangeCollection(data.Tid));
                break;
            case E_PetChangeViewType.Pet:
                if (DBPetCollect.GetPetCollection(data.Tid, out var petTable) == false)
                    return;
                SetPetSlot(petTable);
                break;
            case E_PetChangeViewType.Ride:
                if (DBPetCollect.GetRideCollection(data.Tid, out var rideTable) == false)
                    return;
                SetRideSlot(rideTable);
                break;
        }

    }

    private void SetAbilityAction(uint tidOne, uint tidTwo)
    {
        List<UIAbilityData> list = new List<UIAbilityData>();

        DBAbilityAction.GetAbilityTypeList(tidOne, ref list);
        DBAbilityAction.GetAbilityTypeList(tidTwo, ref list);

        string abilityText = string.Empty;

        for (int i = 0; i < list.Count; i++)
        {
            abilityText += $"{DBLocale.GetText(DBAbility.GetAbilityName(list[i].type))} {DBAbility.ParseAbilityValue((uint)list[i].type, list[i].value)}";

            if (i >= list.Count - 1)
                continue;

            abilityText += "\n";
        }

        txtAbility.text = abilityText;
    }

    private void SetState(bool state)
    {
        canvasGroup.alpha = state ? ALPHA_CANVAS_ON : ALPHA_CANVAS_OFF;

        Color origin = imgBackground.color;
        origin.a = state ? ALPHA_IMAGE_ON : ALPHA_IMAGE_OFF;

        var imgColor = imgIconCollection.color;

        imgColor.a = state ? ALPHA_ICON_ON : ALPHA_ICON_OFF;
        imgIconCollection.color = imgColor;

        txtTitle.color = state ? COLOR_TITLE_COMPELTE : COLOR_TITLE_PROGRESS;

        imgBackground.color = origin;

        if (state)
        {
            txtProgress.text = DBLocale.GetText("PCR_Collection_Complete");
        }

        objComplete.SetActive(state);
    }


    public void SetToggle(bool b)
    {
        objToggle.SetActive(!b);
        objDetail.SetActive(b);
    }

    private void SetChangeSlot(ChangeCollection_Table data)
    {
        txtTitle.text = data.ChangeCollectionTextID;

        var changeList = DBChangeCollect.GetCollectChangeList(data);

        int progress = 0;
        for (int i = 0; i < changeList.Count; i++)
        {
            listSlot[i].SetSlotSimple(new C_PetChangeData(changeList[i]));
            listSlot[i].gameObject.SetActive(true);

            E_PCR_PostSetting postSetting = E_PCR_PostSetting.None;

            if (Me.CurCharData.GetChangeDataByTID(changeList[i].ChangeID) != null)
                progress++;
            else
                postSetting |= E_PCR_PostSetting.GainStateOn;

            if (scrollData.SelectedTid == changeList[i].ChangeID)
                postSetting |= E_PCR_PostSetting.SelectOn;
            else
                postSetting |= E_PCR_PostSetting.SelectOff;


            listSlot[i].SetPostSetting(postSetting);
        }

        txtProgress.text = UICommon.GetProgressText(progress, changeList.Count);

        SetAbilityAction(data.AbilityActionID_01, data.AbilityActionID_02);

        SetState(progress == changeList.Count);
    }

    private void SetPetSlot(PetCollection_Table data)
    {
        txtTitle.text = data.PetCollectionTextID;

        var petRideList = DBPetCollect.GetCollectPetRideList(data);

        int progress = 0;
        for (int i = 0; i < petRideList.Count; i++)
        {
            listSlot[i].SetSlotSimple(new C_PetChangeData(petRideList[i]));
            listSlot[i].gameObject.SetActive(true);

            E_PCR_PostSetting postSetting = E_PCR_PostSetting.None;

            if (Me.CurCharData.GetPetData(petRideList[i].PetID) != null)
                progress++;
            else
                postSetting |= E_PCR_PostSetting.GainStateOn;

            if (scrollData.SelectedTid == petRideList[i].PetID)
                postSetting |= E_PCR_PostSetting.SelectOn;
            else
                postSetting |= E_PCR_PostSetting.SelectOff;


            listSlot[i].SetPostSetting(postSetting);
        }

        txtProgress.text = UICommon.GetProgressText(progress, petRideList.Count);

        SetAbilityAction(data.AbilityActionID_01, data.AbilityActionID_02);

        SetState(progress == petRideList.Count);
    }

    private void SetRideSlot(PetCollection_Table data)
    {
        txtTitle.text = data.PetCollectionTextID;

        var petRideList = DBPetCollect.GetCollectPetRideList(data);

        int progress = 0;
        for (int i = 0; i < petRideList.Count; i++)
        {
            listSlot[i].SetSlotSimple(new C_PetChangeData(petRideList[i]));
            listSlot[i].gameObject.SetActive(true);

            E_PCR_PostSetting postSetting = E_PCR_PostSetting.None;

            if (Me.CurCharData.GetRideData(petRideList[i].PetID) != null)
                progress++;
            else
                postSetting |= E_PCR_PostSetting.GainStateOn;

            if (scrollData.SelectedTid == petRideList[i].PetID)
                postSetting |= E_PCR_PostSetting.SelectOn;
            else
                postSetting |= E_PCR_PostSetting.SelectOff;


            listSlot[i].SetPostSetting(postSetting);
        }

        txtProgress.text = UICommon.GetProgressText(progress, petRideList.Count);

        SetAbilityAction(data.AbilityActionID_01, data.AbilityActionID_02);

        SetState(progress == petRideList.Count);
    }

    public void OnClickToggleFold()
    {
        onClickSlotToggle?.Invoke(scrollData);
    }

    private void OnClickSlotPCR(C_PetChangeData pcrData)
    {
        onClickSlotPCR?.Invoke(scrollData, pcrData);
    }
}
