using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CAddressableProviderGameObject : CAddressableProviderBase<CAddressableProviderGameObject>
{
    private AsyncOperationHandle<GameObject> mAsyncHandle;
    //-------------------------------------------------------------------
    protected override void OnLoadUpdate()
    {
        if (mAsyncHandle.IsValid())
        {
            if (mAsyncHandle.Status == AsyncOperationStatus.Failed)
            {
                LoadError(mAsyncHandle.OperationException.ToString());
                return;
            }
            else
            {
                LoadProgress(mAsyncHandle.GetDownloadStatus().Percent);
            }
        }
    }

    protected override void OnLoadStart()
    {
        mAsyncHandle = Addressables.LoadAssetAsync<GameObject>(mAddressableName);
        mAsyncHandle.Completed += HandleLoadComplete;
    }

    //---------------------------------------------------------------------
    private void HandleLoadComplete(AsyncOperationHandle<GameObject> _LoadedGameObject)
    {
        if (_LoadedGameObject.IsValid())
        {
            if (_LoadedGameObject.Status == AsyncOperationStatus.Failed)
            {
                LoadError(_LoadedGameObject.OperationException.Message);
            }
            else 
            {
                SLoadResult LoadResult = new SLoadResult(mAddressableName, _LoadedGameObject.Result);
                LoadFinish(ref LoadResult);     
            }
        }
        else
        {
            LoadError(_LoadedGameObject.OperationException.ToString());
        }
    }
}
