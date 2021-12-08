public class UINameTagMonster : UINameTagBase
{
	private uint mMonsterTID = 0;

	//------------------------------------------------
	protected override void OnNameTagInitialize(ZPawn _followPawn)
	{
		base.OnNameTagInitialize(_followPawn);		
	}

	protected override void OnUIWidgetFocus(bool _on)
	{
		base.OnUIWidgetFocus(_on);
		SetNameTagShowHide(_on);
	}

	protected override void OnNameTagRefreshTarget() 
	{
		base.OnNameTagRefreshTarget();
		RefreshQuestTarget();
	}

	protected override void OnNameTagRemove()
	{
		base.OnNameTagRemove();		
	}
	//----------------------------------------------------------------
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
