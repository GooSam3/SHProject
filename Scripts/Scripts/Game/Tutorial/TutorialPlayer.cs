using GameDB;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary> 튜토리얼 플레이어 </summary>
public class TutorialPlayer : MonoBehaviour
{	
	public bool IsPlaying { get { return 0 < m_queSequence.Count || null != CurrentSequence; } }

	public Quest_Table QuestTable { get; private set; } = null;

	public uint QuestTid { get { return QuestTable?.QuestID ?? 0; } }

	private Queue<TutorialSequence_None> m_queSequence = new Queue<TutorialSequence_None>();

	private TutorialSequence_None CurrentSequence;

	private UIFrameTutorial TutorialUI = null;

	public CUIFrameBase LastOpenedUIFrame { get; private set; }

	public void StartTutorial(uint questTid)
	{
		if (true == IsPlaying)
		{
			if (null != QuestTable)
			{
				Stop($"[{QuestTable.QuestID}]이미 튜토리얼 진행중이다.", false);
				return;
			}
		}

		QuestTable = DBQuest.GetQuestData(questTid);

		if (null == QuestTable)
		{
			Stop($"[{questTid}] 해당 퀘스트 Tid가 없다.");
			return;
		}

		if (QuestTable.CompleteCheck != E_CompleteCheck.Tutorial)
		{
			Stop($"[{questTid}] 튜토리얼 퀘스트가 아니다.");
			return;
		}

		InvokeStartTutorial();
	}

	private void InvokeStartTutorial()
	{
		CancelInvoke(nameof(InvokeStartTutorial));

		var hud = UIManager.Instance.Find<UIFrameHUD>();		

		//로딩중이면 대기
		if(null == hud || true == ZGameManager.Instance.IsEnterGameLoading)
		{
			Invoke(nameof(InvokeStartTutorial), 0.1f);
			return;
		}

		//내 캐릭이 없다면 대기
		if(null == ZPawnManager.Instance.MyEntity)
		{
			Invoke(nameof(InvokeStartTutorial), 0.1f);
			return;
		}

		//퀘스트 창열려있으면 대기
		var reward = UIManager.Instance.Find<UIFrameQuest>();

		if(null != reward && reward.Show)
		{
			Invoke(nameof(InvokeStartTutorial), 0.1f);
			return;
		}

		ZPawnManager.Instance.MyEntity.StopAllAction();
		ZPawnManager.Instance.MyEntity.StartDefaultAI();


		hud.RefreshSubHud(E_StageType.Tutorial);

		//Sequence 생성
		if (false == MakeTutorialSequence())
		{
			Stop($"이미 Sequence가 만들어져있다.", false);
			return;
		}	

		//ui 생성
		CreateUI(() =>
		{
			//Sequence 실행
			PlayNextSequence();

			mEventTutorialStarted?.Invoke(QuestTable.QuestID);
		});
	}

	/// <summary> 각 시퀀스 완료 </summary>
	private void OnFinishSequence(bool bSkip)
	{
		if(false == bSkip)
		{
			PlayNextSequence();
		}
		else
		{
			CompleteTutorial();
		}
	}

	/// <summary> 다은 Sequence를 실행한다. </summary>
	private void PlayNextSequence()
	{
		if (0 >= m_queSequence.Count)
		{
			CompleteTutorial();
			return;
		}

		CurrentSequence = m_queSequence.Dequeue();

		//다음 스텝이 다이얼로그라면 패스
		if(CurrentSequence.TutorialType != E_TutorialType.Dialogue)
		{
			HideDialogueUI();
		}
		HideGuideUI();
		HideFocusUI();
		HideSkip();

		CurrentSequence.PlaySequence();
	}

	/// <summary> 튜토리얼 완료 요청 </summary>
	private void CompleteTutorial()
	{
		if (null == QuestTable)
		{
			Stop("Quest_Table이 셋팅되지 않았다.");
			return;
		}

		ZPawnManager.Instance.MyEntity?.StopAllAction();
		ZPawnManager.Instance.MyEntity?.StartDefaultAI();

		CancelInvoke(nameof(CompleteTutorial));

		//소켓이 끊어져있다면 다시 요청
		if (false == ZWebManager.Instance.WebGame.IsUsable || true == ZGameManager.Instance.IsEnterGameLoading)
		{
			Invoke(nameof(CompleteTutorial), 0.1f);
			return;
		}

		UIFrameQuest frameQuest = UIManager.Instance.Find<UIFrameQuest>();

		if (null == frameQuest)
		{
			Invoke(nameof(CompleteTutorial), 0.5f);
			return;			
		}

		HideDialogueUI();
		HideGuideUI();
		HideFocusUI();
		HideSkip();

		CurrentSequence = null;

		frameQuest.QuestChecker.EventTutorialClear();

		DestroyUI();

		// 튜토리얼 종료 이벤트
		mEventTutorialEnded?.Invoke(QuestTable.QuestID);		

		TutorialSystem.Instance.ClearTutotial(QuestTable.QuestID);
	}

	public void TutorialSkip()
	{
		if(null != QuestTable)
		{
			ZLog.Log(ZLogChannel.Quest, $"[{QuestTable.QuestID}] 튜토리얼 스킵!!");
		}
		
		CompleteTutorial();
	}

	private void Stop(string message, bool bClear = true)
	{
		ZLog.LogError(ZLogChannel.Quest, message);

		if (false == bClear)
			return;

		QuestTable = null;
		mEventTutorialStarted = null;
		mEventTutorialEnded = null;
		CurrentSequence = null;
		m_queSequence.Clear();

		DestroyUI();
	}

