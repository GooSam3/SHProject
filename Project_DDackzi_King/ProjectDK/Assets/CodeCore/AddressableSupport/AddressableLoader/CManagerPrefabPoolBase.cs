using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

// 일반적인 오브젝트 풀링 기능 수행 

public abstract class CManagerPrefabPoolBase : CManagerAddressableBase<CManagerPrefabPoolBase, CAddressableProviderGameObject>
{   
	private class SPrefabPool
	{
		public string				Category;
		public string				AddressableName;
		public GameObject 		OrginInstance;
		public GameObject			Parent;
		public bool				AutoRelease = false;
		public Queue<GameObject>	CloneStock = new Queue<GameObject>();
		public List<GameObject>	CloneList = new List<GameObject>();
		public int GetReferenceCount() { return CloneList.Count; }
	}

	private Dictionary<string, Dictionary<string, SPrefabPool>>	m_dicPrefabInstance   = new Dictionary<string, Dictionary<string, SPrefabPool>>();
	private Dictionary<GameObject, SPrefabPool>					m_dicPrefabMap		= new Dictionary<GameObject, SPrefabPool>(); // 검색 비용 절감을 위한 메모리 사용
	private Dictionary<string, GameObject>						m_dicPrefabParent		= new Dictionary<string, GameObject>();
    //------------------------------------------------------------------

	/// <summary>
	/// _autoRelease 옵션의 경우 모든 클론이 반환될 경우 자동으로 메모리를 해재한다. 다음 호출때는 에셋번들에서 읽어오게 되므로 주의할것.
	/// </summary>
    protected void ReserveInstance(string _category, string _addressableName, int _reserveCount, UnityAction<string, GameObject> _eventFinish, bool _autoRelease, bool _cloneInstance)
	{
		Dictionary<string, SPrefabPool> PrefabPool = FindPrefabPoolOrAlloc(_category);

		if (_cloneInstance == false)
		{
			_reserveCount = 0;
			_autoRelease = false;  // 프리로드 상태일경우 무조건 자율해제가 되므로 강제로 꺼준다.
		}

		if (PrefabPool.ContainsKey(_addressableName))
		{
			GameObject CloneInstance = RequestClone(_category, _addressableName);
			_eventFinish?.Invoke(_addressableName, CloneInstance);
		}
		else
		{
			RequestLoad(_addressableName, (string _LoadedAddressableName, object _LoadedGameObject) =>
			{
				ReserveInternal(_category, PrefabPool, _LoadedAddressableName, _LoadedGameObject as GameObject, _reserveCount, _autoRelease);
				if (_eventFinish != null)
				{
					GameObject CloneInstance = null;
					if (_cloneInstance)
					{
						CloneInstance = RequestClone(_category, _addressableName);
					}					
					_eventFinish?.Invoke(_LoadedAddressableName, CloneInstance);
				}
			}, null);
		}

		OnPrefabRequestOrigin(_addressableName);
	}

    protected GameObject RequestClone(string _category, string _addressableName)
	{
		GameObject Instance = null;
		Dictionary<string, SPrefabPool> PrefabInstance = FindPrefabPoolOrAlloc(_category);
		if (PrefabInstance.ContainsKey(_addressableName))
		{
			SPrefabPool PrefabPool = PrefabInstance[_addressableName];
			if (PrefabPool.CloneStock.Count == 0)
			{
				AllocateCloneInstance(PrefabPool);
			}
			Instance = PrefabPool.CloneStock.Dequeue();
		}
		return Instance;
	}

    protected void ReturnClone(GameObject _returnObject)
	{
		SPrefabPool Prefab = FindPrefabPool(_returnObject);
		if (Prefab != null)
		{
			_returnObject.transform.SetParent(Prefab.Parent.transform, false);
			_returnObject.SetActive(false);
			Prefab.CloneStock.Enqueue(_returnObject);

			if (Prefab.AutoRelease)
			{
				if (Prefab.CloneList.Count == Prefab.CloneStock.Count) // 외부로 나간 클론이 모두 돌아오면 메모리를 해제한다.
				{
					RemoveInstance(Prefab.Category, Prefab.AddressableName);
				}
			}
		}		
	}

    protected void RemoveInstance(string _category, string _addressable)
	{
		Dictionary<string, SPrefabPool> PrefabInstance = FindPrefabPoolOrAlloc(_category);
		if (PrefabInstance.ContainsKey(_addressable))
		{
			RemoveInternal(PrefabInstance[_addressable]);
			PrefabInstance.Remove(_addressable);
		}
	}

	protected void RemoveInstanceAll()
	{
		Dictionary<string, Dictionary<string, SPrefabPool>>.Enumerator it = m_dicPrefabInstance.GetEnumerator();

		while (it.MoveNext())
		{
			DeletePrefabParent(it.Current.Key);
			RemoveCategoryInternal(it.Current.Value);
		}

		m_dicPrefabInstance.Clear();
		m_dicPrefabMap.Clear();
	}

