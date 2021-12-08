using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UnityEngine.UI;

public class UISubHUDTemplePetTaming : ZUIFrameBase
{
    #region #### Serialize Field ####
    [Header("#### Serialize Field ####")]
    [SerializeField]
    private GameObject GameUI;
    [SerializeField]
    private GameObject SliderUI;

    [Header("활성화시 유저가 터치해야하는 버튼들")]
    [SerializeField]
    private List<UIPetTamingButton> ActiveButtons;

    [Header("테이밍 게이지")]
    [SerializeField]
    private Slider SliderTaming;

    [Header("테이밍 실패 게이지")]
    [SerializeField]
    private Slider SliderFail;

    [Header("게임 결과 Object - Success")]
    [SerializeField]
    private GameObject ObjResult_Success;

    [Header("게임 결과 Object - Fail")]
    [SerializeField]
    private GameObject ObjResult_Fail;
    #endregion

    // 연출 Tween
    private Tween _tweener;

    // 정답을 맞출때까지 기다려주는 시간
    [HideInInspector] public float WaitTime;
    // 정답 선택 Antion
    [HideInInspector] public Action<int> Action_SelectButtonReturn;
    // 현재 정답 번호
    [HideInInspector] public int AnserIndex;

    private IDisposable _disposable;

    protected override void OnInitialize()
	{
		base.OnInitialize();

        for (int index = 0; index < ActiveButtons.Count; index++)
		{
            ActiveButtons[index].SetDefault();
            ActiveButtons[index].ButtonIndex = index;
        }

        if (null != _tweener)
            _tweener.Kill();

        GameUI.SetActive(true);
        SliderUI.SetActive(true);

        SliderTaming.value = 0;
        SliderFail.value = 0;

        ObjResult_Success.SetActive(false);
        ObjResult_Fail.SetActive(false);

        ResetButtonClick();
    }

	protected override void OnHide()
	{
        if(null != _tweener)
            _tweener.Kill();

        if (null != _disposable)
        {
            _disposable.Dispose();
            _disposable = null;
        }
    }

    /// <summary>
    /// 인덱스 받고 해당 인덱스의 버튼 활성화
    /// </summary>
    /// <param name="index"></param>
    public void ActiveSelectIndex(int index)
	{
        if (ActiveButtons.Count < index)
		{
            ZLog.LogError(ZLogChannel.System, "UISubHUDTemplePetTaming error : Random Index Overflow");
            return;
        }

        if(null != _disposable)
		{
            _disposable.Dispose();
            _disposable = null;
        }

        for(int mindex = 0; mindex < ActiveButtons.Count; mindex++)
		{
            if (mindex == index)
                ActiveButtons[mindex].PlayActiveButton();
            else
                ActiveButtons[mindex].SetActiveButtonInteraction(false);
        }

        AnserIndex = index;

        _disposable = Observable.Timer(TimeSpan.FromSeconds(WaitTime)).Subscribe( _ =>
        {
            OnClick_ActiveButton(ActiveButtons.Count + 1);
        }).AddTo(this);
	}

    /// <summary>
    /// 패턴 버튼 선택
    /// </summary>
    /// <param name="clickButton"></param>
    public void OnClick_ActiveButton(int index)
	{
        if (null == Action_SelectButtonReturn)
            return;

        if (null != _disposable)
        {
            _disposable.Dispose();
            _disposable = null;
        }

        Action_SelectButtonReturn(index);
        ActiveButtons[AnserIndex].SetButtonResult(index == AnserIndex);
        ResetButtonClick();
    }

    private void ResetButtonClick()
	{
        foreach(var button in ActiveButtons)
		{
            button.SetActiveButtonInteraction(false);
		}
	}

    /// <summary>
    /// 결과 보여주기
    /// </summary>
    public void ShowResult(bool success, float aniTime)
	{
        ObjResult_Success.SetActive(success);
        ObjResult_Fail.SetActive(!success);

        GameUI.SetActive(false);
        SliderUI.SetActive(false);

        if (null != _disposable)
        {
            _disposable.Dispose();
            _disposable = null;
        }

        _disposable = Observable.Timer(TimeSpan.FromSeconds(aniTime)).Subscribe(_ =>
        {
            UIManager.Instance.Close("UISubHUDTemplePetTaming");
        }).AddTo(this);
    }

    /// <summary>
    /// 패턴 맞추기 성공
    /// </summary>
	public void SetTweenSuccess(float targetValue)
    {
        float beforeValue;
        beforeValue = SliderTaming.value;

        if (null != _tweener)
            _tweener.Kill();

        if (SliderTaming.value == targetValue)
            return;

        _tweener = SliderTaming.DOValue(targetValue, 1f).OnComplete(() => _tweener.Kill());
    }

    /// <summary>
    /// 패턴 맞추기 실패
    /// </summary>
    public void SetTweenFail(float targetValue)
	{
        float beforeValue;
        beforeValue = SliderFail.value;

        if (null != _tweener)
            _tweener.Kill();

        if (beforeValue == targetValue)
            return;

        _tweener = SliderFail.DOValue(targetValue, 1f).OnComplete(() => _tweener.Kill());
    }

	private void OnDisable()
	{
        if (null != _disposable)
        {
            _disposable.Dispose();
            _disposable = null;
        }

        if (null != _tweener)
            _tweener.Kill();
    }
}
