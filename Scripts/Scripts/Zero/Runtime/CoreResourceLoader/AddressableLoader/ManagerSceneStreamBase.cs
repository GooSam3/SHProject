using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceProviders;

public abstract class ManagerSceneStreamBase : ManagerSceneLoaderBase<ManagerSceneStreamBase>
{ public static new ManagerSceneStreamBase Instance { get { return ManagerSceneLoaderBase<ManagerSceneStreamBase>.Instance as ManagerSceneStreamBase; } }

    private CSeamlessWorldDescription mWorldDescription = null;

    public void ImportVisit(CSeamlessStageAttacherBase _VisitNode)
    {
        
    }

    //---------------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        SetWorldDescription(GetComponentInChildren<CSeamlessWorldDescription>());
    }

    protected override void OnSceneFinishMain(string _addressableName, SceneInstance _mainScene)
	{
        //FindOpenStage(_mainScene.Scene.GetRootGameObjects());

        //TODO :: StageAttacher 사용하지 않음.
        RefreshLoadStage(_addressableName);
    }

    protected override void OnSceneAdditiveLoad(string _addressableName, SceneInstance _mainScene) 
    {
        _mainScene.ActivateAsync().priority = 1;
    }
    protected override void OnSceneAdditiveUnload(string _addressableName, SceneInstance _mainScene) 
    {
        
    }


	//---------------------------------------------------------------------------
	protected void SetWorldDescription(CSeamlessWorldDescription _WorldDescription)
    {
        if (_WorldDescription != null)
		{
            mWorldDescription = _WorldDescription;
        }
    }

    //---------------------------------------------------------------------------
    private void FindOpenStage(GameObject[] _rootGameObject)
	{
        for (int i = 0; i < _rootGameObject.Length; i++)
        {
            CSeamlessStageAttacherBase Attacher = _rootGameObject[i].GetComponentInChildren<CSeamlessStageAttacherBase>();
            if (Attacher != null)
            {
                OpenStage(Attacher);
                break;
            }
        }
    }

    private void OpenStage(CSeamlessStageAttacherBase _attacher)
    {
        _attacher.DoSeamlessStageInitialize();
        if (mWorldDescription != null)
        {
            List<CSeamlessStageBase> listStageLoaded = new List<CSeamlessStageBase>();
            mWorldDescription.DoWorldDescriptionStageOpen(_attacher.pWorldID, _attacher.pStageID, listStageLoaded);
            RefreshLoadStage(listStageLoaded);
        }
    }

    /// <summary> </summary>
    private void RefreshLoadStage(string _mainSceneName)
    {
        //TODO :: 일단 규칙대로 서브씬하나만 로드 
        if(false == _mainSceneName.Contains("_Main"))
        {
            return;
        }

        StringBuilder builder = new StringBuilder(_mainSceneName);
        
        builder.Replace("_Main", "");
        builder.Append("_Sub1");

        OnSceneOpenRootStage(builder.ToString());
        LoadAdditiveScene(builder.ToString(), 1, (string _loadedSceneName) => {
            OnSceneFinishRootStage(_loadedSceneName);
        });

        //var it = Addressables.ResourceLocators.GetEnumerator();

        //while(it.MoveNext())
        //{            
        //    foreach (var key in it.Current.Keys)
        //    {
        //        if (key.ToString() != additiveSceneName)
        //            continue;

        //        OnSceneOpenRootStage(additiveSceneName);
        //        LoadAdditiveScene(additiveSceneName, 1, (string _loadedSceneName) => {
        //            OnSceneFinishRootStage(_loadedSceneName);
        //        });
        //    }
        //}
    }


    private void RefreshLoadStage(List<CSeamlessStageBase> _listStageRequest)
    {
        List<CSeamlessStageBase> listRequireScene = new List<CSeamlessStageBase>();
        HashSet<string> setLoadedScene = new HashSet<string>();
        ExtractAdditiveScene(setLoadedScene);

        // 기존 로드된 에디티브 씬과 비교한다.        
        for (int i = 0; i < _listStageRequest.Count; i++)
        {
            string SceneName = _listStageRequest[i].pStageName;

            if (setLoadedScene.Contains(SceneName))
            {
                setLoadedScene.Remove(SceneName);
            }
            else
            {
                listRequireScene.Add(_listStageRequest[i]);
            }
        }
        // 로드 리스트에 남아있지 않는 씬은 제거 
        HashSet<string>.Enumerator it = setLoadedScene.GetEnumerator();
        while (it.MoveNext())
        {
            UnloadAdditiveScene(it.Current, null);
        }

        // 최초 스테이지 로드 : 임시코드 
        int Index = 0;
        string PrimeStage = listRequireScene[Index].pStageName;
        OnSceneOpenRootStage(PrimeStage);
        LoadAdditiveScene(PrimeStage, 1, (string _loadedSceneName) => {
            OnSceneFinishRootStage(_loadedSceneName);
        });
        
        // 나머지 스테이지 로드 
        for (int i = 0; i < listRequireScene.Count; i++)
        {
            if (i != Index)
			{
                LoadAdditiveScene(listRequireScene[i].pStageName, 1, null);
            }
        }
    }
    //-----------------------------------------------------------
    protected virtual void OnSceneOpenRootStage(string _loadedSceneName) { }
    protected virtual void OnSceneFinishRootStage(string _loadedSceneName) { }
}
