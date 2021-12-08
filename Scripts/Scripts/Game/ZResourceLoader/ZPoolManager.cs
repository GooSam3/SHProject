using UnityEngine;
using UnityEngine.Events;

public class ZPoolManager : ManagerPrefabAutoReleaseBase
{	public static new ZPoolManager Instance { get { return ManagerPrefabPoolBase.Instance as ZPoolManager; } }
	
    //--------------------------------------------------------------
	/// <summary>
	/// 
	/// </summary>
    public void Spawn(E_PoolType _poolType, string _addressableName, UnityAction<GameObject> _eventFinish, int _priority = 0, int _reserveCount = 1, bool bActiveSelf = true, bool _autoRelease = false)
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _priority, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
            if (null != _loadedObject)
            {
                _loadedObject.SetActive(bActiveSelf);
            }

            _eventFinish?.Invoke(_loadedObject);
		}, _autoRelease, true);
	}

    public void Spawn<COMPONENT_TYPE>(E_PoolType _poolType, string _addressableName, UnityAction<COMPONENT_TYPE> _evenFinish, int _priority = 0, int _reserveCount = 1, bool bActiveSelf = true, bool _autoRelease = false) where COMPONENT_TYPE : Component
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _priority, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
            if (null != _loadedObject)
            {
                _loadedObject.SetActive(bActiveSelf);
            }

            COMPONENT_TYPE Component = _loadedObject.GetComponent<COMPONENT_TYPE>();
            _evenFinish?.Invoke(Component);
		}, _autoRelease, true);
	}

	public void Spawn<COMPONENT_TYPE>(E_PoolType _poolType,  UnityAction<COMPONENT_TYPE> _evenFinish, int _priority = 0, int _reserveCount = 1, bool bActiveSelf = true, bool _autoRelease = false) where COMPONENT_TYPE : Component
	{
		ReserveInstance(_poolType.ToString(), typeof(COMPONENT_TYPE).ToString(), _priority, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
			if (null != _loadedObject)
			{
				_loadedObject.SetActive(bActiveSelf);
			}

			COMPONENT_TYPE Component = _loadedObject.GetComponent<COMPONENT_TYPE>();
			_evenFinish?.Invoke(Component);
		}, _autoRelease, true);
	}

	/// <summary>
	/// 별도의 Clone을 생성하지 않는다.  수동으로 해제하지 않으면 메모리에 상주하므로 주의. 
	/// </summary>
	public void SpawnFreeLoad(E_PoolType _poolType, string _addressableName, UnityAction _eventFinish, int _priority = 0, int _reserveCount = 1, bool bActiveSelf = true, bool _autoRelease = false)
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _priority, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
			_eventFinish?.Invoke();
		}, _autoRelease, false);
	}


	//-----------------------------------------------------------------
	/// <summary>
	/// 간편하게 사용할수 있는 동기함수. Reserve가 되어 있지 않으면 Null을 반환
	/// </summary>
	public GameObject FindClone(E_PoolType _poolType, string _addressableName)
	{
		GameObject CloneInstance = RequestClone(_poolType.ToString(), _addressableName);
		if (CloneInstance == null)
		{
			ZLog.LogWarn(ZLogChannel.Loading, $"[ZPoolMananger] There is no reserve instance.  call Spawn() and reserve an Instance. | {_addressableName}");
		}
		return CloneInstance;
	}

	//----------------------------------------------------------
	public void Clear(E_PoolType _poolType, string _addressableName)
	{
		RemoveInstance(_poolType.ToString(), _addressableName);
	}

	public void ClearCategory(E_PoolType _poolType)
	{
		RemoveCategory(_poolType.ToString());
	}

	public void ClearAll()
	{
		RemoveInstanceAll();
	}

	public void Return(GameObject _returnObject)
	{
		ReturnClone(_returnObject);
	}

	//-------------------------------------------------------------
	protected override void OnPrefabInstanceOrigin(string _addressableName, GameObject _originInstance)
	{
		base.OnPrefabInstanceOrigin(_addressableName, _originInstance);
	}

	protected override void OnPrefabRequestOrigin(string _addressableName)
	{
		base.OnPrefabRequestOrigin(_addressableName);
	}
}
