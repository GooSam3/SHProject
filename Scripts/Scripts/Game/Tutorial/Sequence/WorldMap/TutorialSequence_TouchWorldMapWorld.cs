/// <summary> minimap touch </summary>
public class TutorialSequence_TouchWorldMapWorld : TutorialSequence_FocusButton<UIFrameWorldMap>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Map_Panel/PopupLeft/WorldMapPanelControl-------new/ButtonGroup/WorldMap/Toggle_On"; } }

	protected override bool Check()
	{
		return true;
	}
}