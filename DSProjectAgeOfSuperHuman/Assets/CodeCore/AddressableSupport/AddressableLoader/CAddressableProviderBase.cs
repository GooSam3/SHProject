using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading;
public abstract class CAddressableProviderBase<TEMPLATE> : CObjectMemoryPoolBase<TEMPLATE> where TEMPLATE : CAddressableProviderBase<TEMPLATE>, new()
{
    private event UnityAction<string, float>     mEventProgress = null;
    private event UnityAction<string, object>    mEventFinish = null;
    //-----------------------------------------------------------
    private UnityAction<TEMPLATE, SLoadResult>   mEventLoadResult = null;
    private UnityAction<TEMPLATE, string, string>         mEventError = null;
    //----------------------------------------------------------
    protected string mAddressableName;                        
    private bool mUpdateWork = false; 
    //---------------------------------------------------------------------------------------
    public void UpdateLoadWork()
    {
        if (mUpdateWork)
        {
            OnLoadUpdate();
        }
    }

    //-----------------------------------------------------------------------------------------
    public void SetLoadPrepare(string _AddressableName, UnityAction<TEMPLATE, SLoadResult> _EventLoadResult, UnityAction<TEMPLATE, string, string> _EventError)
    {
        mAddressableName = _AddressableName;
        mEventLoadResult = _EventLoadResult;
        mEventError = _EventError;
    }

    public void SetLoadEventAdd(UnityAction<string, float> _EventProgress, UnityAction<string, object> _EventFinish)
    {
        if (_EventProgress != null)
		{
            mEventProgress += _EventProgress;
        }
        if (_EventFinish != null)
		{
            mEventFinish += _EventFinish;
        }
    }

    public string GetAddresableName() 
    { 
        return mAddressableName; 
    }

    public void DoLoadStart()
    {
        mUpdateWork = true;
        OnLoadStart();
    }

    //-------------------------------------------------------------------------------------
    protected void LoadProgress(float _Progress)
    {
        mEventProgress?.Invoke(mAddressableName, _Progress);
    }

    protected void LoadError(string _ErrorMessage)
    {
        mUpdateWork = false;
        mEventFinish?.Invoke(mAddressableName, null);
        mEventError?.Invoke(this as TEMPLATE, mAddressableName, _ErrorMessage);
    }

    protected void LoadFinish(ref SLoadResult _LoadResult)
    {
        mUpdateWork = false;
        mEventFinish?.Invoke(mAddressableName, _LoadResult.LoadedObject);
        mEventLoadResult?.Invoke(this as TEMPLATE, _LoadResult);
     }

	protected float ExtractAddressableLoadProgress(AsyncOperationHandle _handle)
	{
		List<AsyncOperationHandle> listDependencies = new List<AsyncOperationHandle>();
        _handle.GetDependencies(listDependencies);

        if (listDependencies.Count > 0)
		{
			AsyncOperationHandle defHandle = listDependencies[0];
			listDependencies.Clear();
			defHandle.GetDependencies(listDependencies);
		}
        else
		{
            return _handle.PercentComplete;
		}

        float fProgress = 0;
		uint iCount = 0;
		for (int i = 0; i < listDependencies.Count; i++)
		{
			if (listDependencies[i].Status == AsyncOperationStatus.Succeeded)
			{
				iCount++;
			}
		}
        fProgress = (float)iCount / (float)listDependencies.Count;
		return fProgress;
	}


	//--------------------------------------------------------------------------------------
	protected sealed override void OnPoolObjectActivate() { }
    protected sealed override void OnPoolObjectDeactivate()
    {
        mEventLoadResult = null;
        mEventProgress = null;
        mEventFinish = null;
        mEventError = null;
        mAddressableName = null;       
        mUpdateWork = false;
    }
    //---------------------------------------------------------------------------------------
    protected virtual void OnLoadUpdate() { }
    protected virtual void OnLoadStart() { }
    //-----------------------------------------------------------------------------------------
    public struct SBundleInfo
    {
        public string BundleFileName;
        public string BundleName;
        public long   BundleDiskSize;
    }

    public struct SLoadResult
    {
        public string        AddressableName;
        public object        LoadedObject;
          
        public SLoadResult(string _AddressableName, object _loadedObject)
        {
            AddressableName = _AddressableName;
            LoadedObject = _loadedObject;           
        }
    }
}
