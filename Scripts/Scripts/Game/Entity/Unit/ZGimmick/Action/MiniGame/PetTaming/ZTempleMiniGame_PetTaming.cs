using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

/// <summary>
/// 펫 길들이기 미니게임
/// </summary>
public class ZTempleMiniGame_PetTaming : ZTempleMiniGameBase
{
	public override E_TempleUIType ControlType => E_TempleUIType.None;

	[Header("성공 이펙트")]
	[SerializeField]
	public ParticleSystem Effect_Success;

	[Header("실패 이펙트")]
	[SerializeField]
	public ParticleSystem Effect_Fail;

	[Header("실패시 얻는 포인트 0 ~ 1")]
	[SerializeField]
	private float FailPoint;

	[Header("성공시 얻는 포인트 0 ~ 1")]
	[SerializeField]
	private float SuccessPoint;

	[Header("패턴 간격(s) : 5 ~ RepeatSecond 간격으로 랜덤하게 패턴 실행")]
	[SerializeField]
	private float RepeatSecond;

	[Header("패턴 시작후 정답을 맞출수있는 시간(s) : 최소 3초")]
	[SerializeField]
	private float AnserWaitSecond;

	// 미니게임 Hud
	private UISubHUDTemplePetTaming UIFrame;

	private bool _isComplete { get; set; }
	private bool _result { get; set; }
	private float _failValue { get; set; }
	private float _successValue { get; set; }
	private int _anserIndex { get; set; }
	private IDisposable _dispose { get; set; }

	/// <summary>
	/// 이니셜라이즈
	/// </summary>
	protected override void InitializeImpl()
	{
		base.InitializeImpl();

		_isComplete = false;
		_anserIndex = -1;
		_result = false;

		RepeatSecond = RepeatSecond <= 5 ? RepeatSecond = 5 : RepeatSecond;
		FailPoint = FailPoint <= 0 ? FailPoint = 0.4f : FailPoint;
		SuccessPoint = SuccessPoint <= 0 ? SuccessPoint = 3.4f : SuccessPoint;
		AnserWaitSecond = AnserWaitSecond <= 3 ? 3 : AnserWaitSecond;

		Gimmick.SetAnimParameter(E_AnimParameter.Idle_001);

		Effect_Success.Clear();
		Effect_Success.Stop();

		Effect_Fail.Clear();
		Effect_Fail.Stop();
	}

	/// <summary>
	/// 미니게임 스타트
	/// </summary>
	protected override void InvokeImpl()
	{
		base.InvokeImpl();
		ClearDisposable();

		ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

		var socket = Gimmick.GetSocket(GameDB.E_ModelSocket.Riding);

		myPc.ModelGo.transform.SetParent(socket, false);
		myPc.SetAnimParameter(E_AnimParameter.Riding_001, true);
		
		UIManager.Instance.Open<UISubHUDTemplePetTaming>((str, frame) =>
		{
			UIFrame = frame;
			UIFrame.Action_SelectButtonReturn = ReturnAnser;
			UIFrame.WaitTime = AnserWaitSecond;
			MakePattern();
		});
	}

	/// <summary>
	/// 미니게임 취소
	/// </summary>
	protected override void CancelImpl()
	{
		base.CancelImpl();
		ClearDisposable();
	}

	/// <summary>
	/// 패턴 만들기
	/// </summary>
	private void MakePattern()
	{
		float maxTime = 0;
		maxTime = RepeatSecond <= 5 ? 5 : RepeatSecond;
		maxTime++;

		if (true == CheckCompleteImpl())
			return;

		if(null != _dispose)
		{
			_dispose.Dispose();
			_dispose = null;
		}

		float nextTime = UnityEngine.Random.Range(5, maxTime);
		_dispose = Observable.Timer(TimeSpan.FromSeconds(nextTime)).Subscribe( _ =>
	    {
			if (true == CheckCompleteImpl())
				return;

			_anserIndex = UnityEngine.Random.Range(0, 4);
			UIFrame.ActiveSelectIndex(_anserIndex);
		}).AddTo(this);
	}
	
	/// <summary>
	/// 버튼을 선택하면 들어옴
	/// </summary>
	private void ReturnAnser(int selectIndex)
	{
		if (_anserIndex == -1)
			return;

		Gimmick.SetAnimParameter(E_AnimParameter.Taming_Rodeo);

		// 성공
		if(selectIndex == _anserIndex)
		{
			_successValue += SuccessPoint;
			if (1 < _successValue)
				_successValue = 1;

			Effect_Success.Play(true);
			UIFrame.SetTweenSuccess(_successValue);
		}
		// 실패
		else
		{
			_failValue += FailPoint;
			if (1 < _failValue)
				_failValue = 1;

			Effect_Fail.Play(true);
			UIFrame.SetTweenFail(_failValue);
		}

		MakePattern();
		_anserIndex = -1;
	}

	/// <summary>
	/// InvokeImpl -> 이후 스타트 들어옴
	/// </summary>
	protected override void StartMiniGame() {}

	/// <summary>
	/// 미니게임 완료 체크
	/// </summary>
	/// <returns></returns>
	protected override bool CheckCompleteImpl()
	{
		if (true == _isComplete)
			return true;

		if (1.0f <= _failValue || 1.0f <= _successValue)
		{
			ClearDisposable();

			_isComplete = true;

			_result = 1.0f <= _failValue ? false : true;

			float aniTime = Gimmick.GetAnimLength(E_AnimStateName.Taming_Rodeo);
			// 성공 애니
			if (_result)
			{
				Gimmick.SetAnimParameter(E_AnimParameter.Taming_Success);
				aniTime += Gimmick.GetAnimLength(E_AnimStateName.Taming_Success);
			}
			// 실패 애니
			else
			{
				Gimmick.SetAnimParameter(E_AnimParameter.Taming_Drop);
				aniTime += Gimmick.GetAnimLength(E_AnimStateName.Taming_Drop);
				aniTime += Gimmick.GetAnimLength(E_AnimStateName.Taming_Fail);
			}

			UIFrame.ShowResult(_result, aniTime);

			Invoke(nameof(CloseMinigame), aniTime);

			return true;
		}

		return false;
	}

	private void CloseMinigame()
	{
		CheckComplete();
	}

	private void ClearDisposable()
	{
		if(null != _dispose)
		{
			_dispose.Dispose();
			_dispose = null;
		}
	}

	/// <summary>
	/// Cancle 을 태우면 -> Stop 이 들어오고 -> End 가 들어옴
	/// </summary>
	protected override void EndMiniGame() 
	{
		ZPawnMyPc myPc = ZPawnManager.Instance.MyEntity;

		var modelGo = myPc.ModelGo;
		if (null == modelGo)
			return;

		modelGo.transform.SetParent(myPc.transform, true);
		modelGo.transform.localPosition = Vector3.zero;
		modelGo.transform.localRotation = Quaternion.identity;

		myPc.SetAnimParameter(E_AnimParameter.Riding_001, false);
	}
}
