using UnityEngine.UI;
/// <summary> 패널 뒤로가기 버튼 터치 (이전에 열려있던 ui frame 기반) </summary>
public class TutorialSequence_TouchClosePanel : TutorialSequence_None
{
	private Button mButton;

	protected override string FindUIName { get { return Owner?.LastOpenedUIFrame?.GetType().Name; } }

	/// <summary> 해당 버튼 가지고 오기 </summary>
	protected virtual Button GetButton()
	{		
		return UIFrame.gameObject.FindChildComponent<Button>("Bt_Panel_Exit", true);
	}

	protected override void StartGuide()
	{
		CancelInvoke(nameof(StartGuide));

		if (false == CheckStartGuideInvoke())
			return;

		if(null == mButton)
			mButton = GetButton();

		if (null == mButton || false == mButton.gameObject.activeSelf)
		{
			Invoke(nameof(StartGuide), 0.1f);
			return;
		}

		//버튼 가이드 시작
		ShowGuide(mButton.gameObject, ButtonAction);

		return;
	}

	private void ButtonAction()
	{
		mButton?.onClick.Invoke();
		EndSequence(false);
	}
}