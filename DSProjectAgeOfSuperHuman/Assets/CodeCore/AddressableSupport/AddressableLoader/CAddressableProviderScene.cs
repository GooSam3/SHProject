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
                float fDownloadPercent = mAsyncHandle.GetDownloadStatus().Percent;
                if (fDownloadPercent == 0)
				{
                    PrivLoadSceneLocalLoad(mAsyncHandle);
                }
                else
				{
					LoadProgress(fDownloadPercent);
				}
			}
        }
    }

    protected override void OnLoadStart()
    {
        mAsyncHandle = Addressables.LoadSceneAsync(mAddressableName, UnityEngine.SceneManagement.LoadSceneMode.Additive, true, 1);
        mAsyncHandle.Completed += HandleLoadSceneComplete;
    }
    //-------------------------------------------------------------------------
    private void PrivLoadSceneLocalLoad(AsyncOperationHandle<SceneInstance> pAsyncHandle)
	{
        LoadProgress(ExtractAddressableLoadProgress(pAsyncHandle));
    }

    //-------------------------------------------------------------------------
    private void HandleLoadSceneComplete(AsyncOperationHandle<SceneInstance> _LoadedScene)
    {
        // [주의!] 씬 인스턴스는 로드 되었으나 씬 내부의 게임 오브젝트가 Awake되었는지는 불분명하다.
        //         디바이스 상태에 따라 Finish 이벤트 발생시 Awake가 모두 호출되지 않을수도 있다.
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
