using UnityEngine;
using UnityEngine.UI;
public class UIQuestSubSlotItem : CUGUIWidgetBase
{
	[SerializeField] private Image			DecoLineTop = null;
	[SerializeField] private Image			DecoLineDown = null;
	[SerializeField] private ZImage			ClearCheck = null;
	[SerializeField] private ZImage			QuestIcon = null;
	[SerializeField] private GameObject		Select = null;
	[SerializeField] private ZText			QuestTitle = null;
	[SerializeField] private ZImage			Alram = null;
	[SerializeField] private ZImage			QuestCurrent = null;
	[SerializeField] private ZButton		SelectButton = null;

	private uint mQuestTID = 0; public uint GetQuestTID { get { return mQuestTID; } }
	public bool IsAlarm { get { return Alram.gameObject.activeSelf; } }
	private UIFrameQuest					mUIFrameQuest = null;
	private UIQuestScrollItemBase			mParentsItem = null;
	private UIQuestScrollBase.SQuestInfo	mQuestInfo = null;
	private UIQuestScrollBase.SQuestInfo	mQuestLink = null;
	//--------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameQuest = _UIFrameParent as UIFrameQuest;
	}

	//---------------------------------------------------------
	public void DoSubSlotQuestInitialize(UIQuestScrollBase.SQuestInfo _questInfo, UIQuestScrollItemBase _parentItem, bool _top, bool _down)
	{
		mQuestInfo = _questInfo;
		mParentsItem = _parentItem;

		mQuestTID = _questInfo.QuestTID;
		QuestTitle.text = _questInfo.QuestTable.QuestTextID;
		DecoLineTop.gameObject.SetActive(_top);
		DecoLineDown.gameObject.SetActive(_down);
		ClearCheck.gameObject.SetActive(_questInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete);
		QuestCurrent.gameObject.SetActive(_questInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Progress);
		Select.SetActive(false);
		Alram.gameObject.SetActive(false);

		Sprite questIcon = ZManagerUIPreset.Instance.GetSprite(_questInfo.QuestTable.ChapterIcon);
		if (questIcon)
		{
			QuestIcon.sprite = questIcon;
		}

		gameObject.SetActive(false);
	}

	public void DoSubSlotQuestSelect(bool _select)
	{
		if (mParentsItem)
		{
			Select.SetActive(_select);
			if (_select)
			{
				mParentsItem.DoQuestScrollSelectSlotItem(this,  mQuestLink == null ? mQuestInfo : mQuestLink);
				SelectButton.Select();
				SelectButton.OnSelect(null);
			}
			else
			{
				SelectButton.OnDeselect(null);
			}
			
		}
	}

	public void DoSubSlotShowHide(bool _show)
	{		
		if (mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Reward)
		{
			Alram.gameObject.SetActive(true);
		}
		else
		{
			Alram.gameObject.SetActive(false); 
		}

		if (_show)
		{
			ClearCheck.gameObject.SetActive(false);
			if (mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Hide)
			{
				if (mQuestLink != null)
				{
					DoSubSlotQuestSelect(true);
					SetMonoActive(true);
				}
				else
				{
					SetMonoActive(false);
					DoSubSlotQuestSelect(false);
				}
			}
			else if (mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Progress || mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Confirm || mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Reward)
			{				
				DoSubSlotQuestSelect(true);
				SetMonoActive(true);
			}
			else if (mQuestInfo.QuestState == UIQuestScrollBase.E_QuestUIState.Complete)
			{
				SetMonoActive(true);
				DoSubSlotQuestSelect(false);
				ClearCheck.gameObject.SetActive(true);
			}
			else 
			{				
				SetMonoActive(true);
				DoSubSlotQuestSelect(false);
			}
		}
		else
		{
			SetMonoActive(false);
			DoSubSlotQuestSelect(false);
		}
	}

	public UIQuestScrollBase.E_QuestUIState GetSubSlotItemState()
	{
		return mQuestInfo.QuestState;
	}

	public void SetSubSlotLinkQuest(UIQuestScrollBase.SQuestInfo _questLinkInfo)
	{
		mQuestLink = _questLinkInfo;
	}


	//--------------------------------------------------------
	public void HandleSubSlotItemSelect()
	{
		DoSubSlotQuestSelect(true);		
	}
}
