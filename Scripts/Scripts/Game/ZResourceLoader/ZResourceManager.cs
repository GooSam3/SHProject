using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Zero;

public class ZResourceManager : ManagerResourceBase
{
	public static new ZResourceManager Instance { get { return ManagerResourceBase.Instance as ZResourceManager; } }
	//----------------------------------------------------------------------
	protected override void OnAddressableError(string _addressableName, string _error)
	{
		ZLog.LogError(ZLogChannel.UI, string.Format("[Addressable] {0} Error : {1}", _addressableName, _error));
	}
	//----------------------------------------------------------------------
	/// <summary>
	/// 게임 오브젝트용 
	/// </summary>
	public void Instantiate(string _addressableName, UnityAction<string, GameObject> _eventFinish, int _priority = 100, UnityAction<string, float> _eventProgress = null)
	{
		RequestLoad(_addressableName, _priority, (string _loadedName, object _loadedObject) =>
		{
			if (_loadedObject == null)
			{
				_eventFinish?.Invoke(_loadedName, null);
			}
			else
			{
				GameObject CloneInstance = Instantiate(_loadedObject as GameObject, gameObject.transform);
				CloneInstance.SetActive(false);
				_eventFinish?.Invoke(_loadedName, CloneInstance);
			}
		}
		, _eventProgress);
	}

	public void Instantiate(List<string> _listAddressableName, UnityAction<List<GameObject>> _listEventFinish, int _priority = 100)
	{
		List<GameObject> listGameObject = new List<GameObject>();
		for (int i = 0; i < _listAddressableName.Count; i++)
		{
			Instantiate(_listAddressableName[i], (string _loadedName, GameObject _loadedObject) =>
			{
				listGameObject.Add(_loadedObject);
				if (listGameObject.Count == _listAddressableName.Count)
				{
					_listEventFinish?.Invoke(listGameObject);
				}
			}
			, _priority);
		}
	}


	/// <summary>
	/// 로드된 GameObject의 복제본 생성후, <typeparamref name="COMPONENT_TYPE"/>컴포넌트를 얻어서 리턴해준다.
	/// </summary>
	public void Instantiate<COMPONENT_TYPE>(string _addressableName, UnityAction<string, COMPONENT_TYPE> _eventFinish, int _priority = 100, UnityAction<string, float> _eventProgress = null) where COMPONENT_TYPE : Component
	{
		Instantiate(_addressableName, (string _loadedName, GameObject _loadedObject) =>
		{
			if (_loadedObject)
			{
				COMPONENT_TYPE component = _loadedObject.GetComponent<COMPONENT_TYPE>();
				_eventFinish?.Invoke(_loadedName, component);
			}
			else
			{
				_eventFinish?.Invoke(_loadedName, null);
			}
		}, _priority, _eventProgress);
	}

	/// <summary>
	/// </summary>
	public void Load<TYPE>(string _addressableName, UnityAction<string, TYPE> _eventFinish, int _priority = 100, UnityAction<string, float> _eventProgress = null) where TYPE : UnityEngine.Object
	{
		RequestLoad(_addressableName, _priority, (string _loadedName, object _loadedObject) =>
		{
			_eventFinish?.Invoke(_loadedName, _loadedObject as TYPE);
		}
		, _eventProgress);
	}


	public void GetTexture2DFromUrl(string url, string hashCode, Action<Texture2D> onLoadTexture, string subDir)
	{
		string path = $"{Auth.DataPath}/{subDir}";

		if (System.IO.Directory.Exists(path) == false)
			System.IO.Directory.CreateDirectory(path);

		string[] urlSplit = url.Split('/');

		if (urlSplit.Length <= 0)
		{
			ZLog.Log(ZLogChannel.Event, $"URL 재확인요망 {url}");
			return;
		}

		string textureName = urlSplit[urlSplit.Length - 1];

		string filePath = $"{path}/{textureName}";

		if (CheckDiff(filePath, hashCode) == false)
		{
			Texture2D texture = new Texture2D(2, 2);
			texture.LoadImage(System.IO.File.ReadAllBytes(filePath));
			onLoadTexture?.Invoke(texture);
		}
		else
		{
            ZWebManager.Instance.DownloadFile(url, (handler) =>
            {
                System.IO.File.WriteAllBytes(filePath, handler.data);

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(System.IO.File.ReadAllBytes(filePath));

                if (onLoadTexture?.Target != null)
                    onLoadTexture?.Invoke(texture);
            });
		}
	}

	public bool CheckDiff(string filePath, string hash)
	{
		if (System.IO.File.Exists(filePath) == false)
			return true;

		byte[] arrBinData = System.IO.File.ReadAllBytes(filePath);

		if (hash.Equals(MD5.ComputeMD5(arrBinData)) == false)
			return true;

		return false;
	}

	/*private IEnumerator CoDownloadFile(string url, Action<UnityEngine.Networking.DownloadHandler> onLoadEnd)
	{
		int rnd = UnityEngine.Random.Range(0, 1000000);

#if UNITY_EDITOR
		url = url.Replace("file:///", "file://");
#endif
		ZLog.Log(ZLogChannel.Event, $"이미지 다운로드 시작 : {url}");

		using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url + "?p=" + rnd))
		{
			yield return www.SendWebRequest();

			if (www.isHttpError || www.isNetworkError || www.error != null)
			{
				ZLog.Log(ZLogChannel.Event, $"이미지 다운로드 에러남 error : {www.error}");
			}
			else
			{
				onLoadEnd?.Invoke(www.downloadHandler);
			}
		}
	}*/

}
