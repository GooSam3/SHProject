using GameDB;
using System;
using System.Collections;
using UniRx;
using UnityEngine;

/// <summary> On/Off 전환 스위치 일정 시간마다 자동 </summary>
public class ZGA_AutoStart : ZGimmickActionBase
{
    [Header("Close 유지시간(s) - 최초 1번은 무시됨")]
    [SerializeField]
    private float CloseWaitTime = 0;

    [Header("Open 애니메이션 속도")]
    [SerializeField]
    private float OpenSpeed = 1;

    [Header("Open 유지시간(s)")]
    [SerializeField]
    private float OpenWaitTime = 0;

    [Header("Close 애니메이션 속도")]
    [SerializeField]
    private float CloseSpeed = 1;

    // 자동문 스탑/플레이 체크
    private ReactiveProperty<bool> IsFreezeStop = new ReactiveProperty<bool>();
    // 애니메이션 시간
    private float aniTime;
    // 최초 1번, 무한 코루틴 실행 여부
    private bool _isStartCoroutine = false;
    // Disposable
    private IDisposable _freezeDispose;
    // 정지 시간 비교
    private DateTime startTime, endTime;
    // 정지했을당시 애니메이션 속도
    private float _stopAniSpeed;
    // 정지이후 다시 시작했을때, 기존 애니메이션 속도
    private bool _isEndDefTime = false;

    protected override void InitializeImpl()
	{
        // 기본 셋팅
        SetDefault();

        _freezeDispose = IsFreezeStop.ObserveEveryValueChanged(d => d.Value).Subscribe( value =>
        {
            if (value == true)
			{
                _isEndDefTime = false;
                startTime = DateTime.Now;
                StopCoroutine(nameof(PlayDefTimeAni));
            }
            else
			{
                if (startTime == DateTime.MinValue)
                    return;

                endTime = DateTime.Now;
                TimeSpan timeDiff = endTime - startTime;
                StartCoroutine(PlayDefTimeAni((float)timeDiff.TotalSeconds));
            }
        }).AddTo(this);
    }

    private void SetDefault()
	{
        StopAllCoroutines();
        if (null != _freezeDispose)
        {
            _freezeDispose.Dispose();
            _freezeDispose = null;
        }

        IsFreezeStop.Value = false;
        aniTime = Gimmick.GetAnimLength(E_AnimStateName.Start_001);
        _isEndDefTime = true;
        startTime = DateTime.MinValue;
        OpenSpeed = OpenSpeed <= 0 ? 1f : OpenSpeed;
        CloseSpeed = CloseSpeed <= 0 ? 1f : CloseSpeed;
        //CloseWaitTime = CloseWaitTime <= 1 ? 1f : CloseWaitTime;
        //OpenWaitTime = OpenWaitTime <= 1 ? 1f : OpenWaitTime;
    }

	private void OnDisable()
	{
        StopAllCoroutines();
        if(null != _freezeDispose)
		{
            _freezeDispose.Dispose();
            _freezeDispose = null;
        }
	}

	protected override void InvokeImpl()
	{
        IsFreezeStop.Value = false;
        if (false == _isStartCoroutine)
            StartCoroutine(nameof(AutoDoorRoutine));
    }

	protected override void CancelImpl()
	{
        IsFreezeStop.Value = true;
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 0f);
    }

	protected override void DestroyImpl()
	{
		base.DestroyImpl();

        StopAllCoroutines();
        if (null != _freezeDispose)
        {
            _freezeDispose.Dispose();
            _freezeDispose = null;
        }
    }

    IEnumerator PlayDefTimeAni(float second)
	{
        Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, _stopAniSpeed);
        yield return new WaitForSeconds(second);
        startTime = DateTime.MinValue;
        _isEndDefTime = true;
    }

    IEnumerator AutoDoorRoutine()
	{
        while (true)
		{
            // 최초 1번 체크
            if(true == _isStartCoroutine)
                yield return new WaitForSeconds(CloseWaitTime);

            _isStartCoroutine = true;

            yield return new WaitUntil(() => !IsFreezeStop.Value && _isEndDefTime);

            Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, 0f);
            Gimmick.SetAnimParameter(E_AnimParameter.Start_001);
            Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, 1f * OpenSpeed);
            _stopAniSpeed = 1f * OpenSpeed;

            yield return new WaitForSeconds(aniTime / OpenSpeed);

            yield return new WaitUntil(() => !IsFreezeStop.Value && _isEndDefTime);

            yield return new WaitForSeconds(OpenWaitTime);

            yield return new WaitUntil(() => !IsFreezeStop.Value && _isEndDefTime);

            Gimmick.PlayByNormalizeTime(E_AnimStateName.Start_001, 1f);
            Gimmick.SetAnimParameter(E_AnimParameter.GimmickAnimSpeed, -1f * CloseSpeed);
            _stopAniSpeed = -1f * CloseSpeed;

            yield return new WaitForSeconds(aniTime / CloseSpeed);

            yield return new WaitUntil(() => !IsFreezeStop.Value && _isEndDefTime);
        }
    }
}