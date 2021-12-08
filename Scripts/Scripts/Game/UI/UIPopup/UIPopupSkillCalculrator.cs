using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIPopupSkillCalculrator : MonoBehaviour
{
    private const int MIN_VALUE = 0;
    private const int SECOND_MAX_VALUE = 59;
    private const int MINUTE_MAX_VALUE = 29;

    #region UI Variable
    [SerializeField] private Button CloseButton = null;
    [SerializeField] private Button MaxButton = null;
    [SerializeField] private Text TitleText = null;
    [SerializeField] private Text TimeText = null;
    [SerializeField] private Text MinuteText = null;
    [SerializeField] private Text SecondText = null;
    [SerializeField] private GameObject MinuteSelectImage = null;
    [SerializeField] private GameObject SecondSelectImage = null;
    [SerializeField] private ZUIButtonRadio radioBtnOn = null;
    [SerializeField] private ZUIButtonRadio radioBtnOff = null;
    [SerializeField] private ZText DescText = null;
    #endregion

    #region System Variable
    private int TimeNumber = 0;
    private bool TimeFlag = true;   // true : 초, false : 분
    private uint SelectSkillId = 0;
    public byte IsUseSkillCycle = 0;
    #endregion

    public void Initialized(uint _skillId)
    {
        DescText.text = string.Format(DBLocale.GetText("Skill_Cooltime_Custom"), "29분 59초");

        SkillOrderData skillOrderData = ZNet.Data.Me.CurCharData.SkillUseOrder.Find(a => a.Tid == _skillId);

        if(skillOrderData != null)
        {
            MinuteText.text = (skillOrderData.CoolTime / 60).ToString();
            SecondText.text = (skillOrderData.CoolTime % 60).ToString();
            TimeNumber = (int)skillOrderData.CoolTime % 60;
            TimeText.text = (skillOrderData.CoolTime % 60).ToString();

            if (skillOrderData.IsUseSkillCycle == 0)
            {
                radioBtnOff.DoRadioButtonToggleOn();
            }
            else
            {
                radioBtnOn.DoRadioButtonToggleOn();
            }
        }
        else
        {
            radioBtnOff.DoRadioButtonToggleOn();
            MinuteText.text = "0";
            SecondText.text = "0";
            TimeText.text = "0";
            TimeNumber = 0;
        }

        SelectSkillId = _skillId;
        MinuteSelectImage.SetActive(false);
        SecondSelectImage.SetActive(true);

        TimeFlag = true;
    }

    public void OnClickMinuteSector()
    {
        if (IsUseSkillCycle == 0)
            return;

        if (int.TryParse(MinuteText.text, out TimeNumber) == false)
            TimeNumber = MIN_VALUE;

        TimeText.text = TimeNumber.ToString();

        TimeFlag = false;

        MinuteSelectImage.SetActive(true);
        SecondSelectImage.SetActive(false);
    }

    public void OnClickSecondSector()
    {
        if (IsUseSkillCycle == 0)
            return;

        if (int.TryParse(SecondText.text, out TimeNumber) == false)
            TimeNumber = MIN_VALUE;

        TimeText.text = TimeNumber.ToString();

        TimeFlag = true;

        MinuteSelectImage.SetActive(false);
        SecondSelectImage.SetActive(true);
    }

    public void OnClickMax()
    {
        if (IsUseSkillCycle == 0)
            return;

        RefreshText(true);
    }

    public void OnClickNum(int num)
    {
        if (IsUseSkillCycle == 0)
            return;

        if (int.TryParse($"{TimeNumber}{num}", out TimeNumber) == false)
            TimeNumber = MIN_VALUE;

        RefreshText();
    }

    public void OnClickAddNum(int num)
    {
        if (IsUseSkillCycle == 0)
            return;

        TimeNumber += num;
        RefreshText();
    }

    public void OnClickBackSpace()
    {
        if (IsUseSkillCycle == 0)
            return;

        string result = $"{TimeNumber}".Substring(0, $"{TimeNumber}".Length - 1);

        if (int.TryParse(result, out TimeNumber) == false)
            TimeNumber = MIN_VALUE;

        RefreshText();
    }

    private void RefreshText(bool bIsMax = false)
    {
        if (bIsMax)
        {
            TimeNumber = TimeFlag ? SECOND_MAX_VALUE : MINUTE_MAX_VALUE;

            MinuteText.text = MINUTE_MAX_VALUE.ToString();
            SecondText.text = SECOND_MAX_VALUE.ToString();
            TimeText.text = TimeNumber.ToString();
        }
        else
        {
            TimeNumber = Mathf.Clamp(TimeNumber, MIN_VALUE, TimeFlag ? SECOND_MAX_VALUE : MINUTE_MAX_VALUE);

            TimeText.text = TimeNumber.ToString();

            if (TimeFlag)
            {
                SecondText.text = TimeNumber.ToString();
            }
            else
            {
                MinuteText.text = TimeNumber.ToString();
            }
        }
    }

    public ulong GetSkillCycle()
    {
        ulong second = 0;
        ulong minute = 0;
        
        if (ulong.TryParse(SecondText.text, out second) == false)
            second = MIN_VALUE;

        if (ulong.TryParse(MinuteText.text, out minute) == false)
            minute = MIN_VALUE;

        return second + (minute * 60);
    }

    public void UseSkillCycleOnOff(int _isUse)
    {
        IsUseSkillCycle = (byte)_isUse;
    }
}
