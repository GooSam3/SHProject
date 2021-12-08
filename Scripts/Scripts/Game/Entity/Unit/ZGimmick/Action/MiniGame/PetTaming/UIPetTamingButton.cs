using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIPetTamingButton : MonoBehaviour
{
    [SerializeField]
    private UISubHUDTemplePetTaming OwnerHud;

    [Header("버튼 활성화 SliderGage")]
    [SerializeField]
    private Image ImageGage;

    [Header("Effect - 활성화")]
    [SerializeField]
    private ParticleSystem Effect_On;

    [Header("Effect - 선택")]
    [SerializeField]
    private ParticleSystem Effect_Click;

    [Header("Effect - 성공")]
    [SerializeField]
    private GameObject Object_Success;

    [Header("Effect - 실패")]
    [SerializeField]
    private GameObject Object_Fail;

    private ZButton _myButton;
    private Tween _tweener;

    [HideInInspector] public int ButtonIndex;

    public void SetDefault()
	{
        Object_Success.SetActive(false);
        Object_Fail.SetActive(false);
        SetActiveButtonInteraction(false);
    }

    #region ####### 연출관련 #######
    /// <summary>
    /// 버튼 성공,실패 상태 연출
    /// </summary>
    /// <param name="result"></param>
    public void SetButtonResult(bool result)
    {
        var invokeObject = result == true ? Object_Success : Object_Fail;
        invokeObject.SetActive(true);

        _tweener.Kill();
        ImageGage.fillAmount = 0;
        Effect_On.Stop();

        if(true == result)
		{
            Effect_Click.gameObject.SetActive(true);
            Effect_Click.Clear();
            Effect_Click.Play(true);
        }

        Invoke(nameof(SetDefault), 2f);
    }

    public void PlayActiveButton()
	{
        // 터치 활성화
        SetActiveButtonInteraction(true);

        // 이펙트 활성화
        Effect_On.gameObject.SetActive(true);
        Effect_On.Clear();
        Effect_On.Play(true);
        ImageGage.fillAmount = 1;

        // 게이지 트윈 시작 ( 1 ~ 0 )
        float beforeValue = 1;
        _tweener = DOTween.To(() => beforeValue, x => beforeValue = x, 0, OwnerHud.WaitTime).OnUpdate(() =>
        {
            ImageGage.fillAmount = beforeValue;
        })
        .OnComplete(() =>
        {
            _tweener.Kill();
        });
    }

    /// <summary>
    /// 버튼 활성 , 비활성
    /// </summary>
    /// <param name="active"></param>
    public void SetActiveButtonInteraction(bool active)
	{
        if (null == _myButton)
            _myButton = GetComponent<ZButton>();

        _myButton.interactable = active;
    }
	#endregion
}
