using Com.TheFallenGames.OSA.CustomAdapters.GridView;
using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIPetChangeViewHolder : CellViewsHolder
{
    private UIPetChangeListItem targetSlot;

    public void UpdateItemSlot(C_PetChangeData data, Action<C_PetChangeData> _onClickSlot)
    {
        targetSlot.SetSlot(data, _onClickSlot);
    }

    public override void CollectViews()
    {
        targetSlot = root.GetComponent<UIPetChangeListItem>();
        targetSlot.Initilaize();

        base.CollectViews();
    }
}

[Flags]// 정보 세팅 후 꺼줄항목
public enum E_PCR_PostSetting
{
    None = 0,
    BlockInput = 1 << 0,

    GainStateOn = 1 << 1,
    GainStateOff = 1 << 2,

    RegistStateOff = 1 << 3,

    SelectOn = 1 << 4,
    SelectOff = 1 << 5,

    CheckOn = 1 << 6,

    AdventurePowerOn = 1 << 7
}

public class UIPetChangeListItem : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image GradeImage;
    [SerializeField] private Image ChangeImage;
    [SerializeField] private Image IconElement;
    [SerializeField] private Image IconClass;
    [SerializeField] private GameObject LockStateObj;
    [SerializeField] private GameObject ChangeStateObj;
    [SerializeField] private Text ChangeStateTxt;
    [SerializeField] private GameObject SelectStateObj;
    [SerializeField] private GameObject GainStateObj;

    [SerializeField] private Text LevelTxt;
    [SerializeField] private Text GainCountTxt;

    [SerializeField] private GameObject CheckObj;
    [SerializeField] private GameObject AdventurePowerObj;
    [SerializeField] private Text txtAdventurePower;
    #endregion

    #region System Variable
    private bool IsInteractible = true;

    public Change_Table ChangeTableData => SlotData.changeData;
    public Pet_Table PetTableData => SlotData.petData;

    E_PetChangeViewType ViewType = E_PetChangeViewType.Change;

    public C_PetChangeData SlotData { get; private set; }

    public Action<C_PetChangeData> onClickSlot;
    #endregion

    public void Initilaize()
    {
        GradeImage.sprite = null;
        ChangeImage.sprite = null;
        IconElement.sprite = null;
        IconClass.sprite = null;

        SetDefaultState();
    }

    public void SetDefaultState()
    {
        IsInteractible = true;

        LockStateObj.SetActive(false);
        ChangeStateObj.SetActive(false);
        IconElement.gameObject.SetActive(false);
        IconClass.gameObject.SetActive(false);
        GainStateObj.gameObject.SetActive(true);
        LevelTxt.gameObject.SetActive(false);
        GainCountTxt.gameObject.SetActive(false);

        CheckObj.SetActive(false);
        AdventurePowerObj.SetActive(false);

        SetSelectState(false);
    }

    public void SetSlot(C_PetChangeData data, Action<C_PetChangeData> _onClickSlot)
    {
        SlotData = data;
        ViewType = SlotData.type;

        SetDefaultState();

        SetSelectState(data.isSelected);

        onClickSlot = _onClickSlot;

        switch (SlotData.type)
        {
            case E_PetChangeViewType.Change:
                SetSlot(data.changeData, Me.CurCharData.GetChangeDataByTID(data.Tid));
                break;
            case E_PetChangeViewType.Pet:
                SetSlot(data.petData, Me.CurCharData.GetPetData(data.Tid));
                break;
            case E_PetChangeViewType.Ride:
                SetSlot(data.petData, Me.CurCharData.GetRideData(data.Tid));
                break;
        }

        if (data != null)
        {
            if (data.ViewCount > 0)
            {
                GainCountTxt.gameObject.SetActive(true);
                GainCountTxt.text = data.ViewCount.ToString();
            }
        }

        if (data.postSetting != E_PCR_PostSetting.None)
            SetPostSetting(data.postSetting);
    }

    //강림설정
    public void SetSlot(Change_Table changeData, ChangeData myChangeData)
    {
        if (myChangeData != null)
        {
            GainStateObj.SetActive(false);
            LockStateObj.SetActive(myChangeData.IsLock);
            ChangeStateObj.SetActive(false);

            if (myChangeData.ChangeQuestTid > 0)//파견중임
            {
                ChangeStateObj.SetActive(true);

                if (myChangeData.ChangeQuestExpireDt <= TimeManager.NowSec)//파견완료
                {
                    ChangeStateTxt.text = DBLocale.GetText("Change_Quest_SendEnd");
                }
                else//파견중
                {
                    ChangeStateTxt.text = DBLocale.GetText("Change_Quest_Sending");
                }
            }
            else if (Me.CurCharData.MainChange == myChangeData.ChangeTid)// 메인강림체
            {
                ChangeStateObj.SetActive(true);

                if (Me.CurCharData.ChangeExpireDt > TimeManager.NowSec)//적용시간이 남았을때만 강림중임
                {
                    ChangeStateTxt.text = DBLocale.GetText("Change_Changed");
                }
                else
                {
                    ChangeStateTxt.text = DBLocale.GetText("Change_Registed");
                }
            }
        }

        IconElement.gameObject.SetActive(true);
        IconClass.gameObject.SetActive(true);

        GradeImage.sprite = UICommon.GetGradeSprite(changeData.Grade);
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(changeData.Icon);
        IconElement.sprite = UICommon.GetAttributeSprite(changeData.AttributeType, UICommon.E_SIZE_OPTION.Small);
        IconClass.sprite = UICommon.GetClassIconSprite(changeData.ClassIcon, UICommon.E_SIZE_OPTION.Small);
    }

    // 펫설정
    public void SetSlot(Pet_Table petData, PetData myPetData)
    {
        LevelTxt.gameObject.SetActive(myPetData != null);

        if (myPetData != null)
        {
            GainStateObj.SetActive(false);
            LockStateObj.SetActive(myPetData.IsLock);
            ChangeStateObj.SetActive(false);

            LevelTxt.text = DBLocale.GetText("Attribute_Level", DBPetLevel.GetLevel(petData.PetExpGroup, myPetData.Exp));

            if (petData.PetType == E_PetType.Pet)
            {
                if (myPetData.AdvId > 0)//모험중임
                {
                    ChangeStateObj.SetActive(true);

                    ChangeStateTxt.text = DBLocale.GetText("PetAdventure_Leaving");
                }
                else if (Me.CurCharData.MainPet == myPetData.PetTid)
                {
                    ChangeStateObj.SetActive(true);
                    if (Me.CurCharData.PetExpireDt > TimeManager.NowSec)//적용시간이 남았을때만 착용중임
                    {
                        ChangeStateTxt.text = DBLocale.GetText("Pet_Equiped");
                    }
                    else
                    {
                        ChangeStateTxt.text = DBLocale.GetText("Pet_Registed");
                    }
                }
            }
            else//탈것
            {//탈것은 장착중인지 여부만 판단
                if (Me.CurCharData.MainVehicle == myPetData.PetTid)
                {
                    ChangeStateObj.SetActive(true);
                    ChangeStateTxt.text = DBLocale.GetText("Pet_Registed");
                }
            }

        }

        GradeImage.sprite = UICommon.GetGradeSprite(petData.Grade);
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(petData.Icon);
    }

    /// <summary>
    /// 착용중, 모험중 등등 없는버전
    /// 캐릭터 정보창에서쓰임
    /// </summary>
    public void SetSlotSimple(C_PetChangeData data)
    {
        SlotData = data;
        ViewType = SlotData.type;

        SetDefaultState();

        GainStateObj.SetActive(false);

        switch (SlotData.type)
        {
            case E_PetChangeViewType.Change:
                SetSlotSimple(SlotData.changeData, Me.CurCharData.GetChangeDataByTID(data.Tid));
                break;
            case E_PetChangeViewType.Pet:
                SetSlotSimple(SlotData.petData, Me.CurCharData.GetPetData(data.Tid));
                break;
            case E_PetChangeViewType.Ride:
                SetSlotSimple(SlotData.petData, Me.CurCharData.GetRideData(data.Tid));
                break;
        }
    }

    // 사진만뜸
    public void SetSlotSimple(E_PetChangeViewType type, uint tid, Action<C_PetChangeData> onClick = null, E_PCR_PostSetting postSetting = E_PCR_PostSetting.None)
    {
        SetDefaultState();

        onClickSlot = onClick;

        switch (type)
        {
            case E_PetChangeViewType.Change:
                {
                    var table = DBChange.Get(tid);
                    ChangeImage.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
                    GradeImage.sprite = UICommon.GetGradeSprite(table.Grade);
                }
                break;
            case E_PetChangeViewType.Pet:
                {
                    var table = DBPet.GetPetData(tid);
                    ChangeImage.sprite = ZManagerUIPreset.Instance.GetSprite(table.Icon);
                    GradeImage.sprite = UICommon.GetGradeSprite(table.Grade);
                }
                break;
            case E_PetChangeViewType.Ride:
                break;
        }

        SetPostSetting(postSetting);
    }

    private void SetSlotSimple(Change_Table changeData, ChangeData data)
    {
        LockStateObj.SetActive(data?.IsLock ?? false);

        IconElement.gameObject.SetActive(true);
        IconClass.gameObject.SetActive(true);

        GradeImage.sprite = UICommon.GetGradeSprite(changeData.Grade);
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(changeData.Icon);
        IconElement.sprite = UICommon.GetAttributeSprite(changeData.AttributeType, UICommon.E_SIZE_OPTION.Small);
        IconClass.sprite = UICommon.GetClassIconSprite(changeData.ClassIcon, UICommon.E_SIZE_OPTION.Small);

    }

    private void SetSlotSimple(Pet_Table petData, PetData data)
    {
        LockStateObj.SetActive(data?.IsLock ?? false);

        GradeImage.sprite = UICommon.GetGradeSprite(petData.Grade);
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(petData.Icon);
    }
    public void SetSelectState(bool state)
    {
        SelectStateObj.SetActive(state);
    }

    public void SetPostSetting(E_PCR_PostSetting postSetting)
    {
        if (postSetting.HasFlag(E_PCR_PostSetting.BlockInput))
            IsInteractible = false;

        if (postSetting.HasFlag(E_PCR_PostSetting.GainStateOn))
            GainStateObj.SetActive(true);

        if (postSetting.HasFlag(E_PCR_PostSetting.GainStateOff))
            GainStateObj.SetActive(false);

        if (postSetting.HasFlag(E_PCR_PostSetting.RegistStateOff))
            ChangeStateObj.SetActive(false);

        if (postSetting.HasFlag(E_PCR_PostSetting.SelectOn))
            SelectStateObj.SetActive(true);

        if (postSetting.HasFlag(E_PCR_PostSetting.SelectOff))
            SelectStateObj.SetActive(false);

        if (postSetting.HasFlag(E_PCR_PostSetting.CheckOn))
            CheckObj.SetActive(true);

        if (postSetting.HasFlag(E_PCR_PostSetting.AdventurePowerOn))
        {
            if (SlotData?.type == E_PetChangeViewType.Pet)
            {
                AdventurePowerObj.SetActive(true);
                txtAdventurePower.text = DBPetAdventure.GetPetAdventurePower(Me.CurCharData.GetPetData(SlotData.Tid)).ToString();
            }
        }
    }

    public void HandleSlotSelect()
    {
        if (IsInteractible == false)
            return;

        onClickSlot?.Invoke(SlotData);

        //SetSelectState(true);
    }
}
