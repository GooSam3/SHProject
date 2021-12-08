using UnityEngine;

public class ZUIButtonCommand : CUGUIButtonBase
{
    [SerializeField] ZCommandUIButton ButtonCommand = new ZCommandUIButton();
    //--------------------------------------------------------------------
    protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitialize(_UIFrameParent);
    }

	protected override CCommandUIWidgetBase OnUIWidgetCommandExtract()
	{        
        return ButtonCommand;
    }

    //------------------------------------------------------------------

    public void SetUIButtonArgument(int _Argument)
	{
        ButtonCommand.SetCommandArgument(_Argument);
	}

    public int GetUIButtonArgument()
	{
        return ButtonCommand.GetCommandArgument(); 
	}

    
}
