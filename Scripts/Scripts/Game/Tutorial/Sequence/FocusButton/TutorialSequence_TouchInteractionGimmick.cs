using System.Collections.Generic;
using UnityEngine.UI;

/// <summary> 기믹 인터렉션 영역 터치 </summary>
public class TutorialSequence_TouchInteractionGimmick : TutorialSequence_MoveTo
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected string InteractionButtonPath { get { return "Bt_Interaction"; } }

	protected override bool Check()
	{
		return true;
	}

	protected override void StartGuide()
	{
		if(null == DestPosition)
		{
			PostArrive();
		}
		else
		{
			SetBlockScereen(true, true);
			base.StartGuide();
		}				
	}

	protected Selectable GetInteractionSelectable(UISubHUDTemple templeUI)
	{ 
		return templeUI?.transform.Find(InteractionButtonPath)?.GetComponent<Selectable>() ?? null;
	}

	protected override void PostArrive()
	{
		CancelInvoke(nameof(PostArrive));
		SetBlockScereen(true, true);

		var templeUI = UIManager.Instance.Find<UISubHUDTemple>();

		if (null == templeUI)
		{
			if (true == CheckStartGuideInvoke())
			{
				Invoke(nameof(PostArrive), 0.1f);
			}
			return;
		}	

		mSelectable = GetInteractionSelectable(templeUI);

		//걍 띄우자
		if (null == mSelectable || false == mSelectable.gameObject.activeInHierarchy)
		{
			templeUI.SetInteractionGimmick(true, () =>
			{
				templeUI.SetInteractionGimmick(false);
			});

			mSelectable = GetInteractionSelectable(templeUI);
		}

		if (null == mSelectable || false == mSelectable.gameObject.activeInHierarchy)
		{
			Invoke(nameof(PostArrive), 0.1f);
			return;
		}


		//버튼 가이드 시작
		ShowGuide(mSelectable.gameObject, InteractionButtonAction);
	}

	private void InteractionButtonAction()
	{
		if (mSelectable is Button button)
		{
			button.onClick?.Invoke();
		}
		else if (mSelectable is Toggle toggle)
		{
			toggle.isOn = true;
		}

		EndSequence(false);
	}

	protected override void SetParams(List<string> args)
	{
		if(0 < args.Count && ZGimmickManager.Instance.TryGetValue(args[0], out var gimmcks))
		{
			DestPosition = gimmcks[0].Position;
			//목적지 셋팅
			//DestPosition = new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
		}
		else
		{
			DestPosition = null;
		}
	}

	/// <summary> 도착 체크 </summary>
	protected override bool CheckArrive()
	{
		var templeUI = UIManager.Instance.Find<UISubHUDTemple>();

		if (null != templeUI && (GetInteractionSelectable(templeUI)?.gameObject.activeInHierarchy ?? false))
			return true;
			
		return base.CheckArrive();
	}
}