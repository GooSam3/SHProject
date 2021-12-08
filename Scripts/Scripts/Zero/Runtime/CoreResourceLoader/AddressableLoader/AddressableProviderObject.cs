using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableProviderObject : AddressableProviderBase<AddressableProviderObject>
{
    private AsyncOperationHandle<object> mAsyncHandle;
    //------------------------------------------------------------------------------

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
        mAsyncHandle = Addressables.LoadAssetAsync<object>(mAddressableName);
        mAsyncHandle.Completed += HandleLoadComplete;
    }

    //---------------------------------------------------------------------
    private void HandleLoadComplete(AsyncOperationHandle<object> _LoadedGameObject)
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
        Addressables.Release(mAsyncHandle);
    }
}
