using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SHManagerSceneLoader : CManagerSceneLoaderBase<SHManagerSceneLoader>
{   public static new SHManagerSceneLoader Instance { get { return CManagerSceneLoaderBase<SHManagerSceneLoader>.Instance as SHManagerSceneLoader; } }

    private UnityAction<float>      mEventProgress = null;
    private UnityAction<string>     mEventFinish = null;
    private string                 mLoadingSceneName;
  	//--------------------------------------------------------------------
	protected override void OnSceneFinishMain(string _addressableName, Scene _mainScene)
	{
        base.OnSceneFinishMain(_addressableName, _mainScene);
        Debug.Log("[SceneLoader] =============Finish Loading :" + _addressableName);
        mEventFinish?.Invoke(_addressableName);
    }
    
    protected override void OnSceneProgress(string _addressableName, float _progress)
	{
        base.OnSceneProgress(_addressableName, _progress);
        mEventProgress?.Invoke(_progress);
    }

	protected override void OnAddressableError(string _addressableName, string _Error)
	{
        Debug.LogError(string.Format("[Addressable Map]{0} : {1}", _addressableName, _Error));
    }
	//---------------------------------------------------------------------
	public void DoOpenScenMain(string _addressableScene, UnityAction<float> _eventProgress, UnityAction<string> _eventFinish, bool bForceDelete = false)
	{      
        mEventFinish = _eventFinish;
        mEventProgress = _eventProgress;
        mLoadingSceneName = _addressableScene;

        LoadMainScene(_addressableScene, bForceDelete);
    }

    public void DoOpenSceneAdditive(string _addressableScene, UnityAction<float> _eventProgress, UnityAction<string> _eventFinish)
	{
        mEventProgress = _eventProgress;
        LoadAdditiveScene(_addressableScene, _eventFinish);
	}

    public void DoCloseSceneAdditive(string _addressableScene, UnityAction<string> _eventFinish)
	{
        UnloadAdditiveScene(_addressableScene, _eventFinish);
	}

    public string GetCurrentSceneName()
	{
        return pMainSceneName;
	}

    //----------------------------------------------------------------------
   
}