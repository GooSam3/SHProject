using UnityEngine.Video;

public class UILoginIntroVideo : UIFrameLogin
{
	#region Variable
	private UIFrameLogin Frame = null;
	#endregion

	/// <summary> 인트로 영상 설정 </summary>
	protected override void Initialize(ZUIFrameBase _frame)
	{
		base.Initialize(_frame);

		Frame = _frame as UIFrameLogin;

		Frame.VideoPlayer.errorReceived += HandleVideoLoadingErrorReceived;
		Frame.VideoPlayer.prepareCompleted += HandleVideoLoadingFinish;
	}

	/// <summary>Intro 영상 로드가 성공했을 경우 Callback</summary>
	private void HandleVideoLoadingFinish(VideoPlayer _source)
	{
		Frame.LoginBody.SetActive(true);

		UIManager.Instance.Close<UIFramePatchProcess>(true);
	}

	/// <summary>Intro 영상 로드를 실패했을 경우 Callback</summary>
	private void HandleVideoLoadingErrorReceived(VideoPlayer _source, string _msg)
	{
		UICommon.OpenSystemPopup_One(ZUIString.ERROR,
			_msg,
			ZUIString.LOCALE_OK_BUTTON,
			delegate
			{
				// 로드가 실패했으나 로그인 자체는 정상적으로 진행
				Frame.LoginBody.SetActive(true);

				UIManager.Instance.Close<UIFramePatchProcess>(true);
			});
	}
}