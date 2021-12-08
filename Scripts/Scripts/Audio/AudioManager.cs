using ClockStone;
using GameDB;
using System.Collections.Generic;
using UnityEngine;
using Zero;

public class AudioManager : Singleton<AudioManager>
{
	public AudioController Controller { get; private set; }
	public IAudioHandler Handler { get; private set; }
	public IAudioHelper Helper { get; private set; }
	public IAudioLoader Loader { get; private set; }
	public bool Enable { get; private set; }

	// to do : 상수 Constant 클래스가 생성되면 옮길 것
	public const float SFX_PLAY_IN_RANGE = 80 * 80;


	protected override void Init()
	{
		base.Init();

		ZGameOption.Instance.OnOptionChanged -= OnChangedOption;
		ZGameOption.Instance.OnOptionChanged += OnChangedOption;

		OnChangedOption(ZGameOption.OptionKey.Option_Bgm);
		OnChangedOption(ZGameOption.OptionKey.Option_SfxSound);

		Initialize();
	}

	public void Initialize()
	{
		Handler = GetComponent<AudioPlayer>();
		Helper = GetComponent<AudioHelper>();
		Loader = GetComponent<AudioLoader>();

		AudioController.musicParent = transform;
		AudioController.ambienceParent = transform;

		Enable = true;
	}

	private void OnChangedOption(ZGameOption.OptionKey changedKey)
    {
		switch(changedKey)
        {
			case ZGameOption.OptionKey.Option_Bgm:
				AudioController.SetCategoryVolume((E_SoundType.BGM).ToString(), Mathf.Clamp(ZGameOption.Instance.BGMSound, 0.0001f, 1f));
				break;
			case ZGameOption.OptionKey.Option_SfxSound:
				var sfxVolume = ZGameOption.Instance.SFXSound;
				AudioController.SetCategoryVolume((E_SoundType.Effect).ToString(), Mathf.Clamp(sfxVolume, 0.0001f, 1f));
				AudioController.SetCategoryVolume((E_SoundType.UI).ToString(), Mathf.Clamp(sfxVolume, 0.0001f, 1f));
				AudioController.SetCategoryVolume((E_SoundType.Normal).ToString(), Mathf.Clamp(sfxVolume, 0.0001f, 1f));
				break;
			case ZGameOption.OptionKey.Option_AlramSound:
				AudioController.SetCategoryVolume((E_SoundType.Alarm).ToString(), Mathf.Clamp(ZGameOption.Instance.AlramSound, 0.0001f, 1f));
				break;
        }
    }

	/// <summary>
	/// 오디오 실행 함수
	/// 추후 Res Idx만 넘겨주면 테이블에서 데이터를 참조하여 AudioFile을 자동으로 구성하도록 변경 예정
	/// AudioType에 맞춰 Audio Play API를 호출함
	/// <paramref name="audioFile"/>Audio File</param>
	/// </summary>
	public void Play(uint soundID, Transform parent = null, Vector3? worldPos = null)
	{
		if (!Enable || soundID == 0)
			return;

		SetAudioFile(soundID, (audioItem) => 
		{
			AudioObject playingObject = null;

			if (null == audioItem)
			{
				Debug.LogError("Not found File from PlayAudio");
				return;
			}

			switch (audioItem.SoundType)
			{
				case E_SoundType.UI:
				case E_SoundType.Alarm:
					playingObject = PlayAudio(audioItem, this.transform);

					Handler.SetPrimaryPitchAndBlend(playingObject, 1f, 0f);
					break;

				case E_SoundType.BGM:
					audioItem.subItems[0].ClipStartTime = audioItem.StartTime;
					playingObject = PlayMusic(audioItem, null);

					Handler.SetPrimaryPitchAndBlend(playingObject, 1f, 0f);
					Handler.SetSpatialBlend(playingObject, 0f);
					break;

				case E_SoundType.Effect:
				case E_SoundType.Normal:
					audioItem.subItems[0].ClipStartTime = audioItem.StartTime;
					playingObject = PlayAudio(audioItem, parent, worldPos);

					Handler.SetPrimaryPitchAndBlend(playingObject, audioItem.SpeedRate, 1f);
					break;
			}

			if (audioItem.FadeState)
				Fade(audioItem, playingObject);
		});		
	}

