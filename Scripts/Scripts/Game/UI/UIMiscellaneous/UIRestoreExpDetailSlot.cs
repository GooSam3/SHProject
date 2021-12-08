using System;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UIRestoreExpDetailSlot : MonoBehaviour
{
    #region UI Variable
    // 복구 경험치 
    [SerializeField] private Text txtLossNum;
    // 남은시간 
    [SerializeField] private Text txtRemainedTime; 
    // 사망 원인 공격자 
    [SerializeField] private Text txtAttacker;
    // 선택 이미지 
    [SerializeField] private Image imgSelectLine;
    #endregion

    #region System Variable
    public RestoreExpData data { get; private set; }
    event Action<UIRestoreExpDetailSlot, bool> onClick;
    ulong lastRemainedTime;

    public bool IsValid
    {
        get
        {
            return data.ExpireDt > TimeManager.NowSec;
        }
    }
    #endregion

    #region Public Methods
    public void Setup(RestoreExpData data, Action<UIRestoreExpDetailSlot, bool> onClick)
    {
        lastRemainedTime = ulong.MaxValue;

        this.data = data;
        this.onClick = onClick;

        imgSelectLine.gameObject.SetActive(false);

        var expRate = DBLevel.GetExpRate(data.Exp, Me.CurCharData.Level, Me.CurCharData.TID);
        txtLossNum.text = string.Format("{0}({1:0.00}%)", data.Exp, expRate * 100f);
        txtAttacker.text = data.KillerNick;
    }

    public void UpdateUI()
    {
        if (UpdateLastRemainedTime())
            txtRemainedTime.text = TimeHelper.GetRemainTime(lastRemainedTime);
    }

    bool UpdateLastRemainedTime()
    {
        ulong curRemainedTime = 0;

        if (data.ExpireDt < TimeManager.NowSec)
            curRemainedTime = 0;
        else curRemainedTime = data.ExpireDt - TimeManager.NowSec;

        bool update = lastRemainedTime != curRemainedTime;
        if (update)
            lastRemainedTime = curRemainedTime;
        return update;
    }

    public void SetSelection(bool selected)
    {
        imgSelectLine.gameObject.SetActive(selected);
    }
    #endregion

    #region OnClick Event
    public void OnClick()
    {
        onClick?.Invoke(this, true);
    }
    #endregion
}
