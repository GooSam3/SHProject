using System;

public class CharacterSelectState : IcarusStateBase
{
	public override void OnEnter(Action callback, params object[] args)
	{
		base.OnEnter(callback, args);

		ZSceneManager.Instance.OpenMain("Creation", (float _progress) =>
		{
			ZLog.Log(ZLogChannel.Default, $"Scene loading progress: {_progress}");
		},
		(string _addressableName) =>
		{
			ZGameManager.Instance.ChangeQualityTemporary(E_Quality.Creation);
			UIManager.Instance.Open<UIFrameCharacterSelect>(delegate
			{
				UIManager.Instance.Close<UIFrameLoadingScreen>();
			});
			ZGameManager.Instance.SetupSceneGraphics();			
		});
	}

	public override void OnExit(Action callback)
	{
		ZGameManager.Instance.RestoreQualityLevel();

		UIManager.Instance.Close<UIFrameCharacterSelect>(true);
		base.OnExit(callback);
	}
}