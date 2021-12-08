using UnityEngine;
/// <summary> path에 해당하는 게임오브젝트 포커스. </summary>
public abstract class TutorialSequence_Focus<UI_FRAME_TYPE> : TutorialSequence_UIFrameBase<UI_FRAME_TYPE> where UI_FRAME_TYPE : CUIFrameBase
{
	/// <summary> 해당 ui 경로 </summary>
	protected abstract string Path { get; }

	/// <summary> 해당 튜토리얼에서 강조할 그룹 </summary>
	protected virtual string HightlightPath { get { return Path; } }

	/// <summary> 해당 경로 가지고 오기 </summary>
	protected GameObject GetGameObject()
	{
		return OwnerUI.transform.Find(Path).gameObject;
	}
	/// <summary> 해당 튜토리얼에서 강조할 그룹 </summary>
	protected override Transform GetHighlightObject()
	{
		return OwnerUI.transform.Find(HightlightPath);
	}

	protected override void StartGuide()
	{
		if (false == CheckStartGuideInvoke())
			return;

		CancelInvoke(nameof(StartGuide));
		var go = GetGameObject();

		if (null == go)
		{
			Invoke(nameof(StartGuide), 0.1f);
			return;
		}

		//버튼 가이드 시작
		ShowGuide(go, () =>
		{
			// TODO :: 사운드 출력할까

			EndSequence(false);
		});
	}
}