	/// <summary> ui 생성 </summary>
	private void CreateUI(Action onFinish)
	{
		if (null != TutorialUI)
			return;

		ZResourceManager.Instance.Instantiate<UIFrameTutorial>(nameof(UIFrameTutorial), (assetName, asset) =>
		{
			TutorialUI = asset;
			TutorialUI.gameObject.SetActive(true);
			TutorialUI.transform.SetParent(UIManager.Instance.transform, false);
			onFinish?.Invoke();
		});
	}

	/// <summary> ui 제거 </summary>
	private void DestroyUI() 
	{
		if (null != TutorialUI)
			GameObject.Destroy(TutorialUI.gameObject);

		TutorialUI = null;
	}

	private void OnDestroy()
	{
		DestroyUI();
	}

	#region ===== :: 이벤트 :: =====
	private Action<uint> mEventTutorialStarted;
	private Action<uint> mEventTutorialEnded;

	/// <summary> 튜토리얼 시작시 이벤트 </summary>
	public void DoAddEventTutorialStarted(Action<uint> action)
	{
		DoRemoveEventTutorialStarted(action);
		mEventTutorialStarted += action;
	}

	/// <summary> 튜토리얼 시작시 이벤트 제거 </summary>
	public void DoRemoveEventTutorialStarted(Action<uint> action)
	{
		mEventTutorialStarted -= action;
	}

	/// <summary> 튜토리얼 시작시 이벤트 </summary>
	public void DoAddEventTutorialEnded(Action<uint> action)
	{
		DoRemoveEventTutorialEnded(action);
		mEventTutorialEnded += action;
	}

	/// <summary> 튜토리얼 시작시 이벤트 제거 </summary>
	public void DoRemoveEventTutorialEnded(Action<uint> action)
	{
		mEventTutorialEnded -= action;
	}

	#endregion

	/// <summary> Tutorial Table 기반으로 Tutorial Sequence를 생성한다. </summary>
	private bool MakeTutorialSequence()
	{
		if (0 < m_queSequence.Count)
			return false;

		foreach (var table in GameDBManager.Container.Tutorial_Table_data)
		{
			if (table.Value.QuestID != QuestTable.QuestID)
				continue;

			if (table.Value.AttributeType != E_UnitAttributeType.None && table.Value.AttributeType != ZPawnManager.Instance.MyEntity.OriginalAttributeType)
				continue;

			var seq = CreateSequence(table.Value, OnFinishSequence);
			
			if (null == seq)
				continue;

			m_queSequence.Enqueue(seq);
		}

		return true;
	}

	/// <summary> Tutorial Table의 GuideType으로 해당 클래스를 생성한다. </summary>
	private TutorialSequence_None CreateSequence(Tutorial_Table table, Action<bool> onFinish)
	{		
		var assembly = Assembly.Load(new System.Reflection.AssemblyName("Assembly-CSharp"));

		//var sequenceType = Activator.CreateInstance(assembly.GetType($"TutorialSequence_{table.GuideType}"));

		var type = assembly.GetType($"TutorialSequence_{table.GuideType}");

		if(null != type)
		{
			var sequenceType = gameObject.AddComponent(type);

			if (sequenceType is TutorialSequence_None sequence)
			{
				sequence.Initialize(this, table, onFinish);
				return sequence;
			}
			else
			{
				ZLog.LogError(ZLogChannel.Quest, $"[{table.TutorialID}]의 GuideType[{table.GuideType}]이 존재하지 않는다.");
			}
		}
		else
		{
			ZLog.LogError(ZLogChannel.Quest, $"[{table.TutorialID}]의 GuideType[{table.GuideType}]이 존재하지 않는다.");
		}


		return null;
	}

	#region ===== :: UI :: =====
	/// <summary> 다이얼로그 ui 활성화 </summary>
	public void ShowDialogueUI(string name, string text, string imageName, Action onFinish)
	{
		TutorialUI.SetDialogue(true, name, text, imageName, onFinish);
	}

	/// <summary> 다이얼로그 ui 바활성화 </summary>
	public void HideDialogueUI()
	{
		TutorialUI.SetDialogue(false);
	}

	/// <summary> 가이드 ui 활성화 </summary>
	public void ShowGuideUI(string name, string text, string imageName)
	{
		TutorialUI.SetGuide(true, name, text, imageName);
	}

	/// <summary> 가이드 ui 바활성화 </summary>
	public void HideGuideUI()
	{
		TutorialUI.SetGuide(false);
	}

	/// <summary> 포커스 ui 활성화 </summary>
	public void FocusUI(GameObject target, Transform highlightObject, Action onClick, Action<PointerEventData> onFocusPointUp = null, Action<PointerEventData> onFocusPointDown = null)
	{
		TutorialUI.FocusUI(target, highlightObject, onClick, onFocusPointUp, onFocusPointDown);
	}

	/// <summary> 포커스 ui 비활성화 </summary>
	public void HideFocusUI()
	{
		TutorialUI.HideFocusUI();
	}

	public void ShowSkip()
	{
		TutorialUI.ShowSkip();
	}

	public void HideSkip()
	{
		TutorialUI.HideSkip();
	}

	public void SetBockScreen(bool bActive, bool bAlpha)
	{
		TutorialUI.SetBockScreen(bActive, bAlpha);
	}

	/// <summary> 튜토리얼중 마지막에 열린 패널 셋팅 </summary>
	public void SetLastOpenedUIFrame(CUIFrameBase frame)
	{		
		LastOpenedUIFrame = frame;
	}
	#endregion
}