	private void SetAudioFile(uint soundID, System.Action<AudioItem> loadCB)
	{
		Loader.AddAudioFileLoadAsync(soundID, (audioItem) => 
		{
			if (null == audioItem)
			{
				return;
			}

			// to do 상수 클래스 추가되면 관련 변수 추가
			audioItem.MaxInstanceCount = 7;
			audioItem.MinTimeBetweenPlayCalls = 0.1f;

			if (AudioTest.mTestAudio)
			{
				TestAudioForm Form = AudioTest.Instance.GetFile();

				audioItem.Volume = Form.Volume;
				audioItem.Delay = Form.Delay;
				audioItem.StartTime = Form.StartTime;
				audioItem.SpeedRate = Form.SpeedRate;
				audioItem.FadeState = Form.FadeState;
				audioItem.FadeIn = Form.FadeIn;
				audioItem.FadeOut = Form.FadeOut;
				audioItem.SoundType = Form.Type;
				audioItem.Loop = Form.Loop ? AudioItem.LoopMode.LoopSubitem : AudioItem.LoopMode.DoNotLoop;
				loadCB?.Invoke(audioItem);
			}

			if (audioItem.SoundType == E_SoundType.BGM)
			{
				audioItem.Loop = AudioItem.LoopMode.LoopSubitem;
				audioItem.Delay = 0.5f;
				audioItem.StartTime = 0.5f;
			}
			else
				audioItem.Loop = AudioItem.LoopMode.DoNotLoop;

			loadCB?.Invoke(audioItem);
		});
	}

	private void Fade(AudioItem audioFile, AudioObject playObject)
	{
		if (audioFile.FadeIn > 0) playObject.FadeIn(audioFile.FadeIn);
		if (audioFile.FadeOut > 0) playObject.FadeOut(audioFile.FadeOut);
	}

	/// <summary>
	/// 원하는 오디오 타입을 제외한 다른 타입의 사운드를 모두 Fade Out 처리
	/// <paramref name="audioType"/>덕킹을 하려는 사운드 타입</param>
	/// <paramref name="fadeTime"/></param>
	/// </summary>
	public void DuckingAllAudio(E_SoundType audioType, float fadeTime)
	{
		List<AudioObject> audioList = AudioController.GetPlayingAudioObjects();

		if (audioList.Count == 0)
		{
			Debug.LogWarning("No audio category with name " + name);
			return;
		}

		for (int i = 0; i < audioList.Count; i++)
			if (audioList[i].category.Name != audioType.ToString())
				audioList[i].FadeOut(fadeTime);
	}

	public void StopAllAudio(float fadeOutLength = 0.5f)
	{
		AudioController.StopAll(fadeOutLength);
	}

	public void StopAudio(E_SoundType audioType, string fileName)
	{
		switch (audioType)
		{
			case E_SoundType.BGM:
				AudioController.StopMusic(0.5f);
				break;

			default:
				AudioController.Stop(fileName, 0.1f);
				break;
		}
	}

	private AudioObject PlayAudio(AudioItem audioFile, Transform parent = null, Vector3? worldPosition = null)
	{
		if (null == audioFile)
			return null;

		Handler.ApplyAudioFile(audioFile);

		if (parent) return AudioController.Play(audioFile.Name, null != worldPosition ? worldPosition.Value : parent.position, parent, audioFile.Volume, audioFile.Delay, audioFile.StartTime);
		else return AudioController.Play(audioFile.Name, audioFile.Volume, audioFile.Delay, audioFile.StartTime);
	}
	private AudioObject PlayMusic(AudioItem audioFile, Transform parent)
	{
		if (audioFile == null)
			return null;

		Handler.ApplyAudioFile(audioFile);

		if (parent) return AudioController.PlayMusic(audioFile.Name, parent, audioFile.Volume, audioFile.Delay, audioFile.StartTime);
		else return AudioController.PlayMusic(audioFile.Name, audioFile.Volume, audioFile.Delay, audioFile.StartTime);
	}

	/// <summary>
	/// 3D 사운드 실행 함수
	/// 리스너와 AudioManager의 거리에 따라 사운드 실행 유무를 판단한다.
	/// <paramref name="audioFile"/>Audio File</param>
	/// <paramref name="pos"/>리스너의 월드 포지션 값</param>
	/// </summary>
	public void PlaySFX(uint soundID, Vector3? pos = null)
	{
		if (!Enable)
			return;

		if (null != pos)
		{
			//if ((AudioManager.Instance.transform.position - pos.Value).sqrMagnitude > SFX_PLAY_IN_RANGE)
			//	return;

			//카메라 위치에 따라 처리되야함.
			if (false == CameraManager.hasInstance)
				return;

			if (null == CameraManager.Instance.Main)
				return;
			
			if ((CameraManager.Instance.Main.transform.position - pos.Value).sqrMagnitude > SFX_PLAY_IN_RANGE)
				return;
			
		}

		Play(soundID, CameraManager.Instance.Main.transform, pos);
	}
}