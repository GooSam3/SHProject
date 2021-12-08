using UnityEngine;

public class UIWorldScrollAllLocalMapItem : CUGUIWidgetSlotItemBase
{
	[SerializeField] ZImage ImageLock = null;
	[SerializeField] ZImage ImageArrow = null;
	[SerializeField] ZText  TextStage = null;

	private uint mStageTID = 0;
	private UIFrameWorldMap mUIFrameWorldMap = null;
	//----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameWorldMap = _UIFrameParent as UIFrameWorldMap;
	}
	//----------------------------------------------------------------
	public void DoAllLocalMapItem(UIWorldScrollAllLocalMap.SLocalStageInfo _stageInfo)
	{
		mStageTID = _stageInfo.StageTID;
	
		if (_stageInfo.CanEnter)
		{
			ImageLock.gameObject.SetActive(false);
		}
		else
		{
			ImageLock.gameObject.SetActive(true);
		}

		if (_stageInfo.Selected)
		{
			ImageArrow.gameObject.SetActive(true);
		}
		else
		{
			ImageArrow.gameObject.SetActive(false);
		}

		TextStage.text = _stageInfo.LocalStageName;
	}

	//-----------------------------------------------------------------
	public void HandleWorldStageSelect()
	{
		mUIFrameWorldMap.DoUILocalMapOpen(mStageTID);
	}
} 
