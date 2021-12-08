using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public abstract class CManagerSceneLoaderBase<TEMPLATE> : CManagerAddressableBase<TEMPLATE, CAddressableProviderScene> where TEMPLATE : CManagerSceneLoaderBase<TEMPLATE>
{
    protected class SLoadedSceneInfo
	{
		public string				AddressableName;
		public SceneInstance		LoadedSceneInstance;
		public UnityAction<string> EventFinishLoad = null;
		public UnityAction<string> EventFinishUnload = null;
	}

	/// <summary>
	/// NonAddressable asset으로 그냥 앱빌드에 포함된 씬으로 사용
	/// </summary>
	[SerializeField]
	private string LoadingScene = "LoadingScene";
	[SerializeField]
	private float MinLoadingTime = 1f;

	private string mMainSceneName;		public string pMainSceneName { get { return mMainSceneName; } }
	private SceneInstance mMainScene =	new SceneInstance();
	private float mMainSceneStartTime = 0;
	private Dictionary<string, SLoadedSceneInfo>		m_dicSceneInstance = new Dictionary<string, SLoadedSceneInfo>();
	private Queue<SLoadedSceneInfo>					m_queUnloadScene = new Queue<SLoadedSceneInfo>();
	//------------------------------------------------------------------------
	protected override void OnUnityStart()
	{
		base.OnUnityStart();
		StartCoroutine(CoroutineUnloadScene());	
	}
	
	//-----------------------------------------------------------------------
	private IEnumerator CoroutineUnloadScene()
	{
		while(true)
		{
			if (m_queUnloadScene.Count > 0)
			{
				SLoadedSceneInfo LoadedScene = m_queUnloadScene.Dequeue();
				AsyncOperationHandle<SceneInstance> AsyncHandle = Addressables.UnloadSceneAsync(LoadedScene.LoadedSceneInstance);
				AsyncHandle.Completed += (AsyncOperationHandle<SceneInstance> _Result) =>
				{
					
					OnSceneAdditiveUnload(LoadedScene.AddressableName, _Result.Result.Scene);
					LoadedScene.EventFinishUnload?.Invoke(LoadedScene.AddressableName);
				};
				yield return AsyncHandle;
			}
			else
			{
				yield return null;
			}
		}
	}

	private IEnumerator CoroutineLoadMainScene(string _addressableName)
	{
		// 로딩 씬 (내용 없음)을 로드하면서 기존 씬을 전부 메모리 해제한다.
		SceneManager.LoadScene(LoadingScene, LoadSceneMode.Single);
		AsyncOperationHandle<SceneInstance> AsyncHandle = Addressables.LoadSceneAsync(_addressableName, LoadSceneMode.Single, true, 100);	
		while(true)
		{
			if (AsyncHandle.IsValid() == false)
			{
				OnAddressableError(_addressableName, "");
				yield break;
			}
			else if (AsyncHandle.IsDone)
			{
				mMainScene = AsyncHandle.Result;

				float fConsumetime = mMainSceneStartTime - Time.time;
				if (fConsumetime < MinLoadingTime)
				{
					OnSceneProgress(_addressableName, 1f);
					yield return new WaitForSeconds(MinLoadingTime - fConsumetime);
				}
				OnSceneFinishMain(_addressableName, mMainScene.Scene);
				yield break;
			}
			else
			{
				OnSceneProgress(_addressableName, AsyncHandle.PercentComplete);
				yield return null;
			}
		}
	}

	protected sealed override void OnAddressableLoadScene(string _addressableName, ref SceneInstance _SceneInstance)
	{
		if (m_dicSceneInstance.ContainsKey(_addressableName))
		{
			SLoadedSceneInfo LoadScene = m_dicSceneInstance[_addressableName];
			LoadScene.LoadedSceneInstance = _SceneInstance;
			OnSceneAdditiveLoad(_addressableName, _SceneInstance.Scene);
			LoadScene.EventFinishLoad?.Invoke(_addressableName);
		}
	}
		//-----------------------------------------------------------------------
	protected void LoadMainScene(string _addressableName, bool bForceDelete) // DontDestroy까지 모두 삭제해버린다. 완전 클리어 조심.
	{
		mMainSceneName = _addressableName;
		mMainSceneStartTime = Time.time;
		PrivSceneClearAll(bForceDelete);
		StartCoroutine(CoroutineLoadMainScene(_addressableName));
	}

	protected void LoadAdditiveScene(string _addressableName, UnityAction<string> _eventFinish)
	{
		if (m_dicSceneInstance.ContainsKey(_addressableName))
		{
			_eventFinish?.Invoke(_addressableName);
			return;
		}

		SLoadedSceneInfo LoadedScene = new SLoadedSceneInfo();
		LoadedScene.AddressableName = _addressableName;
		LoadedScene.EventFinishLoad = _eventFinish;

		m_dicSceneInstance[_addressableName] = LoadedScene;
		RequestLoad(_addressableName, null, HandleSceneProgress);
	}

	internal void ImportLoadAdditiveScene(string _addressableName, UnityAction<string> _eventFinish)
	{
		LoadAdditiveScene(_addressableName, _eventFinish);
	}

	protected void UnloadAdditiveScene(string _addressableName, UnityAction<string> _eventFinish)
	{
		if (m_dicSceneInstance.ContainsKey(_addressableName))
		{
			SLoadedSceneInfo SceneInfo = m_dicSceneInstance[_addressableName];
			SceneInfo.EventFinishUnload = _eventFinish;
			m_queUnloadScene.Enqueue(SceneInfo);
			m_dicSceneInstance.Remove(_addressableName);
		}
	}

	protected void ExtractAdditiveScene(HashSet<string> _sceneList)
	{
		Dictionary<string, SLoadedSceneInfo>.Enumerator it = m_dicSceneInstance.GetEnumerator();
		while (it.MoveNext())
		{
			_sceneList.Add(it.Current.Key);
		}
	}

	//-------------------------------------------------------------------------
	private void PrivSceneClearAll(bool bForceDelete)
	{
		m_dicSceneInstance.Clear();
		m_queUnloadScene.Clear();

		if (bForceDelete)
		{
			GameObject [] aGameObject = mMainScene.Scene.GetRootGameObjects();
			for (int i = 0; i < aGameObject.Length; i++)
			{
				Destroy(aGameObject[i]);
			}
		}
	}

	//------------------------------------------------------------------------
	private void HandleSceneProgress(string _AddressableName, float _Progress)
	{
		OnSceneProgress(_AddressableName, _Progress);
	}

	//-----------------------------------------------------------------------
	protected virtual void OnSceneFinishMain(string _addressableName, Scene _mainScene) { }
	protected virtual void OnSceneAdditiveLoad(string _addressableName, Scene _mainScene) { }
	protected virtual void OnSceneAdditiveUnload(string _addressableName, Scene _mainScene) { }
	protected virtual void OnSceneProgress(string _addressableName, float _progress) { }
}
