using UnityEngine;

public class ZUIFrameDynamicLoad : CUIFrameDynamicBase
{
    [SerializeField]
    private E_UILocalizing Language = E_UILocalizing.Korean;
    [SerializeField]
    protected E_UIFrameType FrameType = E_UIFrameType.None;
    //--------------------------------------------------------------
    protected sealed override SUIFrameInfo Infomation()
    {
        SUIFrameInfo uiFrameInfo = new SUIFrameInfo();
        uiFrameInfo.ID = FrameType.ToString();
        uiFrameInfo.LocalizingKey = (int)Language;
        uiFrameInfo.ThemaType = 0;
        return uiFrameInfo;
    }
}
