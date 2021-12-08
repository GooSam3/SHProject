using UnityEngine;

/// <summary> 메시지 띄움 </summary>
public class ZGA_NoticeMessage : ZGimmickActionBase
{
    [Header("알림 메시지 ID")]
    public string LocaleId = string.Empty;

    protected override void InvokeImpl()
    {
        if (string.IsNullOrEmpty(LocaleId))
        {
            return;
        }
            
        UICommon.SetNoticeMessage(DBLocale.GetText(LocaleId), Color.red, 1.0f, UIMessageNoticeEnum.E_MessageType.BackNotice);
    }

    protected override void CancelImpl()
    {
    }
}