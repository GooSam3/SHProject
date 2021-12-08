using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameDB;

public class UIQuestDialogNPCAccept : CUGUIWidgetBase
{
	[SerializeField] ZRawImage NPCImage;
	[SerializeField] ZText		NPCName;
	[SerializeField] ZText		DialogText;
	[SerializeField] UIQuestScrollDialogAccept ScrollQuestList;
	[SerializeField] CAnimationController DirectionAni = null;

	protected UIFrameQuest mUIFrameQuest = null;
	//------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
	}

	protected override void OnUIWidgetFrameShowHide(bool _show)
	{
		base.OnUIWidgetFrameShowHide(_show);
	}


	//------------------------------------------------------------------
	public void DoDialogNPCAccept(uint _stageTID, uint _npcTID,  List<UIQuestScrollBase.SQuestInfo> _listQuest)
	{
		NPC_Table npcTable = null;
		DBNpc.TryGet(_npcTID, out npcTable);

		if (npcTable == null) return;

		SetDialogNPCData(npcTable);
		ScrollQuestList.DoDialogAccept(_listQuest, this);
		AnimationAppear();
	}

	//------------------------------------------------------------------
	private void SetDialogNPCData(NPC_Table _npcTable)
	{
		if (_npcTable.QuestResource != null)
		{
			NPCImage.LoadTexture(_npcTable.QuestResource);
		}

		NPCName.text = _npcTable.NPCTextID;
	}


	private void AnimationAppear()
	{
		DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Accept_Appear", (_aniName) =>
		{

		});
	}


	private void AnimationDisappear()
	{
		if (DirectionAni.IsPlaying) return;

		DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Accept_Appear", (_aniName) =>
		{
			mUIFrameQuest.Close();
		}, true, true);
	}

	//----------------------------------------------------------
	public void HandleDialogNPCAcceptClose()
	{
		AnimationDisappear();
	}

	public void HandleDialogNPCAcceptSelect(uint _selectAcceptQuest)
	{
		DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Accept_Disappear", (_aniName) =>
		{
			mUIFrameQuest.DoUIQuestDialog(_selectAcceptQuest, UIQuestDialog.E_DialogType.Accept);
		});
	}
}
