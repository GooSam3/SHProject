using GameDB;
using UnityEngine;

public class UIQuestReward : CUGUIWidgetBase
{
	[SerializeField] private UIQuestSlotTarget		TargetItem = null;
	[SerializeField] private UIQuestScrollReward		ScrollReward = null;
	[SerializeField] private UIQuestScrollReward		ScrollRandomReward = null;
	[SerializeField] private ZButton				ButtonComplete = null;
	[SerializeField] private ZButton				ButtonWalk = null;
	[SerializeField] private ZButton				ButtonWarp = null;
	[SerializeField] private ZButton				ButtonShortCut = null;

	[SerializeField] private GameObject				ActionStart = null;
	[SerializeField] private GameObject				ActionProgress = null;
	[SerializeField] private GameObject				ActionComplete = null;

	private UIQuestScrollBase.SQuestInfo mQuestInfo = null;
	private UIFrameQuest mUIFrameQuest = null;
	//------------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
		ButtonWalk.gameObject.SetActive(false);
	}
	//------------------------------------------------------------------
	public void DoUIQuestReward(UIQuestScrollBase.SQuestInfo _quesInfo)
	{
		SetMonoActive(true);
		mQuestInfo = _quesInfo;
		MakeScrollRectItem(_quesInfo.QuestMajor == null ? _quesInfo : _quesInfo.QuestMajor);
		RefreshQuestTarget(_quesInfo);
	} 

	public void DoUIQuestRewardRefresh(UIQuestScrollBase.SQuestInfo _quesInfo)
	{
		if (mQuestInfo == null) return;

		if (mQuestInfo.QuestTID == _quesInfo.QuestTID) 
		{
			RefreshQuestTarget(_quesInfo);
		}
	}

	//-------------------------------------------------------------------------
	private void MakeScrollRectItem(UIQuestScrollBase.SQuestInfo _quesInfo)
	{
		if (_quesInfo.QuestTable.UIQuestType == E_UIQuestType.Main)
		{
			ScrollReward.DoUIQuestScrollReward(_quesInfo.QuestTable, UIQuestScrollReward.E_UIRewardType.Main, _quesInfo.QuestState != UIQuestScrollBase.E_QuestUIState.Complete);
			ScrollRandomReward.SetMonoActive(false);
		}
		else if (_quesInfo.QuestTable.UIQuestType == E_UIQuestType.Sub)
		{
			ScrollReward.DoUIQuestScrollReward(_quesInfo.QuestTable, UIQuestScrollReward.E_UIRewardType.Sub, _quesInfo.QuestState != UIQuestScrollBase.E_QuestUIState.Complete);
			ScrollRandomReward.DoUIQuestScrollReward(_quesInfo.QuestTable, UIQuestScrollReward.E_UIRewardType.Sub_Random_FrontSide, _quesInfo.QuestState != UIQuestScrollBase.E_QuestUIState.Complete);
			ScrollRandomReward.SetMonoActive(true);
		}
		else if (_quesInfo.QuestTable.UIQuestType == E_UIQuestType.Temple)
		{
			ScrollReward.DoUIQuestScrollReward(_quesInfo.QuestTable, UIQuestScrollReward.E_UIRewardType.Temple, _quesInfo.QuestState != UIQuestScrollBase.E_QuestUIState.Complete);
			ScrollRandomReward.SetMonoActive(false);
		}
		else if (_quesInfo.QuestTable.UIQuestType == E_UIQuestType.Achievement)
		{
			ScrollReward.DoUIQuestScrollReward(_quesInfo.QuestTable, UIQuestScrollReward.E_UIRewardType.Achieve, false);
			ScrollRandomReward.SetMonoActive(false);
		}

		if (_quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete)
		{
			ScrollReward.DoUIQuestScrollRewardItemCheck(true);
		}

	}

	private void RefreshQuestTarget(UIQuestScrollBase.SQuestInfo _quesInfo)
	{
		TargetItem.DoUISlotTarget(_quesInfo, _quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete);

		if (_quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Confirm)
		{
			ActionStart.SetActive(true);
			ActionProgress.SetActive(false);
			ActionComplete.SetActive(false);
		}
		else if (_quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Progress)
		{
			ActionStart.SetActive(false);
			ActionProgress.SetActive(true);
			ActionComplete.SetActive(false);

			ProcessMoveType();
		}
		else if (_quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Reward)
		{
			ActionStart.SetActive(false);
			ActionProgress.SetActive(false);
			ActionComplete.SetActive(true);
			ButtonComplete.interactable = true;			
		}
		else if (_quesInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete)
		{
			ActionStart.SetActive(false);
			ActionProgress.SetActive(false);
			ActionComplete.SetActive(true);
			ButtonComplete.interactable = false;
		}
		else
		{
			ActionStart.SetActive(false);
			ActionProgress.SetActive(false);
			ActionComplete.SetActive(false);
		}

	}

	private void ProcessMoveType() 
	{
		if (mQuestInfo.QuestTable.AutoProgressType == E_AutoProgressType.Auto)
		{

			if (mQuestInfo.QuestTable.UIShortCut != E_UIShortCut.None)
			{
				ButtonShortCut.gameObject.SetActive(false);
				ButtonWarp.gameObject.SetActive(true);

				Portal_Table portalTable = null;
				DBPortal.TryGet(mQuestInfo.QuestTable.QuestWarpPortalID, out portalTable);
				if (portalTable != null && portalTable.StageID == ZGameModeManager.Instance.StageTid)
				{
					ButtonWalk.gameObject.SetActive(true);
				}
				else
				{
					ButtonWalk.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			ButtonWalk.gameObject.SetActive(false);
			ButtonWarp.gameObject.SetActive(false);

			if (mQuestInfo.QuestTable.UIShortCut == E_UIShortCut.None)
			{
				ButtonShortCut.gameObject.SetActive(false);
			}
			else
			{
				ButtonShortCut.gameObject.SetActive(true);
			}
		}		
	}

	//--------------------------------------------------------------------------
	public void HandleQuestAccept()
	{		
		mUIFrameQuest.DoUIQuestDialog(mQuestInfo.QuestTID, UIQuestDialog.E_DialogType.Confirm);
	}

	public void HandleQuestShortCut()
	{
		mUIFrameQuest.QuestAutoPilot.EventQuestShortCut(mQuestInfo.QuestTID);
		mUIFrameQuest.Close();
	}

	public void HandleQuestRequestReward()
	{
		// 다이얼로그를 출력하고 완료를 눌러야 보상 요청을 한다. 
		mUIFrameQuest.DoUIQuestDialog(mQuestInfo.QuestTID, UIQuestDialog.E_DialogType.Reward);
	}

	public void HandleQuestMoveWalk()
	{
		mUIFrameQuest.DoUIQuestMove(mQuestInfo.QuestTable, true);
	}

	public void HandleQuestMoveWarp()
	{
		mUIFrameQuest.DoUIQuestMove(mQuestInfo.QuestTable, false);
	}

}
