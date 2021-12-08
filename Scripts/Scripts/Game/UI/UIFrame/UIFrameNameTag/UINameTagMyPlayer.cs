public class UINameTagMyPlayer : UINameTagPartyPlayer
{
	protected override void OnNameTagInitialize(ZPawn _followPawn)
	{
		base.OnNameTagInitialize(_followPawn);
		VisibleBar(false);
	}

	protected override void OnUIWidgetFocus(bool _on)
	{
		base.OnUIWidgetFocus(_on);

		// 내 pc 는 생성시 항상 켜준다
		SetNameTagShowHide(true);
	}
}
