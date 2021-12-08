using System;

public class UIMessagePopup
{
    /// <summary> 제목, 내용이 들어가는 확인/취소 팝업 </summary>
    public static void ShowPopupOkCancel(string title, string message, Action onOk, Action onCancel = null)
    {
        UIManager.Instance.Open<UIMessagePopupNormal>((name, popup) =>
        {
            popup.Set(title, message, onOk, onCancel);
        });
    }

    /// <summary> 제목, 내용이 들어가는 확인/취소 팝업 + 추가 체크 액션을 할당 할 수 있음</summary>
    public static void ShowPopupCheckOkCancel(string title, string message, string checkMessage, bool bDefaultCheck , Action<bool> onOk, Action onCancel = null)
    {
        UIManager.Instance.Open<UIMessagePopupNormal>((name, popup) =>
        {
            popup.Set(title, message, checkMessage, bDefaultCheck, onOk, onCancel);
        });
    }

    /// <summary> 내용만 들어가는 확인/취소 팝업 </summary>
    public static void ShowPopupOkCancel(string message, Action onOk, Action onCancel = null)
    {
        UIManager.Instance.Open<UIMessagePopupDefault>((name, popup) =>
        {
            popup.Set(message, onOk, onCancel);
        });
    }

    /// <summary> 내용만 들어가고, 버튼 1개인 팝업</summary>
    public static void ShowPopupOk(string message, Action onOK = null)
    {
        UIManager.Instance.Open<UIMessagePopupDefault>((name, popup) =>
        {
            popup.Set(message, onOK);
        });
    }

    /// <summary> 비용 소모 관련 확인/취소 팝업 </summary>
    public static void ShowCostPopup(string title, string message, uint costItemTid, ulong cost, Action onOk, Action onCancel = null, string costDescKey = "Cost_Desc")
    {
        UIManager.Instance.Open<UIMessagePopupCost>((name, popup) =>
        {
            popup.Set(title, message, costItemTid, cost, onOk, onCancel, costDescKey);
        });
    }

    /// <summary> 텍스트 입력 가능한 확인/취소 팝업 </summary>
    public static void ShowInputPopup(string title, string message, Action<string> onOk, Action onCancel = null, int characterLimit = 20)
    {
        UIManager.Instance.Open<UIMessagePopupInput>((name, popup) =>
        {
            popup.Set(title, message, onOk, onCancel, characterLimit);
        });
    }

	/// <summary> 텍스트 입력 가능한 팝업 (커스텀 가능 버전) </summary>
	public static void ShowInputPopup(Action<UIMessagePopupInput> _Opened)
	{
		UIManager.Instance.Open<UIMessagePopupInput>((name, popup) =>
		{
			_Opened?.Invoke(popup);
		});
	}
}