using GameDB;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class ZUIScrollPetChangeListItem : CUGUIWidgetSlotItemBase
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
    #endregion

    #region System Variable
    private const string FORMAT_GRADE_BG = "img_grade_0{0}";

    private bool IsSelected = false;

    public Change_Table ChangeTableData => SlotData.changeData;
    public Pet_Table PetTableData => SlotData.petData;

    E_PetChangeViewType ViewType = E_PetChangeViewType.Change;

    public C_PetChangeData SlotData { get; private set; }
    #endregion

    protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitialize(_UIFrameParent);

        GradeImage.sprite = null;
        ChangeImage.sprite = null;
        IconElement.sprite = null;
        IconClass.sprite = null;

        SetDefaultState();
    }

    public void SetDefaultState()
    {
        LockStateObj.SetActive(false);
        ChangeStateObj.SetActive(false);
        IconElement.gameObject.SetActive(false);
        IconClass.gameObject.SetActive(false);
        GainStateObj.gameObject.SetActive(true);
        SetSelectState(false);
    }

    public void SetSlot(C_PetChangeData data)
    {
        SlotData = data;
        ViewType = SlotData.type;

        SetDefaultState();

        switch (SlotData.type)
        {
            case E_PetChangeViewType.Change:
                SetSlot(data.changeData, Me.CurCharData.GetChangeDataByTID(data.Tid));
                break;
            case E_PetChangeViewType.Pet:
                SetSlot(data.petData,Me.CurCharData.GetPetData(data.Tid));
                break;
            case E_PetChangeViewType.Ride:
                break;
        }

    }

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

        GradeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, changeData.Grade));
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(changeData.Icon);
        IconElement.sprite = UICommon.GetAttributeSprite(changeData.AttributeType, UICommon.E_SIZE_OPTION.Small);
        IconClass.sprite = UICommon.GetClassIconSprite(changeData.ClassIcon, UICommon.E_SIZE_OPTION.Small);
    }

    public void SetSlot(Pet_Table petData, PetData myPetData)
    {
        if (myPetData != null)
        {
            GainStateObj.SetActive(false);
            LockStateObj.SetActive(myPetData.IsLock);
            ChangeStateObj.SetActive(false);

            if (myPetData.AdvId > 0)//모험중임
            {
                ChangeStateObj.SetActive(true);

                ChangeStateTxt.text = DBLocale.GetText("PetAdventure_Leaving");
            }
            else if (Me.CurCharData.MainPet == myPetData.PetTid)
            {
                ChangeStateObj.SetActive(true);
                if (Me.CurCharData.PetExpireDt>TimeManager.NowSec)//적용시간이 남았을때만 착용중임
                {
                    ChangeStateTxt.text = DBLocale.GetText("Pet_Equiped");
                }
                else
                {
                    ChangeStateTxt.text = DBLocale.GetText("Pet_Registed");
                }

            }
        }

        GradeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, petData.Grade));
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
                SetSlotSimple(SlotData.changeData, Me.CurCharData.GetChangeDataByTID(data.Tid).IsLock);
                break;
            case E_PetChangeViewType.Pet:
                SetSlotSimple(SlotData.petData, Me.CurCharData.GetPetData(data.Tid).IsLock);
                break;
            case E_PetChangeViewType.Ride:
                break;
        }
    }

    private void SetSlotSimple(Change_Table changeData, bool isLock)
    {

        LockStateObj.SetActive(isLock);

        IconElement.gameObject.SetActive(true);
        IconClass.gameObject.SetActive(true);

        GradeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, changeData.Grade));
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(changeData.Icon);
        IconElement.sprite = UICommon.GetAttributeSprite(changeData.AttributeType, UICommon.E_SIZE_OPTION.Small);
        IconClass.sprite = UICommon.GetClassIconSprite(changeData.ClassIcon, UICommon.E_SIZE_OPTION.Small);

    }

    private void SetSlotSimple(Pet_Table petData, bool isLock)
    {
        LockStateObj.SetActive(isLock);

        GradeImage.sprite =
             ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, petData.Grade));
        ChangeImage.sprite =
            ZManagerUIPreset.Instance.GetSprite(petData.Icon);
    }
    public void SetSelectState(bool state)
    {
        SelectStateObj.SetActive(state);
    }

    public void HandleSlotSelect()
    {
        if (mSlotItemOwner == null) return;

        mSlotItemOwner.ISlotItemSelect(this);
        SetSelectState(true);
    }
}
