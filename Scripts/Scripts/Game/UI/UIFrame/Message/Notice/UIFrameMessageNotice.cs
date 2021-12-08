using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameMessageNotice : CUIFrameBase
{
    #region External
    public UIMessageNoticeEffect Effect { get; private set; } = null;
    #endregion

    #region Variable
    private Queue<UIMessageNoticeData> NoticeMessageQueue = new Queue<UIMessageNoticeData>();
    private Queue<UIMessageNoticeData> NoticeSubMessageQueue = new Queue<UIMessageNoticeData>();

    [SerializeField] private Text[] NoticeTxt = null;
    [SerializeField] private CanvasGroup[] CanvasGroup = null;

    [SerializeField] private bool[] IsMessageShow = null;
    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        Effect = new UIMessageNoticeEffect();

        for (int i = 0; i < ZUIConstant.MESSAGE_NOTICE_STYLE; i++)
        {
            NoticeTxt[i].text = string.Empty;
            CanvasGroup[i].alpha = 1.0f;
            IsMessageShow[i] = false;
        } 
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        ShowMessage(UIMessageNoticeEnum.E_NoticeType.Main);
        ShowMessage(UIMessageNoticeEnum.E_NoticeType.Sub);
    }

    public void AddMessage(UIMessageNoticeEnum.E_NoticeType _type, UIMessageNoticeData _data)
    {
        GetQueue(_type).Enqueue(_data);

        if (gameObject.activeSelf && !IsMessageShow[Convert.ToInt32(_type)])
            ShowMessage(_type);
    }

    private void ShowMessage(UIMessageNoticeEnum.E_NoticeType _type)
    {
        Queue<UIMessageNoticeData> msgQueue = GetQueue(_type);

        if (msgQueue.Count == 0)
            return;

        int typeIdx = Convert.ToInt32(_type);
        if (!IsMessageShow[typeIdx])
        {
            IsMessageShow[typeIdx] = true;

            Effect.Show(NoticeTxt[typeIdx], msgQueue.Peek(), delegate {
                msgQueue.Dequeue(); 
                IsMessageShow[typeIdx] = false;
                NoticeTxt[typeIdx].text = string.Empty;
                CanvasGroup[typeIdx].alpha = 1.0f;
                ShowMessage(_type);
            });
        }
    }

    private Queue<UIMessageNoticeData> GetQueue(UIMessageNoticeEnum.E_NoticeType _type)
    {
        return _type == UIMessageNoticeEnum.E_NoticeType.Main ? NoticeMessageQueue : NoticeSubMessageQueue;
    }
}