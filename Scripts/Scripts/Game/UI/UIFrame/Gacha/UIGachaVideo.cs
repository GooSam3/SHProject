using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

public class UIGachaVideo : UIFrameGacha
{
	#region Variable
	private UIFrameGacha Frame = null;

	public VideoClip DefaultVideo { get; private set; } = null;
	public VideoClip NextVideo { get; private set; } = null;

	// 임시 로드할 비디오(강림 선택용)
	public VideoClip SubVideo { get; private set; } = null;

	public VideoClip EndVideo { get; private set; } = null;
	#endregion

	protected override void Initialize(ZUIFrameBase _frame)
	{
		base.Initialize(_frame);

		Frame = _frame as UIFrameGacha;

		Frame.VideoPlayer.errorReceived += HandleVideoLoadingErrorReceived;
		Frame.VideoPlayer.prepareCompleted += HandleVideoLoadingFinish;
		Frame.VideoPlayer.loopPointReached += HandleVideoPlayingFinish;
	}

	protected override void OnHide()
	{
		base.OnHide();

		StopVideo();
		Clear();
	}

	public void Clear()
	{
		DefaultVideo = null;
		NextVideo = null;
		SubVideo = null;
		EndVideo = null;
	}

	#region Video Play Process
	public void StartPlayVideo()
	{
		string assetName = string.Empty;
		string nextAssetName = string.Empty;
		string endName = string.Empty;

		string subVideo = string.Empty;	

		switch (Frame.CurrentGachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Pet:
				assetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_Start);

				int randPet = UnityEngine.Random.Range(0, 3);

				switch(randPet)
				{
					case 0: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_Playing_1); break;
					case 1: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_Playing_2); break;
					case 2: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_Playing_3); break;
				}

				endName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_End);
				break;
			case UIGachaEnum.E_GachaStyle.Class:
				assetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Start);


				nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Skill_1);
				subVideo = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Skill_2);
				
				endName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, Frame.IsRareSequence() ? UIGachaEnum.E_VideoType.Class_Die_2 : UIGachaEnum.E_VideoType.Class_Die_1);

				break;
			case UIGachaEnum.E_GachaStyle.Ride:
				{
					assetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_Start);

					int randRide = UnityEngine.Random.Range(0, 3);

					switch (randRide)
					{
						case 0: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_Playing_1); break;
						case 1: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_Playing_2); break;
						case 2: nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_Playing_3); break;
					}

					endName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_End);
				}
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				assetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Item, UIGachaEnum.E_VideoType.Item_Normal);
				nextAssetName = UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Item, UIGachaEnum.E_VideoType.Item_Hidden);
				break;
		}

		if (assetName == string.Empty)
		{
			UICommon.OpenSystemPopup_One(ZUIString.ERROR,
						"영상 파일이 존재하지 않습니다.", ZUIString.LOCALE_OK_BUTTON);
			return;
		}

		if(DefaultVideo != null)
			PlayVideo(DefaultVideo);
		else
			LoadVideo(assetName, (string _name, VideoClip _clip) => {
				DefaultVideo = _clip;
				PlayVideo(_clip);
			});

		// Netx Video
		if (NextVideo == null)
			LoadVideo(nextAssetName, (string _nextName, VideoClip _nextClip) => {
				NextVideo = _nextClip;
			});

		// End Video
		if (EndVideo == null)
			LoadVideo(endName, (string _endName, VideoClip _endClip) => {
				EndVideo = _endClip;
			});

		if(string.IsNullOrEmpty(subVideo) == false && SubVideo == null)
        {
			LoadVideo(subVideo, (string _subName, VideoClip _subClip) => {
				SubVideo = _subClip;
			});
		}
	}

	/// <summary>영상 플레이</summary>
	/// <param name="_clip"></param>
	public void PlayVideo(VideoClip _clip)
	{
		Frame.VideoImage.color = new Color(255, 255, 255, 255);

		// 펫,탈것은 타임라인 먼저 보여주고 연출됨
		if (Frame.SkipMode || Frame.CurrentGachaStyle == UIGachaEnum.E_GachaStyle.Pet || Frame.CurrentGachaStyle == UIGachaEnum.E_GachaStyle.Ride)
		{
			switch (Frame.CurrentGachaStyle)
			{
				case UIGachaEnum.E_GachaStyle.Pet:
				case UIGachaEnum.E_GachaStyle.Ride:
                    {
						if (Frame.IsHidden && Frame.SkipMode == false)
						{
							Frame.TurnAllCardBtn.gameObject.SetActive(false);
							Play();
						}
						else if (Frame.IsHidden == false || Frame.SkipMode == true)
						{
							Frame.VideoImage.color = new Color(255, 255, 255, 0);
							Play(delegate {
								TimeInvoker.Instance.RequestInvoke(delegate {
									Frame.VideoPlayer.frame = (long)Frame.VideoPlayer.frameCount;
								}, 0.005f);
							});
						}
                    }
					break;
				case UIGachaEnum.E_GachaStyle.Class:
					{
						Frame.VideoImage.color = new Color(255, 255, 255, 0);

						Play(delegate {
							TimeInvoker.Instance.RequestInvoke(delegate {
								Frame.VideoPlayer.frame = (long)Frame.VideoPlayer.frameCount;
							}, 0.005f);
						});
					}
					break;
				case UIGachaEnum.E_GachaStyle.Item:
					{
						Frame.VideoImage.color = new Color(255, 255, 255, 0);

						Play(delegate {
							TimeInvoker.Instance.RequestInvoke(delegate {
								Frame.VideoPlayer.frame = (long)Frame.VideoPlayer.frameCount;
								Frame.VideoImage.color = new Color(255, 255, 255, 255);
							}, 0.005f);});

						switch (Frame.CurrentTimeLineType)
						{
							case UIGachaEnum.E_TimeLineType.Item_1_Start: Frame.SetCurrentTimeLineType(UIGachaEnum.E_TimeLineType.Item_1_End); break;
							case UIGachaEnum.E_TimeLineType.Item_11_Start: Frame.SetCurrentTimeLineType(UIGachaEnum.E_TimeLineType.Item_11_End); break;
						}
					}
					break;
			}
		}
		else
			Play();

		void Play(Action _callback = null)
		{
			Frame.SetVideoClip(_clip);
			Frame.StateObj[Convert.ToInt32(UIGachaEnum.E_StateObjectType.Video)].SetActive(true);
			Frame.VideoPlayer.Play();

			if (Frame.VideoPlayer.isPlaying == false)
				Play();
			else
				_callback?.Invoke();
		}
	}

	/// <summary>플레이 중인 영상 정지</summary>
	public void StopVideo()
	{
		Frame.VideoPlayer.Stop();
		Frame.SetVideoClip(null);
		Frame.StateObj[Convert.ToInt32(UIGachaEnum.E_StateObjectType.Video)].SetActive(false);
	}

	/// <summary>플레이 중인 영상 일시 정지</summary>
	public void PauseVideo()
	{
		Frame.VideoPlayer.Stop();
	}

	/// <summary>영상 로드가 성공했을 경우 Callback</summary>
	private void HandleVideoLoadingFinish(VideoPlayer _source)
	{
		
	}

	/// <summary>영상 로드를 실패했을 경우 Callback</summary>
	private void HandleVideoLoadingErrorReceived(VideoPlayer _source, string _msg)
	{
		UICommon.OpenSystemPopup_One(ZUIString.ERROR,
			_msg,
			ZUIString.LOCALE_OK_BUTTON,
			delegate
			{
				UICommon.OpenSystemPopup_One(ZUIString.ERROR,
						"영상 정보 요청이 실패하였습니다..", ZUIString.LOCALE_OK_BUTTON);
				return;
			});
	}

	/// <summary>영상 실행이 끝났을 경우 Callback</summary>
	private void HandleVideoPlayingFinish(VideoPlayer _source)
	{
		switch (Frame.CurrentGachaStyle)
		{
			case UIGachaEnum.E_GachaStyle.Pet:
				{
					if (Frame.IsHidden == false)
					{
						Frame.CardLinker?.SetEndGoldCardSequence();
						StopVideo();
						Frame.CheckTimeLine();
						break;
					}
					else if (Frame.IsHidden == true)
					{
						if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_Start))//시작
							PlayVideo(NextVideo);
						else if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Pet, UIGachaEnum.E_VideoType.Pet_End))
						{//마지막
							StopVideo();
							Frame.SetIsHidden(false);
							Frame.TurnAllCardBtn.gameObject.SetActive(true);
							Frame.CardLinker?.SetEndGoldCardSequence();

							//Frame.CheckTimeLine();
						}
						else// 중간 비디오
						{
							PlayVideo(EndVideo);
						}
					}
				}
				break;
			case UIGachaEnum.E_GachaStyle.Ride: // 탈것 및 펫의 영상연출은 특정등급 이상만 뜸
				{
					if(Frame.IsHidden == false)
                    {
						Frame.CardLinker?.SetEndGoldCardSequence();
						StopVideo();
						Frame.CheckTimeLine();
						break;
					}
					else if(Frame.IsHidden == true)
                    {
						if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_Start))//시작
							PlayVideo(NextVideo);
						else if(_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Ride, UIGachaEnum.E_VideoType.Ride_End))
                        {//마지막
							StopVideo();
							Frame.SetIsHidden(false);
							Frame.TurnAllCardBtn.gameObject.SetActive(true);
							Frame.CardLinker?.SetEndGoldCardSequence();

							//Frame.CheckTimeLine();
						}
						else// 중간 비디오
                        {
							PlayVideo(EndVideo);
						}
					}
				}
				break;
			case UIGachaEnum.E_GachaStyle.Class:
                {
					if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Start))
					{// 시작비디오 끝났다!
					 // 선택ui출력
						PauseVideo();

						if (Frame.SkipMode == false)
						{
							Frame.SetSkillInputBtn(true);
						}
						else
						{
							Frame.CheckTimeLine();
						}
					}
					else if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Skill_1) ||
							 _source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Class, UIGachaEnum.E_VideoType.Class_Skill_2))
                    {// 스킬선택했다!!
						PlayVideo(EndVideo);
                    }
					else
                    {
						StopVideo();

						Frame.GachaSceneController.SetNext();
						Frame.SetClassViewSequence(true);
					}
				}
				break;
			case UIGachaEnum.E_GachaStyle.Item:
				{
					if (_source.clip.name == UIGachaData.GetVideoName(UIGachaEnum.E_GachaStyle.Item, UIGachaEnum.E_VideoType.Item_Hidden))
						PauseVideo();
					else if (Frame.IsHidden)
					{
						Frame.SetIsHidden(false);
						PauseVideo();
					}
					else
						Frame.CheckTimeLine();
				}
				break;
		}
	}

	/// <summary> 영상 로드 Callback</summary>
	private void LoadVideo(string _name, UnityAction<string, VideoClip> _callback)
	{
		if (_name == string.Empty)
			return;

		ZResourceManager.Instance.Load(_name, (string _resName, VideoClip _videoClip) =>
		{
			if (_videoClip == null)
			{
				UICommon.OpenSystemPopup_One(ZUIString.ERROR,
						"영상 파일 로드 실패.", ZUIString.LOCALE_OK_BUTTON);
				return;
			}

			_callback?.Invoke(_resName, _videoClip);
		});
	}

	public void PlayVideoFinish(VideoClip _clip)
	{
		Frame.SetVideoClip(_clip);
		Frame.VideoPlayer.Play();
		Frame.VideoPlayer.frame = (long)Frame.VideoPlayer.frameCount;
	}
	#endregion

	#region Data
	public void SetNextVideoClip(VideoClip _videoClip) { NextVideo = _videoClip; }
	public void SetDefaultVideoClip(VideoClip _videoClip) { DefaultVideo = _videoClip; }
	#endregion
}
