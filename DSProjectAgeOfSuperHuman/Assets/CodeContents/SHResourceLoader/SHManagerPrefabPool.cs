using UnityEngine;
using UnityEngine.Events;


public enum EPoolType
{
	UI,
	Unit,
	Effect,
}

public class SHManagerPrefabPool : CManagerPrefabAutoReleaseBase
{	public static new SHManagerPrefabPool Instance { get { return CManagerPrefabPoolBase.Instance as SHManagerPrefabPool; } }
	
    //--------------------------------------------------------------	
    public void LoadGameObject(EPoolType _poolType, string _addressableName, UnityAction<GameObject> _eventFinish, int _reserveCount = 1, bool bActiveSelf = false, bool _autoRelease = false)
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
            if (null != _loadedObject)
            {
                _loadedObject.SetActive(bActiveSelf);
            }

            _eventFinish?.Invoke(_loadedObject);
		}, _autoRelease);
	}

    public void LoadComponent<COMPONENT>(EPoolType _poolType, string _addressableName, UnityAction<COMPONENT> _evenFinish, int _reserveCount = 1, bool bActiveSelf = false, bool _autoRelease = false) where COMPONENT : Component
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
            if (null != _loadedObject)
            {
                _loadedObject.SetActive(bActiveSelf);
            }

            COMPONENT Component = _loadedObject.GetComponent<COMPONENT>();
            _evenFinish?.Invoke(Component);
		}, _autoRelease);
	}


	/// <summary>
	/// 별도의 Clone을 생성하지 않는다.  에셋번들에서 로딩만 수행한다.  수동으로 해제하지 않으면 메모리에 상주하므로 주의. 
	/// </summary>
	public void LoadInstance(EPoolType _poolType, string _addressableName, UnityAction _eventFinish, int _reserveCount = 0, bool bActiveSelf = true, bool _autoRelease = false)
	{
		ReserveInstance(_poolType.ToString(), _addressableName, _reserveCount, (string _loadedAddressable, GameObject _loadedObject) =>
		{
			_eventFinish?.Invoke();
		}, false);
	}


	//-----------------------------------------------------------------
	/// <summary>
	/// 간편하게 사용할수 있는 동기함수. Reserve가 되어 있지 않으면 Null을 반환
	/// </summary>
	public GameObject FindClone(EPoolType _poolType, string _addressableName)
	{
		GameObject CloneInstance = RequestClone(_poolType.ToString(), _addressableName);
		if (CloneInstance == null)
		{
			Debug.LogWarning($"[ZPoolMananger] There is no reserve instance.  call Spawn() and reserve an Instance. | {_addressableName}");
		}
		return CloneInstance;
	}

	//----------------------------------------------------------
	public void ClearAll(EPoolType _poolType, string _addressableName, bool bOnlyRemovePrefab = false)
	{
		RemoveInstance(_poolType.ToString(), _addressableName, bOnlyRemovePrefab);
	}

	public void ClearCategory(EPoolType _poolType)
	{
		RemoveCategory(_poolType.ToString());
	}

	public void ClearAll(bool bOnlyRemovePrefab = false)
	{
		if (bOnlyRemovePrefab)
		{
			RemoveInstanceAllSoft();
		}
		else
		{
			RemoveInstanceAll();
		}
	}

	public void Return(GameObject _returnObject)
	{
		ReturnClone(_returnObject);
	}

	//-------------------------------------------------------------
	
}
