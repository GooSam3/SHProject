using ZDefine;

public class PCContentReplaceChange : PCContentReplaceBase
{
    protected override void InitializeList()
    {
        base.InitializeList();
        scrollReplace.ResetDataList(E_PetChangeViewType.Change);
    }

    protected override void OnConfirm(GachaKeepData data)
    {
        UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("WChangePet_CardChangeP_Confirm"), () =>
        {
            ZWebManager.Instance.WebGame.REQ_ChangeReOpenSelect(data.Id, data.Tid, (recvPacket, recvMsgPacket) =>
            {                
                InitializeList();
            });
        }, () => { });
    }

	protected override void OnReplace(GachaKeepData data)
    {
        var resultList = DBChange.GetChangeDatas(DBChange.GetChangeGrade(data.Tid));

        string resultStr = DBLocale.GetText("PCR_Replace_ReslutList");

        foreach (var iter in resultList)
        {
            resultStr += $"\n{DBUIResouce.GetPetGradeFormat(DBLocale.GetText(iter.ChangeTextID), iter.Grade)}";
        }

        UIMessagePopup.ShowCostPopup(DBLocale.GetText("Text_Button_ReGacha"), resultStr,
            DBConfig.Diamond_ID, DBConfig.CardChange_Diamond,
            () =>
            {
                if (ConditionHelper.CheckCompareCost(DBConfig.Diamond_ID, DBConfig.CardChange_Diamond) == false)
                    return;
               
                ZWebManager.Instance.WebGame.REQ_ChangeReOpen(data.Id, data.Tid, (recvPacket, recvMsgPacket) =>
                {
                    InitializeList();
                },(t,r,rm)=> { print(rm.ErrCode); });
            });

    }
}
