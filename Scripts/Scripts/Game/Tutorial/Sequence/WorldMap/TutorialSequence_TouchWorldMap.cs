/// <summary> minimap touch </summary>
public class TutorialSequence_TouchWorldMap : TutorialSequence_FocusButton<UISubHUDMiniMap>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "HUD_MiniMap_Sub/Body/Btn_WorldMap"; } }

	protected override bool Check()
	{
		return true;
	}
}