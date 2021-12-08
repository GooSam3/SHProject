using UnityEngine.Events;

public abstract class AddressableProviderBase<TEMPLATE> : CObjectMemoryPoolBase<TEMPLATE> where TEMPLATE : AddressableProviderBase<TEMPLATE>, new()
{
    private event UnityAction<string, float>     mEventProgress = null;
    private event UnityAction<string, object>    mEventFinish = null;
    //-----------------------------------------------------------
    private UnityAction<TEMPLATE, SLoadResult>   mEventLoadResult = null;
    private UnityAction<TEMPLATE, string, string>         mEventError = null;
    //----------------------------------------------------------
    protected string mAddressableName;
    private int  mPriority = 0;            public int pPriority { get { return mPriority; } }                          
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
    public void SetLoadPrepare(string _AddressableName, int _Priority, UnityAction<TEMPLATE, SLoadResult> _EventLoadResult, UnityAction<TEMPLATE, string, string> _EventError)
    {
        mAddressableName = _AddressableName;
        mPriority = _Priority;
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

    //--------------------------------------------------------------------------------------
    protected sealed override void OnPoolObjectActivate() { }
    protected sealed override void OnPoolObjectDeactivate()
    {
        mEventLoadResult = null;
        mEventProgress = null;
        mEventFinish = null;
        mEventError = null;
        mAddressableName = null;
        mPriority = 0;
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
