using System.Collections.Generic;
using ZNet.Data;

public class PCContentCombineChange : PCContentCombineBase
{
    // 소유중인, 고대가 아닌
    protected override void InitilaizeList()
    {
        ListContentData.Clear();

        foreach (var iter in Me.CurCharData.GetChangeDataList())
        {
            // 한장만 있다면 ㄴ
            if (iter.Cnt < 1)
                continue;

            if (DBChange.TryGet(iter.ChangeTid, out var table) == false)
                continue;

            if (table.ViewType != GameDB.E_ViewType.View)
                continue;

            if (table.Grade >= ZUIConstant.GRADE_MAX)
                continue;

            if (iter.IsLock)
                continue;


            ListContentData.Add(new C_PetChangeData(iter, (int)iter.Cnt));
        }
    }

    public override void SetSortedContent(E_PetChangeSortType sortType)
    {
        base.SetSortedContent(sortType);

        bool isViewNotice = (sortType == E_PetChangeSortType.Tier_6 || ListSortData.Count == 0);

        TextNotice.gameObject.SetActive(isViewNotice);

        if (isViewNotice)
        {
            if (sortType == E_PetChangeSortType.Tier_6)
            {
                TextNotice.text = DBLocale.GetText("WChange_ChangeCompose_6T");
            }
            else
            {
                TextNotice.text = DBLocale.GetText("WChangePet_PetCompose_Noting");
            }
        }
    }

    protected override long GetCombineCost(int multi) => DBCompose.GetChangeComposeCost((byte)RegistedGrade) * multi;

    protected override int GetCombineCount() => DBCompose.GetChangeComposeCount((byte)RegistedGrade);

    public override void OnClickCombine()
    {
        if (DBCompose.GetChangeComposeTable((byte)RegistedGrade, out var table) == false)
            return;

        int count = CombineList.TotalCount;

        if (count % GetCombineCount() != 0)
            return;

        count /= GetCombineCount();

        if ((long)Me.CurCharData.GetItem(DBConfig.Gold_ID, NetItemType.TYPE_ACCOUNT_STACK).cnt < GetCombineCost(count))
        {
            UIMessagePopup.ShowPopupOk("MessageS_Tooltip_NoSiver");
            return;
        }

        if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
            gainSystem.SetPlayState(false);

        ZWebManager.Instance.WebGame.REQ_ComposeChange(table.ChangeComposeID, (uint)count, CombineList.listCombineData, (recvPacket, recvMsgPacket) =>
        {
            // 목록 동기화해주고 
            ReloadListData();

            //첨으로돌려버리장
            ShowContent();

            List<uint> listcompose = new List<uint>();
            for (int i = 0; i < recvMsgPacket.ResultComposeLength; i++)
            {
                listcompose.Add(recvMsgPacket.ResultCompose(i).Value.ChangeTid);
            }

            if (listcompose.Count <= 0)
            {
                UIMessagePopup.ShowPopupOk(DBLocale.GetText("PCR_Notice_NoCombineResult"));
                return;
            }
            else
            {
                UIManager.Instance.Open<UIPopupGachaResult>((str, frame) =>
                {
                    UIManager.Instance.Find<UIFrameChange>()?.SetReserveContent(UIFramePetChangeBase.E_PetChangeContentType.Content_2);
                    frame.SetCombineResult(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_TimeLineType.Class_10_Start, listcompose);
                });
                return;
            }
        });
    }
}
