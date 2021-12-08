using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Playables;
/// <summary> 컷신을 플레이한다. </summary>
public class TutorialSequence_PlayCutScene : TutorialSequence_None
{
	private PlayableDirector Director = null;
	private string CutSceneName { get { return TutorialTable.GuideParams[0]; } }

	private bool IsSceneHierarchy { get { return 1 < TutorialTable.GuideParams.Count ? bool.Parse(TutorialTable.GuideParams[1]) : false; } }

	private const string CharacterParentName = "PC";

	private bool IsEndProgress = false;
	
	/// <summary> 가이드 시작 </summary>
	protected override void StartGuide()
	{
		ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = true;

		UICommon.FadeInOut(() =>
		{
			if(null == this)
			{
				DestroyDirector();
				return;
			}
			//ui 가리기
			UIManager.Instance.HideAll();

			if (IsSceneHierarchy)
			{
				var go = GameObject.Find(CutSceneName);

				if (go == null)
				{
					ZLog.LogError(ZLogChannel.Quest, $"[{CutSceneName}]해당 컷신을 로드하지 못함.");
					End();
					return;
				}
				InitCutScene(go.GetComponent<PlayableDirector>());
				PlayCutScene();
			}
			else
			{
				ZResourceManager.Instance.Load(CutSceneName, (string cutSceneName, GameObject cutScene) =>
				{
					if (cutScene == null)
					{
						ZLog.LogError(ZLogChannel.Quest, $"[{CutSceneName}]해당 컷신을 로드하지 못함.");
						End();
						return;
					}

					if (null == this)
					{
						DestroyDirector();
						return;
					}

					GameObject go = Instantiate(cutScene);
					//go.transform.SetParent(gameObject.transform);

					InitCutScene(go.GetComponent<PlayableDirector>());
					PlayCutScene();
				});
			}		

		}, E_UIFadeType.FadeIn, 1f);
	}

	private void InitCutScene(PlayableDirector director)
	{
		Director = director;
		Director.stopped += HandleStopped;
		Director.paused += HandleStopped;

		// TODO :: 임시 컷신 캐릭터 처리
		var timeline = Director.playableAsset as TimelineAsset;

		ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

		string resourceFileName = string.Empty;
		if (DBCharacter.TryGet(myPc.TableId, out var characterTable))
		{
			if(DBResource.TryGet(characterTable.ResourceID, out var resourceTable))
			{
				resourceFileName = resourceTable.ResourceFile;
			}
		}

		string resourceName = myPc.ResourceTable.ResourceFile;
		
		foreach (var track in timeline.GetOutputTracks())
		{
			if (null == track.parent || null == track.timelineAsset)
				continue;

			if (false == track.parent.name.Equals(CharacterParentName))
				continue;

			var binding = Director.GetGenericBinding(track);

			if(binding is Animator anim)
			{
				anim.gameObject.SetActive(binding.name.Equals(resourceFileName));
			}
		}
	}

	private void PlayCutScene()
	{
		Director.time = 0f;
		Director.Play();

		SetBlockScereen(true, false);

		Owner.ShowSkip();

		UICommon.FadeInOut(() =>
		{
			//Wrap Mode가 Hold면 Stopped 이벤트가 들어오지 않음..따로 체크
			CancelInvoke(nameof(InvokeCheckEnded));
			InvokeRepeating(nameof(InvokeCheckEnded), 0f, 0.1f);
		}, E_UIFadeType.FadeOut, 1f);
	}

	private void InvokeCheckEnded()
	{
		if(null == Director)
		{
			CancelInvoke(nameof(InvokeCheckEnded));
			return;
		}

		if(Director.time >= (Director.duration - 1f))
		{
			EndProgress();
		}
	}

	private void HandleStopped(PlayableDirector director)
	{
		EndProgress();
	}

	private void EndProgress()
	{
		if (true == IsEndProgress)
			return;

		CancelInvoke(nameof(InvokeCheckEnded));

		IsEndProgress = true;
		UICommon.FadeInOut(() =>
		{
			Director.time = Director.duration;
			End();
		}, E_UIFadeType.FadeIn, 1f);
	}

	private void End()
	{		
		DestroyDirector();

		Owner.HideSkip();

		UICommon.FadeInOut(() =>
		{
			UIManager.Instance.Find<UIFrameHUD>().RefreshSubHud(ZGameModeManager.Instance.Table.StageType);
			ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
			//ui 노출
			EndSequence(false);
		}, E_UIFadeType.FadeOut, 1f);
	}

	private void OnDestroy()
	{
		DestroyDirector(true);
	}

	private void DestroyDirector(bool bDestroy = false)
	{
		if(null != Director)
		{
			Director.stopped -= HandleStopped;
			Director.paused -= HandleStopped;
			if (true == bDestroy)
			{
				Director.time = Director.duration;
			}	
		}
		if(false == IsSceneHierarchy)
		{
			if (null != Director)
				GameObject.Destroy(Director.gameObject);
		}

		if (true == bDestroy)
		{
			if (null != UIManager.Instance)
			{
				UICommon.FadeInOut(() =>
				{
					UIManager.Instance.Find<UIFrameHUD>().RefreshSubHud(ZGameModeManager.Instance.Table.StageType);
					ZPawnManager.Instance.MyEntity.IsBlockMoveMyPc = false;
				}, E_UIFadeType.FadeOut, 1f);
			}
		}

		Director = null;
	}
}