using System;
using System.Text;

abstract public class CPatcherBase : CObjectInstanceBase
{
    public enum E_PatchError
	{       
        NotInitialized,
        AlreadyPatchProcess,
        NotEnoughDiskSpace,
        NetworkDisable,
        CatalogUpdateFail,
        PatchFail,
        HTTPError,
	}

    public class SPatchEvent
    {
        public event Action                                         EventPatchInitComplete = null;
        public event Action<string, long>                             EventPatchDownloadSize = null;
        public event Action<string, long, long, float, uint, uint>      EventPatchProgress = null;
        public event Action<string, byte[]>                           EventPatchEnd = null;
        public event Action<E_PatchError, string>                      EventPatchError = null;
        public event Action                                         EventPatchDownloadFinish = null;
        public event Action<string>                                  EventPatchLabelStart = null;

        protected void ProtReset() { EventPatchInitComplete = null; EventPatchDownloadSize = null; EventPatchProgress = null; EventPatchError = null; EventPatchEnd = null; }
        protected void ProtInitComplete(){ EventPatchInitComplete?.Invoke();}
        protected void ProtDownloadSize(string Name, long Size) { EventPatchDownloadSize?.Invoke(Name, Size);}
        protected void ProtProgress(string Name, long _downloadedByte, long _totalByte, float Progress, uint _loadCurrent, uint _loadMax) { EventPatchProgress?.Invoke(Name, _downloadedByte, _totalByte, Progress, _loadCurrent, _loadMax);}
        protected void ProtEnd(string Name, byte[]ByteBuffer) { EventPatchEnd?.Invoke(Name, ByteBuffer);}
        protected void ProtError(E_PatchError _errorType, string _message) { EventPatchError?.Invoke(_errorType, _message); }
        protected void ProtChangeEnd(Action<string, byte[]> _EventhEnd) { EventPatchEnd = null; EventPatchEnd += _EventhEnd; }
        protected void ProtDownloadFinish() { EventPatchDownloadFinish?.Invoke(); }
        protected void ProtLabelStart(string Name) { EventPatchLabelStart?.Invoke(Name); }
    }

    private class SPatchEventHandle : SPatchEvent
    {
        public void DoReset() { ProtReset(); }
        public void DoInitComplete() { ProtInitComplete(); }
        public void DoDownloadSize(string Name, long Size) { ProtDownloadSize(Name, Size); }
        public void DoProgress(string Name, long _downloadedByte, long _totalByte, float Progress, uint _loadCurrent, uint _loadMax) { ProtProgress(Name, _downloadedByte, _totalByte, Progress, _loadCurrent, _loadMax); }
        public void DoEnd(string Name, byte[] ByteBuffer) { ProtEnd(Name, ByteBuffer); }
        public void DoError(E_PatchError _errorType, string _message) { ProtError(_errorType, _message); }
        public void DoChangeEnd(Action<string, byte[]> _EventhEnd) { ProtChangeEnd(_EventhEnd); }
        public void DoDownloadFinish() { ProtDownloadFinish(); }
        public void DoLabelStart(string Name) { ProtLabelStart(Name); }
    };
 
    private string                  mDownLoadURL;   protected string DownloadURL { get { return mDownLoadURL; } }
    private StringBuilder            mPathNote = new StringBuilder();
    private SPatchEventHandle        mEventHandler = new SPatchEventHandle();
    //---------------------------------------------------------
    public void InitializeDownloader(string downloadURL)
    {
        mDownLoadURL = downloadURL;
        mPathNote.Capacity = 256;
        OnPatcherInitialize();
    }

    public void UpdatePatch(float DeltaTime)
    {
        OnPatcherUpdate(DeltaTime);
    }

    public SPatchEvent DoPatchResetHandler(bool resetHandler)
    {
        if (resetHandler)
        {
            mEventHandler.DoReset();
        }
        return mEventHandler;
    }

    public void ChangeEventPatchEnd(Action<string, byte[]> _EventPatchEnd)
    {
        mEventHandler.DoChangeEnd(_EventPatchEnd);
    }


    //-----------------------------------------------------------
    protected void ProtPatchInitComplete()
    {
        mEventHandler.DoInitComplete();
    }

    protected void ProtPatchDownloadSize(string PatchName, long Size)
    {
        mEventHandler.DoDownloadSize(PatchName, Size);
    }

    protected void ProtPatchProgress(string _patchName, long _downloadedByte, long _totalByte, float _percent, uint _loadCurrent, uint _loadMax)
    {
        mEventHandler.DoProgress(_patchName, _downloadedByte, _totalByte, _percent, _loadCurrent, _loadMax);
    }

    protected void ProtPatchError(E_PatchError _errorType, string _message = null)
    {
        mEventHandler.DoError(_errorType, _message);
    }

    protected void ProtPatchFinish(string PatchName, byte [] Data)
    {
        mEventHandler.DoEnd(PatchName, Data);
    }

    protected void ProtPatchDownloadFinish()
	{
        mEventHandler.DoDownloadFinish();
	}

    protected void ProtPatchLabelStart(string _labelName)
	{
        mEventHandler.DoLabelStart(_labelName);
	}

    //----------------------------------------------------------
    protected virtual void OnPatcherInitialize() { }
    protected virtual void OnPatcherUpdate(float DeltaTime) { }
}
