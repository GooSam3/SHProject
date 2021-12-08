using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class ZSceneManager : ManagerSceneStreamBase
{   public static new ZSceneManager Instance { get { return ManagerSceneStreamBase.Instance as ZSceneManager; } }

    [SerializeField]
    private string WorldDescription = "WorldDescription";

    private UIFrameLoadingScreen    mLoadingScreen = null;
    private UnityAction<float>      mEventProgress = null;
    private UnityAction<string>     mEventFinish = null;
    private string                 mLoadingSceneName;
  	//--------------------------------------------------------------------
	protected override void OnSceneFinishMain(string _addressableName, SceneInstance _mainScene)
	{
        base.OnSceneFinishMain(_addressableName, _mainScene);
        if (mLoadingSceneName == _addressableName)
		{
            mEventFinish?.Invoke(_addressableName);
		}

        ZLog.Log(ZLogChannel.Loading, string.Format("[Addressable] OnSceneFinishMain : {0} ", _addressableName));
    }
    
    protected override void OnSceneOpenRootStage(string _loadedSceneName) 
    {
        base.OnSceneOpenRootStage(_loadedSceneName);
        mLoadingSceneName = _loadedSceneName;
        UIManager.Instance.Open<UIFrameLoadingScreen>(delegate { mLoadingScreen = UIManager.Instance.Find<UIFrameLoadingScreen>(); });
    }
    protected override void OnSceneFinishRootStage(string _loadedSceneName)
	{
        base.OnSceneFinishRootStage(_loadedSceneName);
        if (mLoadingSceneName == _loadedSceneName)
		{
            mEventFinish?.Invoke(_loadedSceneName);
        } 
    }

    protected override void OnSceneProgress(string _addressableName, float _progress)
	{
        base.OnSceneProgress(_addressableName, _progress);

        mEventProgress?.Invoke(_progress);

        if (mLoadingScreen != null)
		{
            mLoadingScreen.DoUIFrameLoadingScreen(_progress);
            ZLog.Log(ZLogChannel.Loading, $"OnSceneProgress : {_progress}");
        }
	}

	protected override void OnAddressableError(string _addressableName, string _Error)
	{
        ZLog.Log(ZLogChannel.Map, string.Format("[Addressable Map]{0} : {1}", _addressableName, _Error));
    }
	//---------------------------------------------------------------------
	public void OpenMain(string _addressableScene, UnityAction<float> _eventProgress, UnityAction<string> _eventFinish)
	{      
        mEventFinish = _eventFinish;
        mEventProgress = _eventProgress;
        mLoadingSceneName = _addressableScene;

        LoadMainScene(_addressableScene);
        UIManager.Instance.Open<UIFrameLoadingScreen>(delegate { mLoadingScreen = UIManager.Instance.Find<UIFrameLoadingScreen>(); });

        ZLog.Log(ZLogChannel.Loading, string.Format("[Addressable] OpenMain : {0} ", _addressableScene));
    }

    public void OpenAdditive(string _addressableScene, UnityAction<float> _eventProgress, UnityAction<string> _eventFinish)
	{
        mEventProgress = _eventProgress;
        LoadAdditiveScene(_addressableScene, 1, _eventFinish);
	}

    public void CloseAdditive(string _addressableScene, UnityAction<string> _eventFinish)
	{
        UnloadAdditiveScene(_addressableScene, _eventFinish);
	}

    public string GetCurrentSceneName()
	{
        return pMainSceneName;
	}

    //----------------------------------------------------------------------
    public void ImportWorldDescription(UnityAction _eventFinish)
	{
        Addressables.InstantiateAsync(WorldDescription).Completed += (AsyncOperationHandle<GameObject> _GameObject) =>
        {
            _GameObject.Result.transform.SetParent(gameObject.transform);
            CSeamlessWorldDescription WorldDescription = _GameObject.Result.GetComponent<CSeamlessWorldDescription>();
            if (WorldDescription != null)
            {
                SetWorldDescription(WorldDescription);
            }
            _eventFinish?.Invoke();
        };
    }
}