using GameDB;
using UnityEngine;
public class UIQuestDescription : CUGUIWidgetBase
{
	[SerializeField] private ZText		 Title = null;
	[SerializeField] private ZText		 Description = null;
	[SerializeField] private ZImage		 QuestIcon = null;
	[SerializeField] private GameObject  Complete = null;
	[SerializeField] private ZButton	 ButtonDelete = null;
	[SerializeField] private GameObject  DeleteConfirm = null;
	[SerializeField] private ZText		 DeleteTitle = null;

	private uint		mQuestTID = 0;
	private bool		mSendPacket = false;
	private string	mQuestName = null;
	private E_UIQuestType mUIQuestType = E_UIQuestType.None;
	//---------------------------------------------------------
	public void DoUIQuestDescription(Quest_Table _tableInfo, bool _complete)
	{
		SetMonoActive(true);

		mQuestTID = _tableInfo.QuestID;
		mQuestName = _tableInfo.QuestTextID;
		mUIQuestType = _tableInfo.UIQuestType;

		Title.text = _tableInfo.QuestTextID;
		Description.text = _tableInfo.QuestTermsText;

		Complete.SetActive(_complete);
		Sprite spriteIcon = ZManagerUIPreset.Instance.GetSprite(_tableInfo.ChapterIcon);
		if (spriteIcon)
		{
			QuestIcon.sprite = spriteIcon;
		}
		else
		{
			ZManagerUIPreset.Instance.SetSprite(QuestIcon, "icon_hud_quest");
		}

		if ((_tableInfo.UIQuestType == E_UIQuestType.Sub || _tableInfo.UIQuestType == E_UIQuestType.Temple) && _complete == false && _tableInfo.QuestOpenType != E_QuestOpenType.CreateCharacter)
		{
			ButtonDelete.gameObject.SetActive(true);
		}
		else
		{
			ButtonDelete.gameObject.SetActive(false);
		}
	}

	//-------------------------------------------------------------
	public void HandleQuestDeleteQuestion()
	{
		DeleteConfirm.SetActive(true);
		DeleteTitle.text = mQuestName;
	}

	public void HandleQuestDeleteConfirm()
	{
		if (mSendPacket) return;
		mSendPacket = true;
		ZWebManager.Instance.WebGame.REQ_QuestCancle(mQuestTID, (questTID) => {
			mSendPacket = false;
			DeleteConfirm.SetActive(false);
			UIFrameQuest uiFrameQuest = mUIFrameParent as UIFrameQuest;

			if (mUIQuestType == E_UIQuestType.Sub)
			{
				uiFrameQuest.DoUIQuestPanelSub(0);
			}
			else if (mUIQuestType == E_UIQuestType.Temple)
			{
				uiFrameQuest.DoUIQuestPanel(UIFrameQuest.E_QuestCategory.Temple);
			}
		});
	}

	public void HandleQuestDeleteCancle()
	{
		DeleteConfirm.SetActive(false);
	}
}
