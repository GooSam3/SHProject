public class UINameTagFieldObject : UINameTagBase
{
	protected override void OnNameTagInitialize(ZPawn _followPawn)
	{
		base.OnNameTagInitialize(_followPawn);
		SetNameTagShowHide(false);
	}

	protected override void OnNameTagRefreshTarget()
	{
		base.OnNameTagRefreshTarget();
		RefreshQuestTarget();
	}

	protected override void OnUIWidgetFocus(bool _on)
	{
		base.OnUIWidgetFocus(_on);
		SetNameTagShowHide(_on);
	}

	//-----------------------------------------------------------
	private void RefreshQuestTarget()
	{
		UIFrameQuest questUI = UIManager.Instance.Find<UIFrameQuest>();
		if (questUI.CheckQuestMonster(mFollowPawn.TableId))
		{
			EffectNameTagAttach(500001);
		}
		else
		{
			EffectNameTagDetach();
		}
	}
}
