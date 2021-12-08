using System.Collections;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
using UnityEngine.Events;
using System.Threading.Tasks;

public class COAuth2GooglePlay : COAuth2Base
{
	private FirebaseAuth m_pFireBaseAuth = null;
	//----------------------------------------------------------------------------
	protected override void OnOAuthInitialize()
	{
#if (UNITY_ANDROID && !UNITY_EDITOR)
		PlayGamesPlatform.InitializeInstance(new PlayGamesClientConfiguration.Builder().RequestIdToken().Build());	
		PlayGamesPlatform.DebugLogEnabled = true;
		PlayGamesPlatform.Activate();
		m_pFireBaseAuth = FirebaseAuth.DefaultInstance;
		Debug.Log("[Login] Initialize FireBase");
#endif
	}

	protected override void OnOAuthLoginStart(UnityAction<bool, string, string> delFinish)
	{
		PrivAuthGooglePlayStart(delFinish);
	}

	protected override void OnOAuthLogOut()
	{
		PlayGamesPlatform.Instance.SignOut();
		m_pFireBaseAuth.SignOut();
	}


	//------------------------------------------------------------------
	private void PrivAuthGooglePlayStart(UnityAction<bool, string, string> delFinish)
	{
		if (!Social.localUser.authenticated) 
		{
			Social.localUser.Authenticate(bSuccess => 
			{
				if (bSuccess) 
				{
					Debug.Log(string.Format("[Login] Google Login Sucess"));
					PrivFireBaseLogin(((PlayGamesLocalUser)Social.localUser).GetIdToken(), delFinish);
				}
				else 
				{
					Debug.Log("[Login] Google Login Fail");
					delFinish?.Invoke(false, "Google Login Fail", "");
				}
			});
		}
	}

	private void PrivFireBaseLogin(string strToken, UnityAction<bool, string, string> delFinish)
	{
		Credential credential = GoogleAuthProvider.GetCredential(strToken, null);
		Firebase.Extensions.TaskExtension.ContinueWithOnMainThread(m_pFireBaseAuth.SignInWithCredentialAsync(credential), task =>
		{
			if (task.IsCanceled)
			{
				Debug.LogError("[Login] FireBase Login Cancle");
				delFinish?.Invoke(false, "FireBase Login Cancle", "");
				return;
			}
			if (task.IsFaulted)
			{
				Debug.LogError("[Login] FireBase Login Error");
				Debug.Log(task.Exception);
				delFinish?.Invoke(false, "FireBase Login Error", "");
				return;
			}

			Debug.Log("[Login] FireBase Login Sucess");
			delFinish?.Invoke(true, task.Result.UserId, strToken);
		});
	}
}
