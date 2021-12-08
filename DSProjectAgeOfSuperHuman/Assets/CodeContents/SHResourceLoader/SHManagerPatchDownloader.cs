using System;
using System.Collections.Generic;
using UnityEngine;

public partial class SHManagerPatchDownloader : CManagerPatchDownloaderBase
{   public static new SHManagerPatchDownloader Instance { get { return CManagerPatchDownloaderBase.Instance as SHManagerPatchDownloader; } }
    
    [SerializeField]
    private string PatchRootURL = "http://localhost/[BuildTarget]";

    [SerializeField]
    private List<string> PatchLable = new List<string>();

    //-------------------------------------------------------------
    public CPatcherBase.SPatchEvent InitializePatcherURL()  // 로그인 서버등에서 받는 다운로드 URL 입력. 입력 안하면 인스펙터 URL을 사용.
    {
        return PrivPatcherInit("https://firebasestorage.googleapis.com/v0/b/theeraofoverman.appspot.com/o/AssetBundle/", true);
    }
    
    public void DoPatcherDownLoadTotalSize(UnityEngine.Events.UnityAction<long> _eventFinish)
	{
        ProtPatchAddressableTotalDowloadSize(PatchLable, _eventFinish);
	}

    public void DoPatcherStartAssetBundle()
    {
        ProtPatchAddressableStart(PatchLable);
    }

    //--------------------------------------------------------------
	protected override void OnPatcherError(CPatcherBase.E_PatchError _errorType, string _message)
	{
		base.OnPatcherError(_errorType, _message);
	}

	//--------------------------------------------------------------
	protected sealed override void OnPatcherInitComplete()
    {      
        Debug.Log(string.Format("[AddressablePatcher]OnPatcherInitComplete"));
    }

    //--------------------------------------------------------------
    private CPatcherBase.SPatchEvent PrivPatcherInit(string _baseURL, bool _resetEventHandler)
    {
        PatchRootURL = _baseURL;
        string bundleURL = _baseURL;

//#if (UNITY_EDITOR && UNITY_ANDROID)
//        bundleURL = string.Format("bundle/{0}", "android");
//#elif (UNITY_EDITOR && UNITY_IOS)
//         bundleURL = string.Format("bundle/{0}", "ios");        
//#elif (UNITY_ANDROID)
//         bundleURL = string.Format("bundle/{0}", "android");
//#elif (UNITY_IOS)       
//         bundleURL = string.Format("bundle/{0}", "ios");
//#endif      
       // string patchPath = System.IO.Path.Combine(_baseURL, bundleURL);
        return ProtPatchAssetBundleInitialize(PatchRootURL, _resetEventHandler);
    }

    //--------------------------------------------------------------------------------

}
