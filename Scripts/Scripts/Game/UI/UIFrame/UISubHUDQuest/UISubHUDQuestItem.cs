using GameDB;
using UnityEngine;
using System;

public class UISubHUDQuestItem : CUGUIWidgetBase
{
    [SerializeField] ZButton        QuestMove;
    [SerializeField] ZImage         QuestIcon;
    [SerializeField] ZText          QuestTitle;
    [SerializeField] ZText          QuestDesc;
    [SerializeField] GameObject      QuestReward;
    [SerializeField] GameObject      AutoPilot;

    private uint mQuestTID;
    private bool mAutoPiolot = false;
    private Quest_Table mQuestTable = null;
    private UIFrameQuest.E_QuestCategory mQuestCategory = UIFrameQuest.E_QuestCategory.Main;

    private Action EventClickButtonQuestOpen;

    //---------------------------------------------------------------
    public void DoSubHudQuestItemSetting(uint _questTID, UIFrameQuest.E_QuestCategory _questCategory, ulong _currentCount, ulong _maxCount,  bool _reward)
	{
        mQuestTID = _questTID;
        mQuestCategory = _questCategory;
        mQuestTable = DBQuest.GetQuestData(_questTID);

        if (mQuestTable == null) return;

        Sprite iconSprite = ZManagerUIPreset.Instance.GetSprite(mQuestTable.ChapterIcon);
        if (iconSprite)
		{
            QuestIcon.sprite = iconSprite;
        }

        ProcessQuestTitleColor(mQuestTable.QuestTextID, mQuestTable.UIQuestType);

        string description = CManagerUIPresetBase.Instance.GetUIPresetLocalizingText(mQuestTable.QuestSimpleText);
        if (_maxCount == 0)
        {
            QuestDesc.text = description;
        }
        else
		{
            QuestDesc.text = string.Format("{0} ({1}/{2})", description, _currentCount, _maxCount);
        }

        if (_reward)
		{
            QuestReward.SetActive(true);
		}
        else
		{
            QuestReward.SetActive(false);
		}

        if (mQuestTable.UIShortCut == E_UIShortCut.Warp)
		{
            QuestMove.interactable = true;
		}
        else
		{
            QuestMove.interactable = false;
        }
    }

    public void DoSubHudQuestItemAutoPilot(bool _start)
	{
        mAutoPiolot = _start;
        AutoPilot.SetActive(_start);
    }

    public uint GetQuestItemTID()
	{
        return mQuestTID;
	}

    //--------------------------------------------------------------------------------------
    private void ProcessQuestTitleColor(string _questTitleText, E_UIQuestType _questType)
	{
        QuestTitle.text = _questTitleText;
        if (_questType == E_UIQuestType.Main)
        {
            QuestTitle.color = ResourceSetManager.Instance.SettingRes.Palette.MainQuest;
        }
        else if (_questType == E_UIQuestType.Sub)
        {
            QuestTitle.color = ResourceSetManager.Instance.SettingRes.Palette.SubQuest;
        }
        else if (_questType == E_UIQuestType.Temple)
		{
            QuestTitle.color = ResourceSetManager.Instance.SettingRes.Palette.TempleQuest;
		}
        else if (_questType == E_UIQuestType.Daily)
		{
            QuestTitle.color = ResourceSetManager.Instance.SettingRes.Palette.DailyQuest;
        }
        else if (_questType == E_UIQuestType.Achievement)
        {
            QuestTitle.color = ResourceSetManager.Instance.SettingRes.Palette.AchivementQuest;
        }
    }

    //---------------------------------------------------------------
    public void HandleButtonQuestOpen()
	{
        //튜토리얼 예외처리        
        if (null != EventClickButtonQuestOpen)
        {
            EventClickButtonQuestOpen.Invoke();
            return;
        }
        else if (null != mQuestTable && mQuestTable.CompleteCheck == E_CompleteCheck.Tutorial)
        {
            return;
		}


        if (QuestReward.activeSelf) // 퀘스트가 완료 상태일 경우 
		{
            UIFrameQuest.Instance.DoUIQuestDialog(mQuestTID, UIQuestDialog.E_DialogType.Reward);
        }
        else
		{
            if (mAutoPiolot)
			{
                UIFrameQuest.Instance.QuestAutoPilot.EventAutoPilotStop(mQuestTID);
            }
            else
			{
                UIFrameQuest.Instance.QuestAutoPilot.EventQuestShortCut(mQuestTID);
            }
        }
	}

    public void HandleButtonQuestMove()
	{
        UIFrameQuest.Instance.QuestAutoPilot.EventWarpAutoPilotStart(mQuestTID);
    } 

    public void DoSetEventClickButtonQuestOpen(Action action)
	{
        EventClickButtonQuestOpen = action;
    }

    public void DoResetEventClickButtonQuestOpen()
	{
        EventClickButtonQuestOpen = null;
    }
}
