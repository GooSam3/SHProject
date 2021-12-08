using GameDB;
using System.Collections.Generic;
using UnityEngine;

public class UIQuestDialog : CUGUIWidgetBase
{
	public enum E_DialogType
	{
		Accept,           // 퀘스트 받을때 
		Confirm,          // 퀘스트 받은후
		ConfirmMain,		 //	
		Reward,			 // 퀘스트 완료후 			
	}

	public class SQuestDialogAnswerGroup
	{
		public uint DialogGroup = 0;
		public SortedList<uint, Dialogue_Table> listAnswer = new SortedList<uint, Dialogue_Table>(); 
	}

	[SerializeField] ZRawImage		Portrait = null;
	[SerializeField] ZText			TextTitle = null;
	[SerializeField] ZText			TextDesc = null;
	[SerializeField] ZText			TextTitleNoImg = null;
	[SerializeField] ZText			TextDescNoImg = null;
	[SerializeField] ZButton		ButtonSkip = null;
	[SerializeField] ZButton		ButtonNext = null;
	[SerializeField] GameObject    ProgressMark = null;
	[SerializeField] CAnimationController		DirectionAni = null;

	private uint			mRewardQuestTID = 0;
	private int			mSelectRewardIndex = -1;
	private bool			mCanNextDialog = true;
	private bool			mSkip = false;

