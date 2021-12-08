using System;
using UnityEngine;

/// <summary>
/// 게임 시작전 필요한 처리를 위한 상태 (ex : 로고나오기전에 해야할 일들)
/// </summary>
public class StartState : IcarusStateBase
{
	public override void OnInitialize(ZGameManager _parent)
	{
		base.OnInitialize(_parent);
	}

	public override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);

		Parent.FSM.DrawOnGui = false;

		PrepareManagers();

		GoNextState();
	}

	/// <summary>
	/// 미리 준비되어도될 Manager 객체 준비
	/// </summary>
	private void PrepareManagers()
	{
		ZLog.BeginProfile("PrepareManagers");

		NTIcarusManager.Instance.InitializeSDK();

		// Addressable Object
		AddressableManagerLoader.Instance.Initialize();
		// Patch UI
		UIManagerRoot.Instance.Initialize();

		// Audio 		
		AudioManager.Instance.Initialize();

		// Resource Set
		ResourceSetManager.Instance.Initialize();

		CameraManager.Instance.DoSetVisible(true);

		PrepareResourceUI();

		ZGameOption.Instance.EmptyFunction();

		ZLog.EndProfile("PrepareManagers");
	}

	private void PrepareResourceUI()
	{
		GameObject UIFrameObjectOrigin = Resources.Load("UI/Prefab/UIFrame/UIFrameLogo") as GameObject;		
		CUIFrameBase CloneUIFrame = Instantiate(UIFrameObjectOrigin).GetComponent<CUIFrameBase>();
		UIManager.Instance.ImportFrame(CloneUIFrame);

		UIFrameObjectOrigin = Resources.Load("UI/Prefab/UIFrame/UIFramePatcher") as GameObject;
		CloneUIFrame = Instantiate(UIFrameObjectOrigin).GetComponent<CUIFrameBase>();		
		UIManager.Instance.ImportFrame(CloneUIFrame);
	}

	public override void OnExit(Action callback)
	{
		base.OnExit(callback);
	}

	public override void Dev_OnGUI()
	{
		base.Dev_OnGUI();
	}
}
