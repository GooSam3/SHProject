using System;
using System.Collections.Generic;
// CManagerPatchDownloaderBase 
// ZeroGames : 정구삼 
// [개요] 패치를 위한 다운로드 기능 추상화 클래스 
// 1) Addressable 기반의 에셋번들 다운로드 기능을 구현하였다.
// 2) WebRequest 기반의 일반 파일 다운로드 시스템
// 3) 일반 파일의 경우 Caching 플레그를 통해 버퍼 다운로드를 제공한다.
//    버퍼의 경우 파일 출력을 하지 않는다.
// 4) 한번 URL을 초기화 하면 이후로는 파일 경로와 이름으로 쉽게 작동이 가능하다. 
// [주의!] AddressableSetting에 MaxConcurrentWebRequest를 500에서 1~10정도로 변경한 후 에셋번들 빌드를 해야한다. 한꺼번에 리퀘스트가 몰리면 딜레이가 상당히 걸리게된다.
abstract public class CManagerPatchDownloaderBase : CManagerTemplateBase<CManagerPatchDownloaderBase>
{
    public static string DownloadURL = "None";
    private CPatcherWebRequest mPatcherWebRequest = new CPatcherWebRequest();
    private CPatcherAddressable mPatcherAddressable = new CPatcherAddressable();

    private bool mReadyToPatch = false;
    //-----------------------------------------------------
    protected virtual void Update()
    {
        mPatcherWebRequest.UpdatePatch(0);
        mPatcherAddressable.UpdatePatch(0);
    }

	protected override void OnUnityAwake()
	{
        base.OnUnityAwake();
 	}

	//-----------------------------------------------------
	protected CPatcherBase.SPatchEvent ProtPatchAssetBundleInitialize(string PatchURL, bool _resetEventHandler)
    {
        CPatcherBase.SPatchEvent Handler = mPatcherAddressable.DoPatchResetHandler(_resetEventHandler);

        if (_resetEventHandler == false)
		{
            Handler.EventPatchInitComplete += HandlePatcherInitComplete;
            Handler.EventPatchDownloadSize += HandlePatcherDownloadSize;
            Handler.EventPatchEnd += HandlePatcherPatchEnd;
            Handler.EventPatchProgress += HandlePatcherProgress;
            Handler.EventPatchError += HandlePatcherError;
            Handler.EventPatchDownloadFinish += HandlePatcherDownloadFinish;
            Handler.EventPatchLabelStart += HandlePatcherStartLabel;
        }

        DownloadURL = PatchURL;
        mPatcherAddressable.InitializeDownloader(DownloadURL);
        return Handler;
    }

    protected void ProtPatchWebRequestInitialize(string PatchURL)
    {       
        mPatcherWebRequest.InitializeDownloader(PatchURL);
    }

    protected void ProtPatchAddressableStart(List<string> _listAssetBundleLable)
    {
        mPatcherAddressable.DoPatcherAddressableStart(_listAssetBundleLable);
    }

    protected void ProtPatchAddressableDowloadSize(List<string> _listAssetBundleLable, UnityEngine.Events.UnityAction<long> _eventFinish)
	{
        mPatcherAddressable.DoPatcherAddressableDowloadSize(_listAssetBundleLable, _eventFinish);
	}

    protected CPatcherBase.SPatchEvent ProtPatchWebRequestStartFile(string _PathFileName, bool _ResetEventHandler = false, bool _AllocBuffer = true)
    {
        mPatcherWebRequest.DoPatcherDownloadFile(_PathFileName, true, _AllocBuffer);
        return mPatcherWebRequest.DoPatchResetHandler(_ResetEventHandler);
    }

    protected CPatcherBase.SPatchEvent ProtPatchWebRequestStartBuffer(string _PathFileName, bool resetEventHandler = false)
    {
        mPatcherWebRequest.DoPatcherDownloadFile(_PathFileName, false, false);
        return mPatcherWebRequest.DoPatchResetHandler(resetEventHandler);
    }

    protected void ProtPatchWebRequestDeleteFile(string _DeleteFileName, bool DeleteAllPathFile = false)
    {   
        // 삭제 함수는 동기(블로킹) 이다. 
        mPatcherWebRequest.DoPatcherDeleteFile(_DeleteFileName, DeleteAllPathFile);
    }

    protected void ProtPatchWebRequestMakeFile(string _PathFileName, string _TextData)
    {
        mPatcherWebRequest.DoPatcherMakeFile(_PathFileName, _TextData);
    }

    protected void ProtPatchWebRequestMakeFile(string _PathFileName, byte[] _FileData)
    {
        mPatcherWebRequest.DoPatcherMakeFile(_PathFileName, _FileData);
    }

    protected bool CheckPatcherFileExistInCacheFolder(string _PathFileName)
    {
        return mPatcherWebRequest.CheckPatcherFileExistInCacheFolder(_PathFileName);
    }

    protected void ProtPatcherWebRequestEventPatchEndHandler(Action<string, byte []> _EventPatchEnd)
    {
        mPatcherWebRequest.ChangeEventPatchEnd(_EventPatchEnd);
    }

    protected string GetPatcherCachingPath()
    {
        return mPatcherWebRequest.GetPatcherCachingPath();
    }

    //----------------------------------------------------
    private void HandlePatcherInitComplete()
    {       
        OnPatcherInitComplete();
    }

    private void HandlePatcherDownloadSize(string PatchName, long Size)
    {
        OnPatcherDownloadSize(PatchName, Size);
    }

    private void HandlePatcherProgress(string PatchName, long _downloadedByte, long _totalByte, float ProgressPercent, uint _loadCurrent, uint _loadMax)
    {
        OnPatcherProgress(PatchName, _downloadedByte, _totalByte,  ProgressPercent, _loadCurrent, _loadMax);
    }

    private void HandlePatcherPatchEnd(string PatchName, byte [] Data)
    {  
        OnPatcherEnd(PatchName);
    }

    private void HandlePatcherError(CPatcherBase.E_PatchError _errorType, string _message)
    {
        OnPatcherError(_errorType, _message);
    }

    private void HandlePatcherDownloadFinish()
	{      
        OnPatcherDownloadFinish();
	}

    private void HandlePatcherStartLabel(string _labelName)
	{
        OnPatcherStartLabel(_labelName);
    }

    //---------------------------------------------------------------------------
    protected virtual void OnPatcherInitComplete() { }
    protected virtual void OnPatcherDownloadSize(string PatchName, long Size) { }
    protected virtual void OnPatcherProgress(string PatchName, long _downloadedByte, long _totalByte, float ProgressPercent, uint _loadCurrent, uint _loadMax) { }
    protected virtual void OnPatcherEnd(string PatchName) { }
    protected virtual void OnPatcherDownloadFinish() { }
    protected virtual void OnPatcherStartLabel(string _labelName) { }
    protected virtual void OnPatcherError(CPatcherBase.E_PatchError _errorType, string _message) { }

}
