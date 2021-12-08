using System;
using UnityEngine;
using UnityEngine.UI;

public class UISubHUDTemple : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] private GameObject ExitTempleButton;
    [SerializeField] private GameObject ControlCancelButton;
    [SerializeField] private GameObject ControlActionButton;
    [SerializeField] private GameObject InteractionButton;
    [SerializeField] private GameObject RideButton;

    [SerializeField] private GameObject ControlForwardButton;
    [SerializeField] private GameObject ControlBackwardButton;

    [SerializeField] private GameObject ObjTempleTitle;
    [SerializeField] private Text TempleTitleText;
    [SerializeField] private UISubHudTempleInfo TempleInfoUI;
    #endregion

    private Action mEventCancel;
    private Action mEventAction;
    private Action mEventInteraction;
    private Action<bool> mEventForwardButton;
    private Action<bool> mEventBackwardButton;

    public bool IsReady { get; private set; } = false;

	protected override void OnInitialize()
	{
		base.OnInitialize();
        TempleInfoUI.gameObject.SetActive(false);
        mEventCancel = null;
    }

	protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
        ControlCancelButton.SetActive(false);
        InteractionButton.SetActive(false);
        ControlActionButton.SetActive(false);
        ObjTempleTitle.SetActive(false);
        ControlForwardButton.SetActive(false);
        ControlBackwardButton.SetActive(false);
        TempleInfoUI.gameObject.SetActive(false);
        TempleInfoUI.ClearChest();

        IsReady = true;

        if(DBStage.TryGet(ZGameModeManager.Instance.StageTid, out var table))
        {
            //사당 나가기 버튼 On/Off
            ExitTempleButton.SetActive(table.StageType == GameDB.E_StageType.Temple);
            RideButton.SetActive(table.StageType == GameDB.E_StageType.Temple && table.RidingType == GameDB.E_RidingType.Riding);
        }
        else
        {
            ExitTempleButton.SetActive(false);
            RideButton.SetActive(false);
        }

        // NOTE(JWK): 유적 내부에서 기믹과 상호작용후 UI프레임워크 초기화가 들어올경우 예외처리로 Cancel을 처리해줌
        if(null != mEventCancel)
		{
            mEventCancel?.Invoke();
            mEventCancel = null;
        }
	}

    public void SetControlGimmick(E_TempleUIType uiType, Action onEventCancel = null, Action onEventAction = null, Action<bool> onEventForwardButton = null, Action<bool> onEventBackwardButton = null)
    {
        var hud = UIManager.Instance.Find<UIFrameHUD>();

        if (null == hud)
        {
            ZLog.LogError(ZLogChannel.UI, "아직 UIFrameHUD가 생성되지 않았다");
            return;
        }

        IsReady = false;

        mEventCancel = onEventCancel;
        mEventAction = onEventAction;
        mEventForwardButton = onEventForwardButton;
        mEventBackwardButton = onEventBackwardButton;

        hud.SetTempleInteractionUI(uiType);

        ControlCancelButton.SetActive(uiType.HasFlag(E_TempleUIType.Cancel));
        ControlActionButton.SetActive(uiType.HasFlag(E_TempleUIType.Action));

        ControlForwardButton.SetActive(uiType.HasFlag(E_TempleUIType.Forward));
        ControlBackwardButton.SetActive(uiType.HasFlag(E_TempleUIType.Backward));
    }

    public void ResetControlGimmick()
    {
        var hud = UIManager.Instance.Find<UIFrameHUD>();
        hud.ResetTempleInteractionUI();

        IsReady = true;

        mEventCancel = null;
        mEventAction = null;
        mEventForwardButton = null;
        mEventBackwardButton = null;

        ControlCancelButton.SetActive(false);
        ControlActionButton.SetActive(false);
        ControlForwardButton.SetActive(false);
        ControlBackwardButton.SetActive(false);
    }

    /// <summary> 기믹 트리거 작동방식에 따라 나오는 UI On/Off </summary>
    public void SetInteractionGimmick(bool bActive, Action eventInteraction = null)
    {
        if (false == IsReady && true == bActive)
            return;

        InteractionButton.SetActive(bActive);
        mEventInteraction = eventInteraction;

        IsReady = !bActive;
    }

    public void ShowTempleTitle()
    {
        if (null == ZGameModeManager.Instance.Table)
            return;

        ObjTempleTitle.SetActive(false);
        TempleTitleText.text = DBLocale.GetText(ZGameModeManager.Instance.Table.StageTextID);
        CancelInvoke(nameof(InvokeShowTempleTitle));
        Invoke(nameof(InvokeShowTempleTitle), 0.1f);
    }

    private void InvokeShowTempleTitle()
    {        
        CancelInvoke(nameof(InovkeHideTempleTitle));
        ObjTempleTitle.SetActive(true);
        Invoke(nameof(InovkeHideTempleTitle), 3f);
    }

    private void InovkeHideTempleTitle()
    {
        CancelInvoke(nameof(InovkeHideTempleTitle));
        ObjTempleTitle.SetActive(false);

        //유적 정보 표시
        TempleInfoUI.ShowTempleInfo();
    }

    public void OnClickCancel()
    {
        mEventCancel?.Invoke();
    }

    public void OnClickAction()
    {
        mEventAction?.Invoke();
    }

    /// <summary> 상호작용 </summary>
    public void OnClickInteraction()
    {
        mEventInteraction?.Invoke();
    }

    public void OnForwardButton(bool bPress)
    {
        mEventForwardButton?.Invoke(bPress);
    }

    public void OnBackwardButton(bool bPress)
    {
        mEventBackwardButton?.Invoke(bPress);
    }


    public void OnClickRidePopup()
    {
        if (UIManager.Instance.Find<UIFramePetChangeSelect>(nameof(UIFramePetChangeSelect)) == false)
        {
            UIManager.Instance.Load<UIFramePetChangeSelect>(nameof(UIFramePetChangeSelect), (loadName, loadFrame) =>
            {
                loadFrame.Init(OpenRidePopup);
            });
        }
        else
        {
            OpenRidePopup();
        }
    }

    private void OpenRidePopup()
    {
        UIManager.Instance.Open<UIFramePetChangeSelect>((str, frame) =>
        {
            frame.SetViewType(E_PetChangeViewType.Ride, () =>
            {
                UIManager.Instance.Close<UIFramePetChangeSelect>();
            });
        });
    }

    /// <summary> 사당 나가기 버튼 클릭 </summary>
    public void OnExitTemple()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) => {
            _popup.Open(ZUIString.ERROR, DBLocale.GetText("Temple_Exit Text")/*"유적을 나가시겠습니까?"*/, new string[] { "취소", "확인" }, new Action[] { () =>
            {
                 _popup.Close();
            }, () =>
            {
                var table = DBTemple.GetTempleTableByStageTid(ZGameModeManager.Instance.StageTid);

                 if (null != table)
                 {
                    //TODO :: 일단 퇴장 처리
                    ZGameManager.Instance.TryEnterStage(table.ExitPortalID, false, 0, 0);
                 }
                _popup.Close();
            } });
        });
    }
}