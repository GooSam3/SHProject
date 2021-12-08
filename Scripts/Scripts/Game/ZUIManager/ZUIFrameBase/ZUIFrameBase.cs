using UnityEngine;
using System.Collections.Generic;
abstract public class ZUIFrameBase : CUIFrameWidgetBase
{
    [SerializeField]
    private E_UILocalizing Language = E_UILocalizing.Korean;
    [SerializeField]
    private E_UIThemaType  Thema = E_UIThemaType.Default;
    //--------------------------------------------------------------
    protected sealed override SUIFrameInfo Infomation() 
    {
        SUIFrameInfo uiFrameInfo = new SUIFrameInfo();
        uiFrameInfo.ID = GetType().ToString();
        uiFrameInfo.LocalizingKey = (int)Language;
        uiFrameInfo.ThemaType = (int)Thema;       
        return uiFrameInfo;
    }

	protected override void OnInitialize()
	{
		base.OnInitialize();
      
    }

	public void Close(bool _remove = false)
	{
        UIManager.Instance.Close(ID, _remove);
	}
    //----------------------------------------------------------------------------------
  

    //----------------------------------------------------------------------------------
    protected override sealed void OnCommand(int _CommandID, int _Group, int _Argument, CUGUIWidgetBase _CommandOwner)
    {
        OnCommandContents((ZCommandUIButton.E_UIButtonCommand)_CommandID, (ZCommandUIButton.E_UIButtonGroup)_Group, _Argument, _CommandOwner);
	}
    //--------------------------------------------------------------------------------
    protected virtual void OnCommandContents(ZCommandUIButton.E_UIButtonCommand _commandID, ZCommandUIButton.E_UIButtonGroup _groupID, int _arguement, CUGUIWidgetBase _commandOwner) { }
}
