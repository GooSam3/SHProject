using UnityEngine;

[System.Serializable]
public class ZCommandUIButton : CCommandUIWidgetBase
{
    public enum E_UIButtonCommand
    {
        None,
        OK,
        Cancel,
        Open,
        Close,          
        Select,
        UnSelect,
        Move,
        Stop,

        ToggleOn,
        ToggleOff,

        RadioOn,
        RadioOff,
    }

    public enum E_UIButtonGroup
	{
        Main,
        Group_1,
        Group_2,
        Group_3,
        Group_4,
        Group_5,
        Group_6,
        Group_7,
        Group_8,
        Group_9,
        Group_10,
    }

    [SerializeField]
    private E_UIButtonCommand Command = E_UIButtonCommand.None;
    [SerializeField]
    private E_UIButtonGroup   Group = E_UIButtonGroup.Main;   public int pUIButtonGroup { get { return GetCommandGroup(); } }
    [SerializeField]
    private     int Argument = 0;
    [SerializeField]
    private     int CommandGroup = 0;   // 0 이 아닌 값을 입력하면 Group 변수 대신 사용된다. 10개가 넘어가는 대형 그룹에 사용 
 
	//---------------------------------------------------------------------
	protected override void OnCommandUIWidget(CUIFrameBase _OnwerFrame, CUGUIWidgetBase _CommandOwner) 
    {    
        if (Command == E_UIButtonCommand.Close)
		{
            if (Group == E_UIButtonGroup.Main)
            {
                UIManager.Instance.Close(_OnwerFrame.ID);
                return;
            }
        }
        _OnwerFrame.ImportCommand((int)Command, (int)Group, Argument, _CommandOwner);
    }

    //----------------------------------------------------------------------
    public void SetCommandArgument(int _Argument)
	{
        Argument = _Argument;
	}

    public void SetCommandGroup(int _commandGroup)
	{
        CommandGroup = _commandGroup;
    }

    public int GetCommandArgument()
	{
        return Argument;
	}

    public int GetCommandGroup()
	{
        int radioGroup = (int)Group;

        if (CommandGroup != 0)
		{
            radioGroup = CommandGroup;
		}

        return radioGroup;
	}
}
