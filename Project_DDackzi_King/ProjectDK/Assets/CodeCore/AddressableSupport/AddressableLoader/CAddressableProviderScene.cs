using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class CAddressableProviderScene : CAddressableProviderBase<CAddressableProviderScene>
{
    private AsyncOperationHandle<SceneInstance> mAsyncHandle;
    //--------------------------------------------------------------------------
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
        mAsyncHandle = Addressables.LoadSceneAsync(mAddressableName, UnityEngine.SceneManagement.LoadSceneMode.Additive, true, 1);
        mAsyncHandle.Completed += HandleLoadSceneComplete;
    }

    //-------------------------------------------------------------------------
    private void HandleLoadSceneComplete(AsyncOperationHandle<SceneInstance> _LoadedScene)
    {
        if (_LoadedScene.IsValid())
        { 
            if (_LoadedScene.Status == AsyncOperationStatus.Failed)
            {
                LoadError(_LoadedScene.OperationException.Message);
            }
            else
            {
                SLoadResult LoadResult = new SLoadResult(mAddressableName, _LoadedScene.Result);
                LoadFinish(ref LoadResult);
            }
        }
  
    }

}
