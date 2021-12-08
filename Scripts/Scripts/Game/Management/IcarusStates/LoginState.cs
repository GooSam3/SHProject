using System;

public class LoginState : IcarusStateBase
{
	public override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);
		ZNet.Data.Global.ClearAllUsers();
		UIManager.Instance.Open<UIFrameLogin>(delegate
		{
		
		});
	}

	public override void OnExit(Action callback)
	{
		UIManager.Instance.Close<UIFrameLogin>(true);
		base.OnExit(callback);		
	}
}