	protected void RemoveCategory(string _category)
	{
		Dictionary<string, SPrefabPool> PrefabPool = FindPrefabPoolOrAlloc(_category);
		if (PrefabPool != null)
		{
			DeletePrefabParent(_category);
			RemoveCategoryInternal(PrefabPool);
			m_dicPrefabInstance.Remove(_category);
		}
	}
	
    //------------------------------------------------------------------
    private void AllocateCloneInstance(SPrefabPool _prefabPool)
	{
		GameObject Instance = Instantiate(_prefabPool.OrginInstance, _prefabPool.Parent.transform);		
		_prefabPool.CloneList.Add(Instance);
		_prefabPool.CloneStock.Enqueue(Instance);
		m_dicPrefabMap.Add(Instance, _prefabPool);

		OnPrefabInstanceClone(_prefabPool.AddressableName, Instance);
    }

	private void ReserveInternal(string _category, Dictionary<string, SPrefabPool> _prefabPool, string _addressableName, GameObject _originInstance, int _reserveCount, bool _autoRelease)
	{
		if (_originInstance == null) return;

		SPrefabPool NewInstance = null;

		if (_prefabPool.ContainsKey(_addressableName))
		{
			NewInstance = _prefabPool[_addressableName];
		}
		else
		{
			NewInstance = new SPrefabPool();
			NewInstance.Category = _category;
			NewInstance.AutoRelease = _autoRelease;
			NewInstance.OrginInstance = _originInstance;
			NewInstance.Parent = FindParentOrAlloc(_category);
			NewInstance.AddressableName = _addressableName;
			_prefabPool[_addressableName] = NewInstance;
		}
		for (int i = 0; i < _reserveCount; i++)
		{
			AllocateCloneInstance(NewInstance);
		}

		OnPrefabInstanceOrigin(_addressableName, _originInstance);
	}

    private void RemoveInternal(SPrefabPool _removePrefab)
	{
		_removePrefab.CloneStock.Clear();

		for (int i = 0; i < _removePrefab.CloneList.Count; i++)
		{
			GameObject gameObjectInstance = _removePrefab.CloneList[i];			
			OnPrefabRemoveClone(_removePrefab.AddressableName, gameObjectInstance);
			m_dicPrefabMap.Remove(gameObjectInstance);
			Destroy(gameObjectInstance);
		}
		_removePrefab.CloneList.Clear();

		OnPrefabRemoveOrigin(_removePrefab.AddressableName, _removePrefab.OrginInstance);
		Addressables.ReleaseInstance(_removePrefab.OrginInstance);
		_removePrefab.OrginInstance = null;
	}

	private void RemoveCategoryInternal(Dictionary<string, SPrefabPool> _category)
	{
		Dictionary<string, SPrefabPool>.Enumerator it = _category.GetEnumerator();
		while(it.MoveNext())
		{
			RemoveInternal(it.Current.Value);
		}
		_category.Clear();
	}

	private SPrefabPool FindPrefabPool(GameObject _cloneInstance)
	{
		SPrefabPool PrefabPool = null;
		if (m_dicPrefabMap.ContainsKey(_cloneInstance))
		{
			PrefabPool = m_dicPrefabMap[_cloneInstance];
		}
		return PrefabPool;
	}

	private Dictionary<string, SPrefabPool> FindPrefabPoolOrAlloc(string _category)
	{
		Dictionary<string, SPrefabPool> Find = null;
		if (m_dicPrefabInstance.ContainsKey(_category))
		{
			Find = m_dicPrefabInstance[_category];
		}
		else
		{
			Find = new Dictionary<string, SPrefabPool>();
			m_dicPrefabInstance.Add(_category, Find);
		}

		return Find;
	}

	private GameObject FindParentOrAlloc(string _category)
	{
		GameObject Find = null;
		if (m_dicPrefabParent.ContainsKey(_category))
		{
			Find = m_dicPrefabParent[_category];
		}
		else
		{
			Find = MakePrefabParent(_category);
		}

		return Find;
	}

	private GameObject MakePrefabParent(string _category)
	{
		GameObject Parent = new GameObject();
		Parent.name = _category;
		Parent.transform.SetParent(transform);
		m_dicPrefabParent.Add(_category, Parent);
		return Parent;
	}

	private void DeletePrefabParent(string _category)
	{
		if (m_dicPrefabParent.ContainsKey(_category))
		{
			GameObject Parent = m_dicPrefabParent[_category];
			m_dicPrefabParent.Remove(_category);
			Destroy(Parent);
		}
	}

	//------------------------------------------------------------------
	protected virtual void OnPrefabRequestOrigin(string _addressableName) { }
	protected virtual void OnPrefabInstanceOrigin(string _addressableName, GameObject _originInstance) { }
	protected virtual void OnPrefabInstanceClone(string _addressableName, GameObject _cloneInstance) { }
	protected virtual void OnPrefabRemoveClone(string _addressableName, GameObject _removeClone) { } // 하위 레이어에서는 이 게임 오브젝트의 소유자에게 이벤트를 전달해야 한다.
    protected virtual void OnPrefabRemoveOrigin(string _addressableName, GameObject _removeOrigin) { }
}
