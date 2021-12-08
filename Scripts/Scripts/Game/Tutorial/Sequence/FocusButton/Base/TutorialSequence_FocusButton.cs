using UnityEngine.UI;
using UnityEngine;

/// <summary> 포커스 버튼 기반! </summary>
public abstract class TutorialSequence_FocusButton<UI_FRAME_TYPE> : TutorialSequence_UIFrameBase<UI_FRAME_TYPE> where UI_FRAME_TYPE : CUIFrameBase
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected abstract string Path { get; }

	/// <summary> 해당 튜토리얼에서 강조할 그룹 </summary>
	protected virtual string HighlightPath { get { return Path; } }

	protected Selectable mSelectable { get; set; }

	/// <summary> 해당 버튼 가지고 오기 </summary>
	protected virtual Selectable GetSelectable()
	{
		return OwnerUI.transform.Find(Path)?.GetComponent<Selectable>() ?? null;
	}

	/// <summary> 해당 튜토리얼에서 강조할 그룹 </summary>
	protected override Transform GetHighlightObject()
	{
		return OwnerUI.transform.Find(HighlightPath);
	}

	protected override void StartGuide()
	{
		CancelInvoke(nameof(StartGuide));

		if (false == CheckStartGuideInvoke())
			return;

		mSelectable = GetSelectable();

		if (null == mSelectable || false == mSelectable.gameObject.activeInHierarchy)
		{
			Invoke(nameof(StartGuide), 0.1f);
			return;
		}

		//버튼 가이드 시작
		ShowGuide(mSelectable.gameObject, this.ButtonAction);
	}

	protected virtual void ButtonAction()
	{
		if(Check())
		{
			if(mSelectable is Button button)
			{
				button.onClick?.Invoke();
			}
			else if(mSelectable is Toggle toggle)
			{
				toggle.isOn = true;
			}
			
			EndSequence(false);
		}
		else
		{
			EndSequence(true);
		}		
	}

	/// <summary> 버튼 액션을 수행할 수 있는지 체크. 실패한다면 튜토리얼 스킵! </summary>
	protected abstract bool Check();
}