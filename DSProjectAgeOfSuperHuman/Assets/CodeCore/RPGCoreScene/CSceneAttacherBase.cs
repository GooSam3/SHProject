using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.Events;

public abstract class CSceneAttacherBase : CMonoBase
{
	//-----------------------------------------------------------
	protected void ProtSceneAttacherLoadResourcePrefab(string strPrefabPath, string strPrefabName, UnityAction<bool> delFinish)
	{
		GameObject Prefab = GameObject.Find(strPrefabName);
		if (Prefab == null)
		{
			PrivSceneAttacherLoadResourcePrefab($"{strPrefabPath}/{strPrefabName}", delFinish);
		}
		else
		{
			delFinish?.Invoke(false);
		}
	}

	protected void ProtSceneAttacherLoadAddressablePrefab(string strPrefabName, UnityAction<bool> delFinish)
	{
		GameObject Prefab = GameObject.Find(strPrefabName);
		if (Prefab == null)
		{
			PrivSceneAttacherLoadAddressablePrefab(strPrefabName, delFinish);
		}
		else
		{
			delFinish?.Invoke(false);
		}		
	}

	protected void ProtSceneAttacherLoadUIScene(string strUISceneName, UnityAction<bool> delFinish)
	{
		CManagerUIFrameBase UIMananger = FindObjectOfType<CManagerUIFrameBase>();
		if (UIMananger == null)
		{
			PrivSceneAttacherLoadUIScene(strUISceneName, delFinish);
		}
		else
		{
			delFinish?.Invoke(false);
		}
	}

	protected void ProtSceneAttacherDestroy()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			GameObject Child = transform.GetChild(i).gameObject;
			Destroy(Child);
		}

		Destroy(gameObject);
	}

	//----------------------------------------------------------
	private void PrivSceneAttacherLoadResourcePrefab(string strPrefabPath, UnityAction<bool> delFinish)
	{
		GameObject Prefab = Instantiate(Resources.Load(strPrefabPath), Vector3.zero, Quaternion.identity) as GameObject;
		if (Prefab == null)
		{
			Debug.LogError("No Exist Prefab================");
		}
		else
		{
			ResetGameObjectName(Prefab);
			DontDestroyOnLoad(Prefab);
		}
		delFinish?.Invoke(true);
	}

	private void PrivSceneAttacherLoadAddressablePrefab(string strPrefabName, UnityAction<bool> delFinish)
	{
		Addressables.InitializeAsync().Completed += (AsyncOperationHandle<IResourceLocator> Result) =>
		{
			AsyncOperationHandle<GameObject> LoadObject = Addressables.InstantiateAsync(strPrefabName);
			LoadObject.Completed += (AsyncOperationHandle<GameObject> Result) =>
			{
				ResetGameObjectName(Result.Result);
				Result.Result.gameObject.SetActive(true);
				DontDestroyOnLoad(Result.Result);
				delFinish?.Invoke(true);
			};
		};
	}

	private void PrivSceneAttacherLoadUIScene(string strUISceneName, UnityAction<bool> delFinish)
	{
		Addressables.InitializeAsync().Completed += (AsyncOperationHandle<IResourceLocator> Result) =>
		{
			Addressables.LoadSceneAsync(strUISceneName, UnityEngine.SceneManagement.LoadSceneMode.Additive).Completed += (AsyncOperationHandle<SceneInstance> Result) =>
			{
				if (Result.Status != AsyncOperationStatus.Succeeded)
				{
					Debug.LogError("No Exist UIScene");
				}
				delFinish?.Invoke(true);
			};
		};
	}

}
