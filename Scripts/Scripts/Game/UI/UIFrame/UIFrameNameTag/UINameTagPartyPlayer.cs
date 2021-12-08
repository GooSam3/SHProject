using UnityEngine;

public class UINameTagPartyPlayer : UINameTagPlayer
{
	[SerializeField] ZSlider HPBar = null;
	[SerializeField] ZSlider MPBar = null;
	//--------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		VisibleBar(true);
	}

	protected override void OnNameTagInitialize(ZPawn _followPawn)
	{
		base.OnNameTagInitialize(_followPawn);

		if (mFollowPawn)
		{
			mFollowPawn.DoAddEventHpUpdated(HandleNameTagHPUpdate);
			mFollowPawn.DoAddEventMpUpdated(HandleNameTagMPUpdate);
		}
	}

	protected override void OnNameTagRemove()
	{
		base.OnNameTagRemove();
	
		if (mFollowPawn)
		{
			mFollowPawn.DoRemoveEventHpUpdated(HandleNameTagHPUpdate);
			mFollowPawn.DoRemoveEventMpUpdated(HandleNameTagMPUpdate);
		}
	}

	//-------------------------------------------------------

	protected void VisibleBar(bool _visible)
	{
		if (HPBar != null)
		{
			HPBar.gameObject.SetActive(_visible);
		}

		if (MPBar != null)
		{
			MPBar.gameObject.SetActive(_visible);
		}
	}

	private void HandleNameTagHPUpdate(float _currentHP, float _maxHP)
	{
		float hpRate = _currentHP / _maxHP;
		if (HPBar != null)
		{
			HPBar.value = hpRate;
		}
	}

	private void HandleNameTagMPUpdate(float _currentMP, float _maxMP)
	{
		float mpRate = _currentMP / _maxMP;
		if (MPBar != null)
		{
			MPBar.value = mpRate;
		}
	}

}
