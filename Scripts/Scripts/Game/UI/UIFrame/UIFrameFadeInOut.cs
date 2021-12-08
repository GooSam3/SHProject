using System;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public class UIFrameFadeInOut : ZUIFrameBase
{
    #region UI Variable
    [SerializeField] CanvasGroup canvasGroup;
    #endregion

    private Tweener mTweener;
    public void FadeInOut(Action _cal, E_UIFadeType _fadeType, float _duration)
    {
        canvasGroup.blocksRaycasts = true;

        mTweener?.Kill(false);

        float from = canvasGroup.alpha;
        float to = _fadeType == E_UIFadeType.FadeIn ? 1f : 0f;

        float offset = Mathf.Abs(to - from);

        _duration *= offset;

        canvasGroup.blocksRaycasts = true;
        mTweener = DOTween.To(() => from, (value) => canvasGroup.alpha = value, to, _duration).OnComplete(() =>
        {
            if(_fadeType == E_UIFadeType.FadeOut)
			{
                canvasGroup.blocksRaycasts = false;
                UIManager.Instance.Close<UIFrameFadeInOut>();
            }
                

            canvasGroup.alpha = to;

            _cal?.Invoke();
        });
    }
}
