using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SHManagerResourceLoader : CManagerResourceBase
{
	public static new SHManagerResourceLoader Instance { get { return CManagerResourceBase.Instance as SHManagerResourceLoader; } }
	//----------------------------------------------------------------------
	protected override void OnAddressableError(string _addressableName, string _error)
	{
		Debug.LogError(string.Format("[Addressable] {0} Error : {1}", _addressableName, _error));
	}
	//----------------------------------------------------------------------
	/// <summary>
	/// 게임 오브젝트용. 원본은 레퍼런스 해재된 상태
	/// </summary>
	public void LoadPrefab(string _addressableName, UnityAction<string, GameObject> _eventFinish, UnityAction<string, float> _eventProgress = null)
	{
		RequestLoad(_addressableName, (string _loadedName, object _loadedObject) =>
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

	public void LoadPrefabList(List<string> _listAddressableName, UnityAction<List<GameObject>> _listEventFinish)
	{
		List<GameObject> listGameObject = new List<GameObject>();
		for (int i = 0; i < _listAddressableName.Count; i++)
		{
			LoadPrefab(_listAddressableName[i], (string _loadedName, GameObject _loadedObject) =>
			{
				listGameObject.Add(_loadedObject);
				if (listGameObject.Count == _listAddressableName.Count)
				{
					_listEventFinish?.Invoke(listGameObject);
				}
			}
			);
		}
	}


	/// <summary>
	/// 로드된 GameObject의 복제본 생성후, 컴포넌트를 얻어서 리턴해준다.
	/// </summary>
	public void LoadComponent<COMPONENT>(string _addressableName, UnityAction<string, COMPONENT> _eventFinish, UnityAction<string, float> _eventProgress = null) where COMPONENT : Component
	{
		LoadPrefab(_addressableName, (string _loadedName, GameObject _loadedObject) =>
		{
			if (_loadedObject)
			{
				COMPONENT component = _loadedObject.GetComponent<COMPONENT>();
				_eventFinish?.Invoke(_loadedName, component);
			}
			else
			{
				_eventFinish?.Invoke(_loadedName, null);
			}
		}, _eventProgress);
	}

	/// <summary>
	/// 텍스처와 같은 일반 에셋을 로드
	/// </summary>
	public void LoadResourceAsset<TYPE>(string _addressableName, UnityAction<string, TYPE> _eventFinish, UnityAction<string, float> _eventProgress = null) where TYPE : UnityEngine.Object
	{
		RequestLoad(_addressableName, (string _loadedName, object _loadedObject) =>
		{
			_eventFinish?.Invoke(_loadedName, _loadedObject as TYPE);
		}
		, _eventProgress);
	}
}
