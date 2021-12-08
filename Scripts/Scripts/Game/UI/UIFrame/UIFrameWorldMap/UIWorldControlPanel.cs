using UnityEngine;

public class UIWorldControlPanel : CUGUIWidgetBase
{
	public enum E_ButtonType
	{
		None,
		Local,
		World,
	}

	[SerializeField] ZButton ButtonWorld = null;
	[SerializeField] ZButton ButtonLocal = null;
	[SerializeField] ZUIButtonToggle ButtonFavorite = null;
	[SerializeField] ZUIButtonToggle ButtonPortalAll = null;
	private UIFrameWorldMap mUIFrameWorldMap = null;
	private E_ButtonType mControlMode = E_ButtonType.World;
	//--------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
	{
		base.OnUIWidgetInitialize(_UIFrameParent);
		mUIFrameWorldMap = _UIFrameParent as UIFrameWorldMap;
	}

	//---------------------------------------------------
	public void DoControllWorldMode(E_ButtonType type)
	{
		mControlMode = type;
		ButtonWorld.interactable = type != E_ButtonType.World;
		ButtonFavorite.DoToggleAction(false);
		ButtonPortalAll.DoToggleAction(false);
	}

	public E_ButtonType GetControlWorldMode()
	{
		return mControlMode;
	}

	public void SetLocalButtonOn()
	{
		ButtonLocal.Select();
		ButtonFavorite.DoToggleAction(false);
		ButtonPortalAll.DoToggleAction(false);
	}

	//------------------------------------------------------
	public void HandleWorldMapOpen()
	{
		mUIFrameWorldMap.DoUIWorldMapOpen();
	}

	public void HandleLocalMapOpen()
	{
		mUIFrameWorldMap.DoUILocalMapOpen();
	}

	public void HandleFavoriteOpen()
	{
		ButtonPortalAll.DoToggleAction(false);
		ButtonFavorite.DoToggleAction(true);
		ButtonWorld.interactable = true;
		ButtonLocal.interactable = true;
	}

	public void HandlePortalList()
	{		
		ButtonFavorite.DoToggleAction(false);
		ButtonPortalAll.DoToggleAction(true);
		ButtonWorld.interactable = true;
		ButtonLocal.interactable = true;
	}
}
