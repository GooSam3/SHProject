using GameDB;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

// 펱용
public class PCContentListViewPet : PCContentListViewPR
{
    [SerializeField] protected GameObject UnEquipButton;

    protected override void InitilaizeList()
    {
        ListContentData.Clear();

        foreach (var data in DBPet.GetAllPetData())
        {
            if (data.ViewType != GameDB.E_ViewType.View)
                continue;

            if (data.PetType != E_PetType.Pet)
                continue;

            uint cnt = Me.CurCharData.GetPetData(data.PetID)?.Cnt ?? 0;

            ListContentData.Add(new C_PetChangeData(data, (int)cnt));
        }
    }

    protected override void SetUI(C_PetChangeData data)
    {
        base.SetUI(data);
        Pet_Table tableData = data.petData;

        SelectPetName.text = DBUIResouce.GetPetGradeFormat(DBLocale.GetText(tableData.PetTextID), tableData.Grade);

        var myPetData = Me.CurCharData.GetPetData(data.Tid);

        hasValue = myPetData != null && myPetData.AdvId == 0;

        SummonButton.interactable = (myPetData?.AdvId??0)<=0;

        isRegisted = data.Tid == Me.CurCharData.MainPet;

        if (hasValue)
        {
            bool isSpawnable = !isRegisted;
            if (isRegisted)// 남은시간 없다면 등록해제로처리
                isSpawnable = Me.CurCharData.PetExpireDt < TimeManager.NowSec;

            UnEquipButton.gameObject.SetActive(!isSpawnable);
            SummonButton.gameObject.SetActive(isSpawnable);
        }
        else
		{
            UnEquipButton.gameObject.SetActive(false);
            SummonButton.gameObject.SetActive(false);
        }

        // 기획사항 : 소환 및 해제 둘중하나만 출력, 아래코드는 혹시모르니 놔둠
        //
        //
        //
        //SummonButton.interactable = hasValue;
        //
        //if (hasValue && isRegisted && Me.CurCharData.PetExpireDt > TimeManager.NowSec)
        //    SummonButton.interactable = false;
        //
        //UnEquipButton.gameObject.SetActive(hasValue && isRegisted);
    }

    public void OnClickUnEquip()
    {
        ZWebManager.Instance.WebGame.REQ_UnEquipPet((recvPacket, recvMsgPacket) =>
        {
            ScrollPetChange.RefreshData();

            SetUI(CurSelectedData);
        });
    }

    private void ReqSummonPet()
    {
        //있는지 한번더확인
        PetData petData = Me.CurCharData.GetPetData(CurSelectedData.Tid);
        if (petData == null)
        {
            // 만약 없다면 버튼꺼줌
            SummonButton.interactable = false;
            return;
        }

        //소모아이템 비교
        ZItem matItem = Me.CurCharData.GetInvenItemUsingMaterial(DBConfig.Pet_Summon_Item);
        if (hasValue == false || (matItem == null || matItem.cnt <= 0))
        {
            UIMessagePopup.ShowPopupOk(DBLocale.GetText("NOT_ENOUGH_PET_USE_ITEM"));
            return;
        }

        //소환
        ZWebManager.Instance.WebGame.REQ_EquipPet(petData.PetId,
                                                  petData.PetTid,
                                                  matItem.item_id,
                                                  (recvPacket, recvMsgPacket) =>
                                                  {
                                                          // 강림패널꺼줌
                                                          UIManager.Instance.Find<UIFramePet>().OnClickClose();
                                                  }, null);
        SummonButton.interactable = false;
    }

    public override void OnClickConfirm()
    {
        if (CurSelectedData.type != E_PetChangeViewType.Pet)
        {
            ZLog.LogError(ZLogChannel.Pet, "들어오면 안됨!!! NULL!!");
            return;
        }

        if (CurSelectedData.petData.PetType != E_PetType.Pet)
            return;

        ReqSummonPet();

        SummonButton.interactable = false;
    }

    protected override S_PCRResourceData GetResourceData()
    {
        Pet_Table data = CurSelectedData.petData;
        return new S_PCRResourceData() { FileName = data.ResourceFile, ViewScale = data.ViewScale, ViewPosY = data.ViewScaleLocY, Grade = data.Grade, ViewRotY = 0f };
    }

}
