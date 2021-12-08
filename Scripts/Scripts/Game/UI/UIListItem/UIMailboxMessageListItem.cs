using Com.TheFallenGames.OSA.Core;
using frame8.Logic.Misc.Other.Extensions;
using UnityEngine.UI;

public class UIMailboxMessageListItem : BaseItemViewsHolder
{
	private Text RemainTime;
	private Text PaperType;
	private Text Title;
	private Text Sender;
	private ZButton ViewButton;
	private ScrollMailMessageData MessageData;
	private ulong MessageIndx;
	private ZImage AlarmDot;

	public override void CollectViews()
	{
		base.CollectViews();

		root.GetComponentAtPath("Mail_Paper_Slot/Txt_RemainTime", out RemainTime);
		root.GetComponentAtPath("Mail_Paper_Slot/Txt_SubScript", out PaperType);
		root.GetComponentAtPath("Mail_Paper_Slot/Txt_MainScript", out Title);
		root.GetComponentAtPath("Mail_Paper_Slot/Txt_Sender", out Sender);
		root.GetComponentAtPath("Mail_Paper_Slot/Bt_View", out ViewButton);
		root.GetComponentAtPath("Mail_Paper_Slot/Alarm_RedDot/Img", out AlarmDot);

		ViewButton.onClick.AddListener(View);
	}

	public void UpdateTitleByItemIndex(ScrollMailMessageData model)
	{
		MessageData = model;
		//TimeManager.NowSec - DeleteDt + DBConfig.Char_Delete_Time
		if(MessageData.messageData.ExpireDt > TimeManager.NowSec)
			RemainTime.text = "<color=#aaaaaa>남은 시간</color>\n" + TimeHelper.GetRemainTime(MessageData.messageData.ExpireDt - TimeManager.NowSec);
		else
			RemainTime.text = "<color=#aaaaaa>남은 시간</color>\n" + "-";

		switch (MessageData.messageData.Type)
		{
			case (uint)WebNet.E_MessageType.Normal:
				PaperType.text = "<color=#64DAFF>" + "일반 쪽지" + "</color>";
				break;
			case (uint)WebNet.E_MessageType.Guild:
				PaperType.text = "<color=#64DAFF>" + "길드 쪽지" + "</color>";
				break;
		}
		Title.text = MessageData.messageData.Title;
		Sender.text = MessageData.messageData.SenderCharNick;
		MessageIndx = MessageData.messageData.MessageIdx;

		if (MessageData.messageData.IsRead == 0)
			AlarmDot.gameObject.SetActive(true);
		else
			AlarmDot.gameObject.SetActive(false);
	}

	public void View()
	{
		UIManager.Instance.Find<UIFrameMailbox>().OpenMessage(RemainTime.text, PaperType.text, Title.text, Sender.text, MessageData.messageData.Message, MessageIndx);
	}
}