using UnityEngine;
using UnityEngine.UI;
/// <summary> UI_FRAME_TYPE 에 해당하는 ui가 오픈되었을 경우 처리 시작 </summary>
public abstract class TutorialSequence_UIFrameBase<UI_FRAME_TYPE> : TutorialSequence_None where UI_FRAME_TYPE : CUIFrameBase
{
	/// <summary> 찾을 UI </summary>
	protected override string FindUIName { get { return typeof(UI_FRAME_TYPE).Name; } }
	/// <summary> ui </summary>
	protected UI_FRAME_TYPE OwnerUI { get { return UIFrame as UI_FRAME_TYPE; } }
}