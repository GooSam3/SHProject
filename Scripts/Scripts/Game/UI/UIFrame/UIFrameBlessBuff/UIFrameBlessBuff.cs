using GameDB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIFrameBlessBuff : ZUIFrameBase
{
	#region Preference Variable
	[SerializeField] private Text titleText;
    [SerializeField] private List<Text> CurBufferTextList;
    [SerializeField] private List<Text> ListBuffStepText;
    [SerializeField] private List<Text> ListBuffStepCount;
    [SerializeField] private List<Image> ListBuffStepIcon;
    [SerializeField] private Text SetUseTitle;
    [SerializeField] private Text SetUseDesc;

    [SerializeField] private Option_BlessBuffPotion BlessSetUI;
    #endregion

    #region System Variables
    bool bChangedUseValue = false;
    public override bool IsBackable => true;
    #endregion

    protected override void OnInitialize()
	{
        InitString();

		base.OnInitialize();
    }

    private void InitString()
	{
        SetUseTitle.text = DBLocale.GetText("God_Tear_AutoUse");
        SetUseDesc.text = DBLocale.GetText("God_Tear_AutoUseDesc");
	}

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
        ZPawnManager.Instance.DoAddEventCreateMyEntity(UpdateBlessFrame);

        bChangedUseValue = false;
        BlessSetUI.bAutoSave = false;
        BlessSetUI.OnChangeValue = (changeValue) => {
            bChangedUseValue = true;
        };
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(UpdateBlessFrame);
    }

    protected override void OnHide()
	{
		base.OnHide();
        ZPawnManager.Instance.DoRemoveEventCreateMyEntity(UpdateBlessFrame);
    }

	private void UpdateBlessFrame()
	{        
        if (false == UpdateBuff(DBConfig.GodPower_AbilityActionID, E_GodBuffType.GodPower))
        {
            if(false == UpdateBuff(DBConfig.GodBless_AbilityActionID, E_GodBuffType.GodBless))
            {
                titleText.text = DBLocale.GetText("God_Tear_Title");

                for (int i = 0; i < CurBufferTextList.Count; i++)
                {
                    CurBufferTextList[i].color = Color.grey;
                }

                UpdateCurrentDesc(DBAbility.GetAction(DBConfig.GodBless_AbilityActionID), E_GodBuffType.GodBless);
                UpdateStackBuffList(E_GodBuffType.GodBless, 0);
            }
        }
    }

    private bool UpdateBuff(uint abilityActionId, E_GodBuffType godBuffType)
    {
        var buffData = ZPawnManager.Instance.MyEntity.FindAbilityAction(abilityActionId);
        
        if (buffData != null)
        {
            var tabledata = DBAbility.GetAction(abilityActionId);
            int stackCount = 0;
            uint maxStackCount = tabledata.MaxBuffStack;

            if (buffData.EndServerTime > TimeManager.NowSec)
            {
                stackCount = Mathf.CeilToInt((float)(buffData.EndServerTime - TimeManager.NowSec) / (float)tabledata.MinSupportTime);
            }

            UpdateCurrentDesc(tabledata, godBuffType);
            UpdateStackBuffList(godBuffType, stackCount);

            switch(godBuffType)
            {
                case E_GodBuffType.GodBless:
                    {
                        titleText.text = DBLocale.GetText("God_Tear_Title");
                    }
                    break;
                case E_GodBuffType.GodPower:
                    {
                        titleText.text = DBLocale.GetText("God_Power_Title");
                    }
                    break;
            }
            
            for (int i = 0; i < CurBufferTextList.Count; i++)
            {
                CurBufferTextList[i].color = Color.white;
            }

            return true;
        }

        return false;
    }

    private void UpdateCurrentDesc(AbilityAction_Table tabledata, E_GodBuffType godBuffType)
	{
		if (tabledata.AbilityViewType == E_AbilityViewType.ToolTip)
		{
			//curBuffText.text = DBLocale.GetText(tabledata.ToolTip);
		}
		else
		{
			Dictionary<E_AbilityType, System.ValueTuple<float, float>> allAbilitys = DBAbility.GetAllAbilityData(tabledata.AbilityActionID);
			string strDesc = "";
			string strValue = "";
            int CurBufferTextIndex = 0;
            foreach (E_AbilityType type in allAbilitys.Keys)
			{
				if (!DBAbility.IsParseAbility(type))
					continue;
				strValue = DBAbility.ParseAbilityValue(type, allAbilitys[type].Item1, allAbilitys[type].Item2, " ");


				if (string.IsNullOrEmpty(strValue))
				{
					if (string.IsNullOrEmpty(strDesc))
                        CurBufferTextList[CurBufferTextIndex].text = string.Format("{0}", DBLocale.GetText(DBAbility.GetAbilityName(type)));
                    //strDesc = string.Format("{0}", DBLocale.GetText(DBAbility.GetAbilityName(type)));
                    else
                        CurBufferTextList[CurBufferTextIndex].text = string.Format("{0}\n{1}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)));
                    //strDesc = string.Format("{0}\n{1}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)));
                }
                else
				{
					if (string.IsNullOrEmpty(strDesc))
                        CurBufferTextList[CurBufferTextIndex].text = string.Format("{0}{1}", DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                    //strDesc = string.Format("{0}{1}", DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                    else
                        CurBufferTextList[CurBufferTextIndex].text = string.Format("{0}\n{1}{2}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                    //strDesc = string.Format("{0}\n{1}{2}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                }
                CurBufferTextIndex++;
            }
			//curBuffText.text = strDesc;
		}
	}

	private void UpdateStackBuffList(E_GodBuffType godBuffType, int CurStackCnt)
	{
		for (int i = 0; i < ListBuffStepText.Count; i++)
            ListBuffStepText[i].text = "";

		List<GodBuff_Table> godBuffList = new List<GodBuff_Table>();
		godBuffList.AddRange(DBGodBuff.GetGodBuffList(godBuffType));

        godBuffList.Sort((x, y) => x.Stack.CompareTo(y.Stack));

        for (int i = 0; i < godBuffList.Count; i++)
        {
            string strDesc = "";

            Dictionary<E_AbilityType, System.ValueTuple<float, float>> allAbilitys = new Dictionary<E_AbilityType, System.ValueTuple<float, float>>();

            if (godBuffList[i].AbilityActionID_01 != 0)
            {
                var tabledata = DBAbility.GetAction(godBuffList[i].AbilityActionID_01);

                if (tabledata.AbilityViewType == E_AbilityViewType.ToolTip)
                {
                    if (string.IsNullOrEmpty(strDesc))
                        strDesc = string.Format("{0}", DBLocale.GetText(tabledata.ToolTip));
                    else
                        strDesc = string.Format("{0},{1}", strDesc, DBLocale.GetText(tabledata.ToolTip));
                }
                else
                {
                    foreach (var data in DBAbility.GetAllAbilityData(tabledata.AbilityActionID))
                    {
                        if (!allAbilitys.ContainsKey(data.Key))
                            allAbilitys.Add(data.Key, (0, 0));
                        allAbilitys[data.Key] = (allAbilitys[data.Key].Item1 + data.Value.Item1, allAbilitys[data.Key].Item2 + data.Value.Item2);
                    }
                }
            }
            if (godBuffList[i].AbilityActionID_02 != 0)
            {
                var tabledata2 = DBAbility.GetAction(godBuffList[i].AbilityActionID_02);

                if (tabledata2.AbilityViewType == E_AbilityViewType.ToolTip)
                {
                    if (string.IsNullOrEmpty(strDesc))
                        strDesc = string.Format("{0}", DBLocale.GetText(tabledata2.ToolTip));
                    else
                        strDesc = string.Format("{0},{1}", strDesc, DBLocale.GetText(tabledata2.ToolTip));
                }
                else
                {
                    foreach (var data in DBAbility.GetAllAbilityData(tabledata2.AbilityActionID))
                    {
                        if (!allAbilitys.ContainsKey(data.Key))
                            allAbilitys.Add(data.Key, (0, 0));
                        allAbilitys[data.Key] = (allAbilitys[data.Key].Item1 + data.Value.Item1, allAbilitys[data.Key].Item2 + data.Value.Item2);
                    }
                }
            }

            if (i > 0)
            {
                Dictionary<E_AbilityType, System.ValueTuple<float, float>> prevallAbilitys = new Dictionary<E_AbilityType, System.ValueTuple<float, float>>();

                if (godBuffList[i - 1].AbilityActionID_01 != 0)
                {
                    var prevtabledata = DBAbility.GetAction(godBuffList[i - 1].AbilityActionID_01);
                    foreach (var data in DBAbility.GetAllAbilityData(prevtabledata.AbilityActionID))
                    {
                        if (!prevallAbilitys.ContainsKey(data.Key))
                            prevallAbilitys.Add(data.Key, (0, 0));
                        prevallAbilitys[data.Key] = (prevallAbilitys[data.Key].Item1 + data.Value.Item1, prevallAbilitys[data.Key].Item2 + data.Value.Item2);
                    }
                }
                if (godBuffList[i - 1].AbilityActionID_02 != 0)
                {
                    var prevtabledata2 = DBAbility.GetAction(godBuffList[i - 1].AbilityActionID_02);
                    foreach (var data in DBAbility.GetAllAbilityData(prevtabledata2.AbilityActionID))
                    {
                        if (!prevallAbilitys.ContainsKey(data.Key))
                            prevallAbilitys.Add(data.Key, (0, 0));
                        prevallAbilitys[data.Key] = (prevallAbilitys[data.Key].Item1 + data.Value.Item1, prevallAbilitys[data.Key].Item2 + data.Value.Item2);
                    }
                }

                List<E_AbilityType> KeyList = new List<E_AbilityType>(allAbilitys.Keys);
                foreach (var key in KeyList)
                {
                    if (prevallAbilitys.ContainsKey(key))
                    {
                        if (allAbilitys[key].Item1 - prevallAbilitys[key].Item1 == 0
                            && allAbilitys[key].Item2 - prevallAbilitys[key].Item2 == 0)
                            allAbilitys.Remove(key);
                    }
                }
            }

            string strValue = "";
            foreach (E_AbilityType type in allAbilitys.Keys)
            {
                if (!DBAbility.IsParseAbility(type))
                    continue;
                strValue = DBAbility.ParseAbilityValue(type, allAbilitys[type].Item1, allAbilitys[type].Item2, " ");

                if (string.IsNullOrEmpty(strValue))
                {
                    if (string.IsNullOrEmpty(strDesc))
                        strDesc = string.Format("{0}", DBLocale.GetText(DBAbility.GetAbilityName(type)));
                    else
                        strDesc = string.Format("{0},{1}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)));
                }
                else
                {
                    if (string.IsNullOrEmpty(strDesc))
                        strDesc = string.Format("{0}{1}", DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                    else
                        strDesc = string.Format("{0},{1}{2}", strDesc, DBLocale.GetText(DBAbility.GetAbilityName(type)), allAbilitys[type].Item1 > 0 ? strValue : string.Format("<color=red>{0}</color>", strValue));
                }
            }
            ListBuffStepCount[i].text = string.Format("[{0}]", godBuffList[i].Stack);
            ListBuffStepText[i].text = string.Format("{0}", strDesc);
            if (CurStackCnt >= godBuffList[i].Stack)
            {
                ListBuffStepCount[i].color = Color.white;
                ListBuffStepText[i].color = Color.white;
                ListBuffStepIcon[i].color = Color.white;
            }
            else
            {
                ListBuffStepCount[i].color = Color.grey;
                ListBuffStepText[i].color = Color.grey;
                ListBuffStepIcon[i].color = Color.grey;
            }

        }
    }

    public void Close()
    {
        if (bChangedUseValue)
        {
            var donotchangeCheck = DeviceSaveDatas.LoadCurCharData("DoNotCheckBlessChange", false);

            if (!donotchangeCheck)
            {
                UIMessagePopup.ShowPopupCheckOkCancel(
                    ZUIString.INFO,
                    DBLocale.GetText("Light_Settings_Message"),
                    DBLocale.GetText("CheckBossPortalDontShow"),
                    donotchangeCheck,
                    (bChecked) =>
                    {

                        DeviceSaveDatas.SaveCurCharData("DoNotCheckBlessChange", bChecked);

                        BlessSetUI.bAutoSave = true;
                        UIManager.Instance.Close<UIFrameBlessBuff>(true);
                    },
                    () =>
                    {
                        UIManager.Instance.Close<UIFrameBlessBuff>(true);
                    });
            }
            else
            {
                BlessSetUI.bAutoSave = true;
                UIManager.Instance.Close<UIFrameBlessBuff>(true);
            }
        }
        else
            UIManager.Instance.Close<UIFrameBlessBuff>(true);
    }
}
