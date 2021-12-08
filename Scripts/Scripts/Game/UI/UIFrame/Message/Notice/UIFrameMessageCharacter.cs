using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameMessageCharacter : CUIFrameBase
{
    #region External
    public UIMessageNoticeEffect Effect { get; private set; } = null;
    #endregion

    #region Variable
    private List<UIMessageListItem> MessageList = new List<UIMessageListItem>();

    [SerializeField] private ScrollRect MessageScrollRect = null;

    private bool IsInit = false;
    #endregion

    protected override void OnInitialize()
    {
        base.OnInitialize();

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIMessageListItem), delegate
        {
            Effect = new UIMessageNoticeEffect();

            IsInit = true;
        });  
    }

    protected override void OnRemove()
    {
        base.OnRemove();

        ZPoolManager.Instance.Clear(E_PoolType.UI, nameof(UIMessageListItem));
    }

    protected override void OnHide()
    {
        base.OnHide();

        for (int i = 0; i < MessageList.Count; i++)
            Destroy(MessageList[i].gameObject);

        MessageList.Clear();
    }

    public void AddMessage(UIMessageNoticeData _data)
    {
        MessageScrollRect.gameObject.SetActive(!UIManager.Instance.GetOpenFocusType(CManagerUIFrameFocusBase.E_UICanvas.Back, CManagerUIFrameFocusBase.E_UIFrameFocusAction.FullScreen));

        UIMessageListItem obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIMessageListItem)).GetComponent<UIMessageListItem>();

        if (obj != null)
        {
            if (MessageList.Count > 5)
                for(int i = 0; i < MessageList.Count; i++)
                    if(MessageList[i].gameObject.activeSelf)
                    {
                        MessageList[i].gameObject.SetActive(false);
                        break;
                    }

            obj.transform.SetParent(MessageScrollRect.content, false);
            MessageList.Add(obj);
            var message = MessageList.Find(item => item == obj);

            if(message != null)
            {
                Effect.Show(message.TxtObj, _data, delegate {
                    var endMessage = MessageList.Find(item => item == message);

                    if(endMessage != null)
                    {
                        Destroy(endMessage.gameObject);
                        MessageList.Remove(endMessage);
                    }
                });
            }
        }
    }
}