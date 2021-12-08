using UnityEngine;

abstract public class CUIFrameScreenBlockBase : CUIFrameBase
{
    [SerializeField]
    private GameObject Indicator;

    //-------------------------------------------------
    protected override void OnInitialize()
    {
        base.OnInitialize();
        Indicator.SetActive(false);
    }

    //---------------------------------------------------
    public void DoScreenBlockShowIndicator(bool _Show)
    {
        if (Indicator != null)
        {
            Indicator.SetActive(_Show);
        }
        OnUIFrameScreenBlockShowIndicator(_Show);
    }
    
    //--------------------------------------------------
    protected virtual void OnUIFrameScreenBlockShowIndicator(bool _Show) { }
    
}
