using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceProviders;

public abstract class CManagerAddressableBase<TEMPLATE, INSTANCE> : CManagerTemplateBase<CManagerAddressableBase<TEMPLATE, INSTANCE>> where TEMPLATE : CManagerAddressableBase<TEMPLATE, INSTANCE> where INSTANCE : CAddressableProviderBase<INSTANCE>, new()
{
	/// <summary> 동시에 로드가능한 개수 </summary>
	public int ConcurrentCount
	{
		get => mConCurrentCount;
		set => mConCurrentCount = Mathf.Max(1, value);
	}
    private int mConCurrentCount = 1;
    private LinkedList<INSTANCE>              m_listCurrentProvider = new LinkedList<INSTANCE>();    
    private Queue<INSTANCE> m_queStandByProvider = new Queue<INSTANCE>(); 
    //----------------------------------------------------------------------
    public bool HasLoadingWork()
	{
        bool work = true;

        if (m_listCurrentProvider.Count == 0 && m_queStandByProvider.Count == 0)
		{
            work = false;
		}

        return work;
	}
    //----------------------------------------------------------------------
    protected virtual void Update()
    {
        if (m_listCurrentProvider.Count > 0)
        {
            foreach(INSTANCE CurrentProvider in m_listCurrentProvider)
			{
                CurrentProvider.UpdateLoadWork();
			}
        }
       
        NextProvider();        
    }

    //-----------------------------------------------------------------------
    protected void RequestLoad(string _addressableName, UnityAction<string, object> _eventFinish, UnityAction<string, float> _eventProgress = null)
	{      
        // 중복로드 요청이 있을 경우 델리게이트만 연결한다.
        INSTANCE provider = FindProvider(_addressableName);
        if (provider == null)
		{
            provider = CAddressableProviderBase<INSTANCE>.InstanceActivate();
            provider.SetLoadPrepare(_addressableName, HandleLoadResult, HandleLoadError);
            provider.SetLoadEventAdd(_eventProgress, _eventFinish);
            m_queStandByProvider.Enqueue(provider);
        }
        else
		{
            provider.SetLoadEventAdd(_eventProgress, _eventFinish);
		}

    }

    //-----------------------------------------------------------------------
    private void NextProvider()
    {
        if (m_queStandByProvider.Count == 0) return;

        int EmptyCount = mConCurrentCount - m_listCurrentProvider.Count;

        for (int i = 0; i < EmptyCount; i++)
		{
            INSTANCE Provider = m_queStandByProvider.Dequeue();
            m_listCurrentProvider.AddLast(Provider);
            Provider.DoLoadStart();
		}
    }

    private void DeleteProvider(INSTANCE _provider)
    {
        CAddressableProviderBase<INSTANCE>.InstanceDeactivate(_provider);
        m_listCurrentProvider.Remove(_provider);
    }
   
    private INSTANCE FindProvider(string _addressableName)
	{
        INSTANCE Find = null;
        LinkedList<INSTANCE>.Enumerator itCurrent = m_listCurrentProvider.GetEnumerator();
        while(itCurrent.MoveNext())
		{
            if (itCurrent.Current.GetAddresableName() == _addressableName)
            {
                Find = itCurrent.Current;
                break;
            }
		}

        if (Find == null)
		{
            IEnumerator<INSTANCE> itStandBy = m_queStandByProvider.GetEnumerator();
            while(itStandBy.MoveNext())
			{
                if (itStandBy.Current.GetAddresableName() == _addressableName)
				{
                    Find = itStandBy.Current;
                    break;
				}
			}
        }

        return Find;
	}

    //-----------------------------------------------------------------------
    private void HandleLoadResult(INSTANCE _provider, CAddressableProviderBase<INSTANCE>.SLoadResult _loadResult)
    {       
        // 참고 : 추출된 GameObject는 프리팹 인스턴스로 GC되지 않으며 Transform을 사용할 수 없다. 
        DeleteProvider(_provider);
        
        if (_loadResult.LoadedObject as GameObject)
		{
            OnAddressableLoadGameObject(_loadResult.AddressableName, _loadResult.LoadedObject as GameObject);
        }
        else
		{
            if (_loadResult.LoadedObject.GetType().IsValueType)
			{
                SceneInstance TypeCastScene = (SceneInstance)_loadResult.LoadedObject;
                OnAddressableLoadScene(_loadResult.AddressableName, ref TypeCastScene);
            }
            else 
			{
                OnAddressableLoadObject(_loadResult.AddressableName, _loadResult.LoadedObject as Object);
            }
		}
    }

    private void HandleLoadError(INSTANCE _provider, string _addressableName, string _error)
	{
        DeleteProvider(_provider);
        OnAddressableError(_addressableName, _error);
    }
    //----------------------------------------------------------------------
    protected virtual void OnAddressableLoadGameObject(string _AddressableName, GameObject _loadedGameObject) { }
    protected virtual void OnAddressableLoadObject(string _addressableName, Object _loadObject) { }
    protected virtual void OnAddressableLoadScene(string _AddressableName, ref SceneInstance _SceneInstance) { }  
    protected virtual void OnAddressableError(string _AddressableName, string _Error) { }
    
}
