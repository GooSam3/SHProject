using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIFrameGuildTab_DisplayReward : UIFrameGuildTabBase
{
    public class DisplaySingleItemInfo
    {
        public Sprite iconSprite;
        public string strCnt;
    }

    public class EventParam_DisplayInfo : GuildDataUpdateEventParamBase
    {
        public string strTitle;
        public DisplaySingleItemInfo[] items;
    }

    #region SerializedField
    #region Preference Variable
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float disappearFadeDuration = 0.3f;
    #endregion

    #region UI Variables
    [SerializeField] private RectTransform uiGroup;
    [SerializeField] private Text txtTitle;
    [SerializeField] private GuildSingleRewardItemInfo itemSourceObj;
    [SerializeField] private RectTransform itemsParent;
    [SerializeField] private CanvasGroup canvasGroup;
    #endregion
    #endregion

    #region System Variables
    private List<GuildSingleRewardItemInfo> rewardItemUIsCached;
    private float displayRemainedSec;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    private void Update()
    {
        if (uiGroup.gameObject.activeSelf)
        {
            if (displayRemainedSec > 0)
            {
                displayRemainedSec -= Time.unscaledDeltaTime;

                if (displayRemainedSec <= 0)
                {
                    displayRemainedSec = 0;
                }
            }
            else
            {
                if (canvasGroup.alpha > 0)
                {
                    canvasGroup.alpha -= Time.unscaledDeltaTime / disappearFadeDuration;
                }
                else
                {
                    CloseDisplay();
                }
            }
        }
    }
    #endregion

    #region Public Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildFrame, FrameGuildTabType type)
    {
        base.Initialize(guildFrame, type);
        rewardItemUIsCached = new List<GuildSingleRewardItemInfo>(2);
    }

    public override void OnOpen()
    {
        base.OnOpen();
        uiGroup.gameObject.SetActive(false);
    }

    public override void OnClose()
    {
        base.OnClose();
        CloseDisplay();
    }

    public override void OnUpdateEventRise(UpdateEventType type, GuildDataUpdateEventParamBase param)
    {
        base.OnUpdateEventRise(type, param);

        switch (type)
        {
            case UpdateEventType.ObtainedGuildReward:
                {
                    var info = param as EventParam_DisplayInfo;

                    if (info != null)
                    {
                        DisplayReward(info);
                    }
                    else
                    {
                        ZLog.LogError(ZLogChannel.UI, "Casting error, this must be DisplayInfo Type");
                    }
                }
                break;
        }
    }
    #endregion

    #region Private Methods
    private void DisplayReward(EventParam_DisplayInfo info)
    {
        if (info.items != null)
        {
            if (rewardItemUIsCached.Count < info.items.Length)
            {
                int addCnt = info.items.Length - rewardItemUIsCached.Count;

                for (int i = 0; i < addCnt; i++)
                {
                    var instance = Instantiate(itemSourceObj, itemsParent);

                    if (instance != null)
                    {
                        instance.gameObject.SetActive(false);
                        rewardItemUIsCached.Add(instance);
                    }
                    else
                    {
                        CloseDisplay();
                        return;
                    }
                }
            }

            txtTitle.text = info.strTitle;
            
            for (int i = 0; i < info.items.Length; i++)
            {
                var data = info.items[i];
                var ui = rewardItemUIsCached[i];

                ui.Set(data.iconSprite, data.strCnt);
                ui.gameObject.SetActive(true);
            }
        }

        displayRemainedSec = displayDuration;
        canvasGroup.alpha = 1;
        uiGroup.gameObject.SetActive(true);
    }

    private void CloseDisplay()
    {
        displayRemainedSec = 0;
        canvasGroup.alpha = 0;
        uiGroup.gameObject.SetActive(false);
        rewardItemUIsCached.ForEach(t => t.gameObject.SetActive(false));
    }
    #endregion

    #region Insepctor Event (인스펙터 연결)
    #endregion
}
