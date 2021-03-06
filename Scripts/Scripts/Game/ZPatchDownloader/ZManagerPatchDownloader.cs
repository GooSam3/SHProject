using System;
using System.Collections.Generic;
using UnityEngine;

public partial class ZManagerPatchDownloader : CManagerPatchDownloaderBase
{   public static new ZManagerPatchDownloader Instance { get { return CManagerPatchDownloaderBase.Instance as ZManagerPatchDownloader; } }
    
    [SerializeField]
    private string PatchRootURL = "http://10.95.162.57:81/icarus/dev1_data";

    [SerializeField]
    private List<string> PatchLable = new List<string>();

    //-------------------------------------------------------------
    public CPatcherBase.SPatchEvent InitializePatcherURL(string _PatchRootURL, bool _resetEventHandler)  // 로그인 서버등에서 받는 다운로드 URL 입력. 입력 안하면 인스펙터 URL을 사용.
    {
        return PrivPatcherInit(_PatchRootURL, _resetEventHandler);
    }
    
    public void DoPatcherDownLoadSize(UnityEngine.Events.UnityAction<long> _eventFinish)
	{
        ProtPatchAddressableDowloadSize(PatchLable, _eventFinish);
	}

    public void DoPatcherStartAssetBundle()
    {
        ProtPatchAddressableStart(PatchLable);
    }

    public string ExtractLocalText(CPatcherBase.E_PatchError _errorType, string _message)
	{
        string localText = null;
        if (_errorType == CPatcherBase.E_PatchError.AlreadyPatchProcess)
        {
            localText = DBLocale.GetTextSimple("Patch_Already_Process");
        }
        else if (_errorType == CPatcherBase.E_PatchError.CatalogUpdateFail)
        {
            localText = DBLocale.GetTextSimple("Patch_Catalog_Fail");
        }
        else if (_errorType == CPatcherBase.E_PatchError.HTTPError)
        {
            localText = DBLocale.GetTextSimple("Patch_HTTP_Error");
        }
        else if (_errorType == CPatcherBase.E_PatchError.NetworkDisable)
        {
            localText = DBLocale.GetTextSimple("Patch_Network_Error");
        }
        else if (_errorType == CPatcherBase.E_PatchError.NotEnoughDiskSpace)
        {
            localText = DBLocale.GetTextSimple("Patch_Not_Enough_DiskSpace");
        }
        else if (_errorType == CPatcherBase.E_PatchError.NotInitialized)
        {
            localText = DBLocale.GetTextSimple("Patch_Not_Initialized");
        }
        else if (_errorType == CPatcherBase.E_PatchError.PatchFail)
        {
            localText = DBLocale.GetTextSimple("Patch_Fail");
        }
        return localText;
	}

    //--------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
    }

    protected override void OnUnityStart()
    {
        base.OnUnityStart();
    }

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
        string bundleURL = string.Empty;

#if (UNITY_EDITOR && UNITY_ANDROID)
        bundleURL = string.Format("bundle/{0}", "android");
#elif (UNITY_EDITOR && UNITY_IOS)
         bundleURL = string.Format("bundle/{0}", "ios");        
#elif (UNITY_ANDROID)
         bundleURL = string.Format("bundle/{0}", "android");
#elif (UNITY_IOS)       
         bundleURL = string.Format("bundle/{0}", "ios");
#endif      
        string patchPath = System.IO.Path.Combine(_baseURL, bundleURL);
        ZLog.Log(ZLogChannel.System, $"ZManagerPatchDownloader.BundleURL : {bundleURL}");
        
        ProtPatchWebRequestInitialize(PatchRootURL);
        return ProtPatchAssetBundleInitialize(patchPath, _resetEventHandler);
    }

    //--------------------------------------------------------------------------------

}
