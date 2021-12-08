using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIMailboxMailListItem : BaseItemViewsHolder
{
	#region PathVariable
	private UIMailItemSlot ItemSlot;
	private Text RemainTime;
	private Text TitleTxt;
	private Text Content;
	private Text ItemCount;
	private Button ReceiveButton;
	private Image GradeBorad;
	#endregion

	#region SystemVariable
	public ScrollMailData MailData;
	#endregion

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("Mail_Slot/UIMailItemSlot", out ItemSlot);
		root.GetComponentAtPath("Mail_Slot/Txt_RemainTime", out RemainTime);
		root.GetComponentAtPath("Mail_Slot/Txt_MainScript", out TitleTxt);
		root.GetComponentAtPath("Mail_Slot/Txt_SubScript", out Content);
		//root.GetComponentAtPath("Mail_Slot/Txt_RemainTime (1)", out Content);
		root.GetComponentAtPath("Mail_Slot/UIMailItemSlot/ItemSlot_Mail/ItemSlot_Lite_Parts/Txt_Num/Txt_Num", out ItemCount);
		root.GetComponentAtPath("Mail_Slot/Bt_Nomal_Blue_Light", out ReceiveButton);
		root.GetComponentAtPath("Mail_Slot/UIMailItemSlot/ItemSlot_Mail/ItemSlot_Share_Parts/Grade_Board", out GradeBorad);
		ReceiveButton.onClick.AddListener(Receive);
	}

	public void UpdateTitleByItemIndex(ScrollMailData model)
	{
		MailData = model;
		ItemSlot.Initialize(MailData.MailData.ItemTid, MailData.MailData.Cnt);

		if(MailData.MailData.ExpireDt > TimeManager.NowSec)
			RemainTime.text = "<color=#aaaaaa>남은 시간</color>\n" + TimeHelper.GetRemainTime(MailData.MailData.ExpireDt - TimeManager.NowSec);
		else
			RemainTime.text = "<color=#aaaaaa>남은 시간</color>\n" + "-";
		var mailType = DBMail.GetMailData((GameDB.E_MailType)MailData.MailData.Type);
		//Content.text = "[" + DBLocale.GetText(mailType.TitleTextID) + "]" + MailData.MailData.Title;
		TitleTxt.text = MailData.MailData.Title;
		Content.text = "Content";
		ItemCount.text = MailData.MailData.Cnt.ToString();

		// 등급 없는 아이템 들이 있어서 임시로 작업 추후 수정(테이블 작업 후).
		if (UICommon.GetItemGradeSprite(MailData.MailData.ItemTid) == null)
			GradeBorad.gameObject.SetActive(false);
		else
		{
			GradeBorad.sprite = UICommon.GetItemGradeSprite(MailData.MailData.ItemTid);
			GradeBorad.gameObject.SetActive(true);
		}
	}

	/// <summary>받기 버튼 클백 </summary>
	public void Receive()
	{
		ZWebManager.Instance.WebGame.REQ_TakeMailItem(MailData.MailData.mailType, MailData.MailData.MailIdx, MailData.MailData.ItemTid, MailData.MailData.Cnt, (recvPacket, msg) => {
			UIManager.Instance.Find<UIFrameMailbox>().RemoveMail(MailData.MailData.MailIdx);

			List<GainInfo> gainItemList = new List<GainInfo>();
			for (int i = 0; i < msg.TakeMailInfosLength; i++)
				gainItemList.Add(new GainInfo(msg.TakeMailInfos(i).Value));

			UIManager.Instance.Open<UIFrameItemRewardShot>((str, frame) =>
			{
				frame.AddItem(gainItemList);
			});
		});
	}
}