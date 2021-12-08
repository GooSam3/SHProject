using UnityEngine;
using UnityEngine.UI;

public class UIFrameLoadingScreen : ZUIFrameBase
{
    [SerializeField] private Slider mProgressBar = null;

    //-----------------------------------------------------
    protected override void OnInitialize()
    {
        base.OnInitialize();
        mProgressBar = GetComponentInChildren<Slider>();
    }

    //-----------------------------------------------------
    public void DoUIFrameLoadingScreen(float _Progress)
    {
        mProgressBar.value = _Progress;
    }

	protected override void OnShow(int _LayerOrder)
	{
		base.OnShow(_LayerOrder);
        if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
            gainSystem.SetPlayState(false);
    }

    protected override void OnHide()
	{
		base.OnHide();
        if (UIManager.Instance.Find<UIGainSystem>(out var gainSystem))
            gainSystem.SetPlayState(true);
    }

}
