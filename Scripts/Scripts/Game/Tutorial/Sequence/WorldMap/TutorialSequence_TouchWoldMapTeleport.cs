/// <summary> 텔레포트 터치 </summary>
public class TutorialSequence_TouchWoldMapTeleport : TutorialSequence_FocusButton<UIFrameWorldMap>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return "Map_Panel/PopupRight/LocalMap_Field_Info_Popup-------new001/Body/Scroll_BtList/Bt_Teleport"; } }

	protected override bool Check()
	{
		return true;
	}
}