using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIQuestScrollDialogAcceptItem : CUGUIWidgetSlotItemBase
{
    [SerializeField] ZImage QuestIcon = null;
    [SerializeField] ZText QuestType = null;
    [SerializeField] ZText QuestName = null;

    private UIQuestScrollBase.SQuestInfo mQuestInfo = null;
	private UIFrameQuest mUIFrameQuest = null;
	private UIQuestDialogNPCAccept mDialogOwner = null;
	//------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
	}

	//------------------------------------------------------------
	public void DoDialogAcceptItem(UIQuestScrollBase.SQuestInfo _questInfo, UIQuestDialogNPCAccept _dialogOwner)
	{
        ZManagerUIPreset.Instance.SetSprite(QuestIcon, _questInfo.QuestTable.ChapterIcon);
        QuestName.text = _questInfo.QuestTable.QuestTextID;
		mQuestInfo = _questInfo;
		mDialogOwner = _dialogOwner;
	}

    //------------------------------------------------------------
    public void HandleAcceptQuest()
	{
		mDialogOwner.HandleDialogNPCAcceptSelect(mQuestInfo.QuestTID);
	}
}