	private Dialogue_Table	mDialogTable = null;	
	private UIFrameQuest	mUIFrameQuest = null;
	private E_DialogType	mDialogType = E_DialogType.Accept;
	private SQuestDialogAnswerGroup mCurrentDialogAnserGroup = null;
	private Dictionary<uint, SQuestDialogAnswerGroup> m_dicAnswerGroup = new Dictionary<uint, SQuestDialogAnswerGroup>();
	private List<UIQuestAnswerItem>					 m_listQuestAnswer = new List<UIQuestAnswerItem>();
	private List<uint>						     m_listSelectedAnsewerID = new List<uint>();
	//-------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
		GetComponentsInChildren<UIQuestAnswerItem>(true, m_listQuestAnswer);
		ShowChioceText(null);		
	}

	protected override void OnUIWidgetFrameShowHide(bool _show)
	{
		base.OnUIWidgetFrameShowHide(_show);
	}

	public void InitializeQuestDialog()
	{
		Dictionary<uint, Dialogue_Table>.Enumerator it = GameDBManager.Container.Dialogue_Table_data.GetEnumerator();

		while (it.MoveNext())
		{
			Dialogue_Table dialogTable = it.Current.Value;
			if (dialogTable.DialogueSelectGroup != 0)
			{
				SQuestDialogAnswerGroup answerGroup = FindOrAllocDialogAnswerGroup(dialogTable.DialogueSelectGroup);
				answerGroup.listAnswer.Add(it.Current.Value.SelectSequence, dialogTable);
			}
		}
	}

	//-------------------------------------------------------------
	public void DoUIQuestDialogStart(uint _rewardQuestID, E_DialogType _dialogType)
	{
		SetNextActionEnable(true);
		ShowChioceText(null);

		ButtonSkip.interactable = true;
		ButtonNext.interactable = true;

		m_listSelectedAnsewerID.Clear();
		mSkip = false;
	
		mRewardQuestTID = _rewardQuestID;
		mDialogType = _dialogType;

		Quest_Table questTable = DBQuest.GetQuestData(_rewardQuestID);
		if (questTable == null) return;

		uint dialogTID = ExtractQuestDialogTID(questTable, _dialogType);

		if (dialogTID == 0)
		{
			ProcessDialogEnd();
		}
		else
		{
			
			Dialogue_Table dialogTable = null;
			GameDBManager.Container.Dialogue_Table_data.TryGetValue(dialogTID, out dialogTable);
			if (dialogTable == null) return;

			ProcessDialog(dialogTable);
			AnimationAppear();
		}
	}

	//-------------------------------------------------------------
	private void ShowChioceText(SQuestDialogAnswerGroup _answerGroup)
	{
		if (_answerGroup == null)
		{
			for (int i = 0; i < m_listQuestAnswer.Count; i++)
			{
				m_listQuestAnswer[i].DoUIWidgetShowHide(false);
			}
			ProgressMark.SetActive(true);
		}
		else
		{
			for (int i = 0; i < m_listQuestAnswer.Count; i++)
			{
				if (i < _answerGroup.listAnswer.Values.Count)
				{
					Dialogue_Table dialogTable = _answerGroup.listAnswer.Values[i];
					bool alreadyAnswer = m_listSelectedAnsewerID.Contains(dialogTable.DialogueID);
					m_listQuestAnswer[i].DoUIWidgetShowHide(true);
					m_listQuestAnswer[i].DoQuestAnswerItem(_answerGroup.listAnswer.Values[i].DialogueSelectText, !alreadyAnswer);
				}
				else
				{
					m_listQuestAnswer[i].DoUIWidgetShowHide(false);
				}

			}

			ProgressMark.SetActive(false);
		}		
	}

	private void ProcessDialog(Dialogue_Table _dialogTable)
	{
		mDialogTable = _dialogTable;

		if (_dialogTable.DialogueType == E_DialogueType.SelectDialogue)
		{
			SetNextActionEnable(false);
			ProcessDialogChoiceAnswer(_dialogTable.DialogueSelectGroup);
			mSkip = false;
		}
		else if (_dialogTable.DialogueType == E_DialogueType.Dialogue)
		{
			ShowChioceText(null);

			if (mSkip)
			{
				ProcessDialogNext();
			}
			else
			{
				PrintSpearker(_dialogTable);
			}

			SetNextActionEnable(true);
		}
	}

	private void ProcessDialogNext()
	{
		uint nextID = mDialogTable.DialogueNextID;
		SetRewardSelectIndex(mDialogTable.SelectRewardOrder);

		if (nextID == 0)
		{
			AnimationDisappear();
		}
		else
		{
			Dialogue_Table dialogTable = null;
			GameDBManager.Container.Dialogue_Table_data.TryGetValue(nextID, out dialogTable);
			if (dialogTable == null)
			{
				ProcessDialogEnd();
			}
			else
			{
				ProcessDialog(dialogTable);
			}
		}
	}

	private void ProcessDialogEnd()
	{
		if (mDialogType == E_DialogType.ConfirmMain)
		{
			mUIFrameQuest.DoUIQuestRewardPopup(mRewardQuestTID, UIQuestAccepRewardPopup.E_RewardType.Confirm, mSelectRewardIndex);
		}		
		else if (mDialogType == E_DialogType.Confirm)
		{
			mUIFrameQuest.Close();			
		}
		else if (mDialogType == E_DialogType.Accept)
		{
			mUIFrameQuest.DoUIQuestRewardPopup(mRewardQuestTID, UIQuestAccepRewardPopup.E_RewardType.Accept, mSelectRewardIndex);			
		}
		else if (mDialogType == E_DialogType.Reward)
		{
			mUIFrameQuest.DoUIQuestRewardPopup(mRewardQuestTID, UIQuestAccepRewardPopup.E_RewardType.Reward, mSelectRewardIndex);
		}
	}

	private void ProcessDialogChoiceAnswer(uint _groupID)
	{
		mCurrentDialogAnserGroup = FindOrAllocDialogAnswerGroup(_groupID);

		if (mCurrentDialogAnserGroup == null) ZLog.LogError(ZLogChannel.UI, string.Format("[Quest] ===== Invalid DialogGroup : {0}", _groupID));

		ShowChioceText(mCurrentDialogAnserGroup);
	}

	private void AnimationAppear()
	{
		if (mDialogType != E_DialogType.Accept)
		{
			DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Appear_Solo", (_name) => {
			});
		}
		else
		{
			DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Appear", (_name) => {

			});
		}
	}

	private void AnimationDisappear()
	{
		if (gameObject.activeSelf == false)
		{
			ProcessDialogEnd();
		}
		else
		{
			ButtonSkip.interactable = false;
			ButtonNext.interactable = false;

			DirectionAni.DoAnimationPlay("Ani_Quest_Dialog_Disappear", (_name) => {
				ProcessDialogEnd();
			});
		}
	}

	private void PrintSpearker(Dialogue_Table _dialogTable)
	{
		if (_dialogTable.DialogueResource == null)
		{
			TextTitle.gameObject.SetActive(false);
			TextDesc.gameObject.SetActive(false);
			TextTitleNoImg.gameObject.SetActive(true);
			TextDescNoImg.gameObject.SetActive(true);
		
			Portrait.gameObject.SetActive(false);
			
			if (_dialogTable.DialogueNPCName != null)
				TextTitleNoImg.text = _dialogTable.DialogueNPCName;
			if (_dialogTable.DialogueText != null)
				TextDescNoImg.text = _dialogTable.DialogueText;
		}
		else
		{
			TextTitle.gameObject.SetActive(true);
			TextDesc.gameObject.SetActive(true);
			TextTitleNoImg.gameObject.SetActive(false);
			TextDescNoImg.gameObject.SetActive(false);
			
			Portrait.gameObject.SetActive(true);
			Portrait.LoadTexture(_dialogTable.DialogueResource);

			if (_dialogTable.DialogueNPCName != null)
				TextTitle.text = _dialogTable.DialogueNPCName;
			if (_dialogTable.DialogueText != null)
				TextDesc.text = _dialogTable.DialogueText;
		}
	}
	//--------------------------------------------------------------
	private SQuestDialogAnswerGroup FindOrAllocDialogAnswerGroup(uint _groupID)
	{
		SQuestDialogAnswerGroup findGroup = null;
		if (m_dicAnswerGroup.ContainsKey(_groupID))
		{
			findGroup = m_dicAnswerGroup[_groupID];
		}
		else
		{
			findGroup = new SQuestDialogAnswerGroup();
			findGroup.DialogGroup = _groupID;
			m_dicAnswerGroup.Add(_groupID, findGroup);
		}

		return findGroup;
	}

	private void SetRewardSelectIndex(uint _selectIndex)
	{
		if (_selectIndex != 0)
			mSelectRewardIndex = (int)(_selectIndex - 1); // 테이블의 0은 예약되어 있어서 1,2,3....이다. 
	}

	private void SetNextActionEnable(bool _enable)
	{
		mCanNextDialog = _enable;
		ButtonSkip.gameObject.SetActive(_enable);
		ButtonNext.gameObject.SetActive(_enable);
	}

	private uint ExtractQuestDialogTID(Quest_Table _questTable, E_DialogType _dialogType)
	{
		uint dialogTID = 0;
		if (_dialogType == E_DialogType.Accept)
		{
			dialogTID = _questTable.DialogueBeforeStartID;
		}
		else if (_dialogType == E_DialogType.Confirm || _dialogType == E_DialogType.ConfirmMain)
		{
			dialogTID = _questTable.DialogueAfterStartID;
		}
		else if (_dialogType == E_DialogType.Reward)
		{
			dialogTID = _questTable.DialogueEndID;
		}
		return dialogTID;
	}	

	//------------------------------------------------------------
	public void HandleDialogSkip()
	{
		mSkip = true;
		HandleDialogNext();
	}

	public void HandleDialogNext()
	{
		if (mCanNextDialog)
		{
			ProcessDialogNext();
		}
	}

	public void HandleDialogAnswer(int _index)
	{
		if (_index >= mCurrentDialogAnserGroup.listAnswer.Values.Count)
		{
			ZLog.LogError(ZLogChannel.UI, string.Format("[Quest] ===== Invalid Answer : {0}", _index));
			return;
		}

		mDialogTable = mCurrentDialogAnserGroup.listAnswer.Values[_index];
		m_listSelectedAnsewerID.Add(mDialogTable.DialogueID);
		ProcessDialogNext();
	}
}
