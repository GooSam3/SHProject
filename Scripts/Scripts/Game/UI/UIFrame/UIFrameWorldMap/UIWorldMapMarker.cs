using UnityEngine;

public class UIWorldMapMarker : CUGUIWidgetBase
{
    [SerializeField] ZImage CheckLock   = null;
    [SerializeField] ZImage CheckSelect = null;
    [SerializeField] ZText  StageName    = null;

	private UIFrameWorldMap		mUIWorldMap = null;
	private ZUIButtonCommand		mButtonCommand = null;
	private uint					mStageID = 0;
	//------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIWorldMap = _UIFrameParent as UIFrameWorldMap;
		mButtonCommand = GetComponent<ZUIButtonCommand>();
		CheckLock.gameObject.SetActive(false);
		CheckSelect.gameObject.SetActive(false);
	}

	//---------------------------------------------------------
	public void DoMapMarkerRefresh(bool _select, bool _canEnter, uint _stageID, string _stageName, int _index)
	{
		mStageID = _stageID;
		StageName.text = $"{_index}.{_stageName}" ;

		if (_canEnter)
		{
			CheckLock.gameObject.SetActive(false);
			StageName.color = Color.white;
			if (_select)
			{
				CheckSelect.gameObject.SetActive(true);
			}
			else
			{
				CheckSelect.gameObject.SetActive(false);
			}
		}
		else
		{
			CheckLock.gameObject.SetActive(true);
			CheckSelect.gameObject.SetActive(false);
			StageName.color = new Color(0.67f, 0.68f, 0.72f);
		}
	}

	//----------------------------------------------------------
	public void HandleWorldMapMarker()
	{
		mUIWorldMap.DoUILocalMapOpen(mStageID);
	}
}
