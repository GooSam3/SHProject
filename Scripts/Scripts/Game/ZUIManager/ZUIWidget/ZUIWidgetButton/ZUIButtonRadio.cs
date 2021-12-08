using UnityEngine;

public class ZUIButtonRadio : CUGUIButtonRadioBase
{
	[SerializeField]
	private ZCommandUIRadio RadioCommand = new ZCommandUIRadio();


	[SerializeField] Color color = Color.white;
	[SerializeField] UnityEngine.UI.Text text;

	//---------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		SetRadioGroup((int)RadioCommand.pUIButtonGroup);
		base.OnUIWidgetInitialize(_UIFrameParent);		
	}
	
	protected override CCommandUIWidgetBase OnUIWidgetCommandExtract()
	{	
		return RadioCommand;
	}

	public void SetUIButtonArgument(int _Argument)
	{
		RadioCommand.SetCommandArgument(_Argument);
	}

	public void SetUIButtoonRadioGroup(int _group)
	{
		RadioCommand.SetCommandGroup(_group);
	}
	
}
