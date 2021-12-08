using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class CAddressableProviderObject : CAddressableProviderBase<CAddressableProviderObject>
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
        // 원본을 미리 해재하므로 사용자 측에서 사본을 폐기하는 순간 메모리에서 내려간다.
        // 원본 해재를 하지 않을 경우 사용자측은 모든 사본을 폐기하고 원본을 폐기해야 한다.
        Addressables.Release(mAsyncHandle);
    }
}
