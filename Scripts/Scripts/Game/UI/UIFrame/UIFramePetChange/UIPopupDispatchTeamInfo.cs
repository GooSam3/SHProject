using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIPopupDispatchTeamInfo : MonoBehaviour
{
    [Serializable]
    public class DispatchTeamInfo
    {
        [SerializeField] private GameObject obj;
        [SerializeField] private Text title;
        [SerializeField] private Text gradeTxt;

        public void Set(ChangeQuestLevel_Table table)
        {
            obj.SetActive(table != null);
            if (table == null)
                return;

            title.text = DBLocale.GetText("Change_Dispatch_Team", table.ChangeQuestLevel);
            gradeTxt.text = DBLocale.GetText(table.LevelInfo);
        }
    }

    [SerializeField] private GameObject objNoticeLvUpTitle;
    [SerializeField] private Text txtNoticeLvUp;

    [SerializeField] private Text txtProgress;
    [SerializeField] private Slider sliderProgress;

    [SerializeField] private DispatchTeamInfo curTeamInfo;
    [SerializeField] private DispatchTeamInfo nextTeamInfo;

    [SerializeField] private ZButton btnTeamLvUp;

    private Action onClickLvUp;

    public void SetState(bool state, Action _onClickLvUp = null)
    {
        onClickLvUp = _onClickLvUp;

        if(state)
        {
            RefreshUI();
        }
        else
        {
            if (this.gameObject.activeSelf == false)
                return;
        }
        this.gameObject.SetActive(state);
    }

    private void RefreshUI()
    {
        var curQuestLevel = Me.CurUserData.ChangeQuestLv;
        var curExp = Me.CurUserData.ChangeQuestExp;

        var curLvTable = DBChangeQuest.GetChangeQuestLevelByLevel(curQuestLevel);
        var nextLvTable = DBChangeQuest.GetChangeQuestLevelByLevel(curQuestLevel + 1);

        bool hasNext = curLvTable.LevelUpType == E_LevelUpType.Up;

        curTeamInfo.Set(curLvTable);
        nextTeamInfo.Set(nextLvTable);

        objNoticeLvUpTitle.SetActive(hasNext);

        var curTotalExp = DBChangeQuest.GetChangeQuestExpByCurrentLevel(curQuestLevel);


        if (hasNext)
        {
            txtNoticeLvUp.text = DBLocale.GetText("ChangeQuest_LevelUp_Guide",DBLocale.GetGradeLocale(curLvTable.LevelUpGrade));
            curExp -= DBChangeQuest.GetChangeQuestExpByPreLevel(curQuestLevel);

            btnTeamLvUp.interactable = curExp >= curLvTable.LevelUpCount;
        }
        else//만렙
        {
            btnTeamLvUp.interactable = false;

            txtNoticeLvUp.text = DBLocale.GetText("ChangeQuest_LevelInfo");
            curExp = curTotalExp;
        }

        txtProgress.text = UICommon.GetProgressText((int)curExp, (int)curTotalExp, false);
        sliderProgress.value = (float)curExp/(float)curTotalExp;
    }

    public void OnClickTeamLevelUp()
	{
        onClickLvUp?.Invoke();
    }

    public void OnClickClose()
    {
        SetState(false);
    }
}
