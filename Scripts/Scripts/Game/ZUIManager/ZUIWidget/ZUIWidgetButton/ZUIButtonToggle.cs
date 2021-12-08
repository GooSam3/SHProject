using UnityEngine;

public class ZUIButtonToggle : CUGUIButtonToggleBase
{
	[SerializeField]
	private ZCommandUIToggle ToggleCommand = new ZCommandUIToggle();

	protected override CCommandUIWidgetBase OnUIWidgetCommandExtract()
	{
		return ToggleCommand;
	}

	public void SetUIButtonArgument(int _Argument)
	{
		ToggleCommand.SetCommandArgument(_Argument);
		if (TogglePair)
		{
			ZUIButtonToggle OtherToggle = TogglePair as ZUIButtonToggle;
			OtherToggle.SetUIButtonArgument(_Argument);
		}
	}
}
