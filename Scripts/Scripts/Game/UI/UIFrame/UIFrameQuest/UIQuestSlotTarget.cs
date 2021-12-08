using GameDB;
using System.Text;
using UnityEngine;
public class UIQuestSlotTarget : CUGUIWidgetBase
{
    [SerializeField] private ZText      TargetText = null;
    [SerializeField] private ZImage     MarkerCompass = null;
    [SerializeField] private ZImage     MarkerDot = null;

    private UIQuestScrollBase.SQuestInfo mQuestInfo = null;
    private bool            mQuestComplete = false;
    private StringBuilder    mNote = new StringBuilder();
    //----------------------------------------------------------
    public void DoUISlotTarget(UIQuestScrollBase.SQuestInfo _questTable, bool _complete)
	{
        mQuestInfo = _questTable;
        mQuestComplete = _complete;
        SwitchCompleteTextColor(mQuestComplete);
        UpdateQuestTarget();
	}

    public void UpdateQuestTarget()
    {
        if (mQuestInfo == null) return;

        string description = CManagerUIPresetBase.Instance.GetUIPresetLocalizingText(mQuestInfo.QuestTable.QuestSimpleText);

        switch (mQuestInfo.QuestTable.CompleteCheck)
		{
            case E_CompleteCheck.MonsterKill:
                UpdateMonsterKill(description);
                break;
            case E_CompleteCheck.GetItem:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.GetObject:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.DeliveryItem:
                UpdateDeliveryItem(description);
                break;
            case E_CompleteCheck.Level:
                UpdateLevel(description);
                break;
            case E_CompleteCheck.MapMove:
                UpdateCommon(description);
                SwitchCompass(true);
                break;
            case E_CompleteCheck.MapPos:
                UpdateCommon(description);
                SwitchCompass(true);
                break;
            case E_CompleteCheck.NPCTalk:
                UpdateCommon(description);
                break;
            case E_CompleteCheck.EquipEnchant:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.EquipUpgrade:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.ChangeBuy:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.ChangeCompose:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.PetBuy:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.PetCompose:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.RideBuy:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.RideCompose:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.Tutorial:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.LevelPer:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.EnterTemple:
                UpdateCommonCount(description);
                break;
            case E_CompleteCheck.ClearTemple:
                UpdateCommonCount(description);
                break;
		}
    }
    //-----------------------------------------------------------
    private void SwitchCompleteTextColor(bool _complete)
	{
        if (_complete)
		{
            TargetText.color = new Color(0.32f, 0.33f, 0.36f, 1f);
		}
        else
		{
            TargetText.color = new Color(0.7333f, 0.72f, 0.66f, 1f);
        }
	}

    private void SwitchCompass(bool _show)
	{
        if (_show)
		{
            MarkerCompass.gameObject.SetActive(true);
            MarkerDot.gameObject.SetActive(false);
        }
        else
		{
            MarkerCompass.gameObject.SetActive(false);
            MarkerDot.gameObject.SetActive(true);
        }
    }
    private string ExtractCountText(ulong _current, ulong _max, bool _complete)
	{
        string result;

        if (_max == 0)
		{
            result = "";
		}
        else
		{
            if (_complete)
            {
                result = string.Format("( {0} / {1} )", _current, _max);
            }
            else
            {
                result = string.Format("<color=#ffffff>( {0} / {1} )</color>", _current, _max);
            }
        }
        return result;
	}
    //-------------------------------------------------------------------------
    private void UpdateCommon(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        TargetText.text = mNote.AppendFormat("{0}", _description).ToString();
    }

    private void UpdateCommonCount(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        ulong countCurrent = mQuestInfo.CountCurrent;
        ulong countMax = mQuestInfo.CountMax;
        string countText = ExtractCountText(countCurrent, countMax, mQuestComplete);
        TargetText.text = mNote.AppendFormat("{0} {1}", _description, countText).ToString();
    }

    private void UpdateMonsterKill(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        ulong countCurrent = mQuestInfo.CountCurrent; 
        ulong countMax = mQuestInfo.QuestTable.TargetCount;
        TargetText.text = mNote.AppendFormat("{0} {1}", _description, ExtractCountText(countCurrent, countMax, mQuestComplete)).ToString();
	}

    private void UpdateGetItem(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        ulong countCurrent = mQuestInfo.CountCurrent;
        ulong countMax = mQuestInfo.CountMax;
        string countText = ExtractCountText(countCurrent, countMax, mQuestComplete);
        TargetText.text = mNote.AppendFormat("{0} {1}", _description, countText).ToString();
    }

    private void UpdateDeliveryItem(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        ulong countCurrent = mQuestInfo.CountCurrent;
        ulong countMax = mQuestInfo.CountMax;
        string countText = ExtractCountText(countCurrent, countMax, mQuestComplete);
        TargetText.text = mNote.AppendFormat("{0} {1}", _description, countText).ToString();
    }

    private void UpdateLevel(string _description)
	{
        SwitchCompass(false);
        mNote.Clear();
        ulong countCurrent = mQuestInfo.CountCurrent;
        ulong countMax = mQuestInfo.CountMax;
        string countText = ExtractCountText(countCurrent, countMax, mQuestComplete);
        TargetText.text = mNote.AppendFormat("{0} {1}", _description, countText).ToString();
    }

 

   

}
