using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

// 최소한의 비용으로 파일 업데이트가 가능하도록 리스트 갱신 및 서버 리스트 대조
// 중간에 작업이 완료 되지 않았을 때 다음 실행시 복구 및 갱신이 가능한 구조 

public partial class ZManagerPatchDownloader : CManagerPatchDownloaderBase
{
    [Serializable]
    private class STableFileHeader
    {
        public Dictionary<string, STableFileItem> fileDic = new Dictionary<string, STableFileItem>();
        public Dictionary<string, STableFileItem> fileSubInfoDic = new Dictionary<string, STableFileItem>();
    }
    [Serializable]
    private class STableFileItem // Json 항목의 이름과 동일하게 
    {
        public string FileName;
        public string EncryptionKey;
        public long   FileSize;
        public STableFileItem() { }
        public STableFileItem(string _FileName, string _EncrptionKey, long _FileSize) { FileName = _FileName; EncryptionKey = _EncrptionKey; FileSize = _FileSize; }
    }
    [SerializeField]
    private string TableFileServerName = "filelist.json";
    [SerializeField]
    private string TableFileClientName = "filelist_client.json";
    [SerializeField]
    private string TableFileRoot = "bin";

    private Queue<STableFileItem> m_queRequireDownloadFileItem = new Queue<STableFileItem>();

    private STableFileHeader        mTableFileHeaderClient = null;
    private STableFileHeader        mTableFileHeaderServer = null;
    private STableFileHeader        mTableFileHeaderClientUpdate = new STableFileHeader();

    private bool             mWorkStart = false;
    private string           mTableFileServerStock;
    private StringBuilder     mPathNameNote = new StringBuilder();

    private Action<string, int, int, float>              mEventProgress;
    private Action                                     mEventFinish;
    private Action<CPatcherBase.E_PatchError, string>      mEventError;
    private Action<string>                              mEventMessage;
    //------------------------------------------------------------------
    public void DoPatcherStartTableFile(Action<string, int, int, float> _EventProgress, Action _EventFinish, Action<CPatcherBase.E_PatchError, string> _EventError, Action<string> _EventMessage)
    {
        if (mWorkStart)
        {
            HandlePatcherTableError(CPatcherBase.E_PatchError.AlreadyPatchProcess, null);
            return;
        }
        mWorkStart = true;
        mEventFinish = _EventFinish;
        mEventError = _EventError;
        mEventMessage = _EventMessage;
        mEventProgress = _EventProgress;
        PrivPatcherTableFileWork();
    }

    public string GetPatcherTableFilePath()
    {
        return $"{GetPatcherCachingPath()}/{TableFileRoot}";
    }

    //---------------------------------------------------------------------
    private void PrivPatcherTableFileWork()
    {
        // 로컬 테이블 파일을 확인 
        string PathFileName = ExtractTableFilePathName(TableFileClientName);
        if (CheckPatcherFileExistInCacheFolder(PathFileName))
        {
            PrivPatcherTableFileHeaderClient(PathFileName);
        }
        else // 없으면 생성후 다운로드 
        {           
            PrivPatcherTableFileHeaderClientNew(PathFileName);
        }
    }

    private void PrivPatcherTableFileHeaderClient(string _PathFileName)
    {
        CPatcherBase.SPatchEvent EventHandler = ProtPatchWebRequestStartFile(_PathFileName, true, true);
        EventHandler.EventPatchDownloadSize += HandlePatcherTableDownloadSize;
        EventHandler.EventPatchEnd += HandlePatcherTableFileClient;
        EventHandler.EventPatchError += HandlePatcherTableError;
    }

    private void PrivPatcherTableFileHeaderServer()
    {
        string PathFileName = ExtractTableFilePathName(TableFileServerName);
        CPatcherBase.SPatchEvent EventHandler = ProtPatchWebRequestStartBuffer(PathFileName, true);
        EventHandler.EventPatchDownloadSize += HandlePatcherTableDownloadSize;
        EventHandler.EventPatchEnd += HandlePatcherTableFileServer;
        EventHandler.EventPatchError += HandlePatcherTableError;
        EventHandler.EventPatchProgress += HandlePatcherTableProgress;
    }

    private void PrivPatcherTableFileDownload(string _PathFileName)
    {
        ProtPatcherWebRequestEventPatchEndHandler(HadlePatcherTableDownload);
        ProtPatchWebRequestStartBuffer(_PathFileName, false);
    }

    private void PrivPatcherTableFileProgress(string _FileName, int _CurrentInfoIndex, float _Progress)
    {
        mEventProgress?.Invoke(_FileName, _CurrentInfoIndex, mTableFileHeaderServer.fileDic.Count, _Progress);
    }
    //-------------------------------------------------------------------------

    private void PrivPatcherTableFileHeaderClientNew(string _PathFileName)
    {
        mTableFileHeaderClient = new STableFileHeader(); // 클라에 파일이 없으므로 모든 파일을 갱신
        PrivPatcherTableFileHeaderServer();
    }

    private void PrivPatcherTableFileHeaderReadClient(string _PathFileName, byte [] _FileData)
    {
        // 클라이언트 로컬 파일을 확보 
        string TableText = Encoding.Default.GetString(_FileData);
        TinyJSON.JSON.MakeInto(TinyJSON.JSON.Load(TableText), out mTableFileHeaderClient);
        // 서버 테이블 파일 요청 
        PrivPatcherTableFileHeaderServer();
    }

    private void PrivPatcherTableFileHeaderReadServer(byte [] _FileData)
    {
        mTableFileServerStock = Encoding.Default.GetString(_FileData);
        TinyJSON.JSON.MakeInto(TinyJSON.JSON.Load(mTableFileServerStock), out mTableFileHeaderServer);

        PrivPatcherTableFileWorkUpdate();
    }

    //---------------------------------------------------------------------
    private void PrivPatcherTableFileVerificationByMD5(string _FileName, string _FileHashString, byte [] _FileData)
    {
        MD5 md5Instance = MD5.Create();
        StringBuilder Note = new StringBuilder(32);
        byte[] md5Hash = md5Instance.ComputeHash(_FileData);

        for (int i = 0; i < md5Hash.Length; i++)
        {
            Note.Append(md5Hash[i].ToString("x2"));
        }

        string md5HashString = Note.ToString();

        if (md5HashString != _FileHashString)
        {    
            // Error!  File was Not Match 
        }
        mTableFileHeaderClientUpdate.fileDic.Add(_FileName, new STableFileItem(_FileName, md5HashString, _FileData.Length));
        PrivPatcherTableFileProgress(_FileName, mTableFileHeaderClientUpdate.fileDic.Count, 1f);
        ProtPatchWebRequestMakeFile(ExtractTableFilePathName(_FileName), _FileData);
        PrivPatcherTableFileItemRequestDownload();
    }

    //------------------------------------------------------------------------------
    private void PrivPatcherTableFileWorkUpdate()
    {
        // 서버와 일치하는 파일 리스트 
        List<STableFileItem> listReserveFileItem = new List<STableFileItem>();
        // 다운로드 파일 리스트 작성
        Dictionary<string, STableFileItem>.Enumerator it = mTableFileHeaderServer.fileDic.GetEnumerator();
        while(it.MoveNext())
        {
            string ServerFileName = it.Current.Key;
            STableFileItem ServerFileInfo = it.Current.Value;
            if (mTableFileHeaderClient.fileDic.ContainsKey(ServerFileName))
            {
                STableFileItem ClientFileInfo = mTableFileHeaderClient.fileDic[ServerFileName];
                if (ServerFileInfo.EncryptionKey != ClientFileInfo.EncryptionKey) // 헤쉬가 다르면 다운로드 대상
                {
                    m_queRequireDownloadFileItem.Enqueue(ServerFileInfo);
                }
                else // 파일이 존재 하지 않을 경우에도 다운로드  
                {
                    string PathFileName = ExtractTableFilePathName(ServerFileInfo.FileName);
                    if (CheckPatcherFileExistInCacheFolder(PathFileName))
                    {
                        listReserveFileItem.Add(ServerFileInfo);
                    }
                    else
                    {
                        m_queRequireDownloadFileItem.Enqueue(ServerFileInfo);
                    }
                }
                mTableFileHeaderClient.fileDic.Remove(ServerFileName); 
            }
            else // 없으면 다운로드 대상 
            {
                m_queRequireDownloadFileItem.Enqueue(ServerFileInfo);
            }
        }
        // 클라에 남은 모든 파일은  폐기 파일 
        List<STableFileItem> listDiscardFile = new List<STableFileItem>();
        it = mTableFileHeaderClient.fileDic.GetEnumerator();
        while(it.MoveNext())
        {
            listDiscardFile.Add(it.Current.Value);
        }

        PrivPatcherTableDiscardFile(listDiscardFile);
        PrivPatcherTableReserveFileList(listReserveFileItem);
        PrivPatcherTableFileItemRequestDownload();
    }

    private void PrivPatcherTableDiscardFile(List<STableFileItem> _ListDiscardFile)
    {
        for (int i = 0; i < _ListDiscardFile.Count; i++)
        {
            string DeletePathName = ExtractTableFilePathName(_ListDiscardFile[i].FileName);
            ProtPatchWebRequestDeleteFile(DeletePathName);
        }
    }

    private void PrivPatcherTableReserveFileList(List<STableFileItem> _ReserveFileItem)
    {
        mTableFileHeaderClientUpdate.fileDic.Clear();
        for (int i = 0; i < _ReserveFileItem.Count; i++)
        {
            mTableFileHeaderClientUpdate.fileDic[_ReserveFileItem[i].FileName] =  _ReserveFileItem[i];
            PrivPatcherTableFileProgress(_ReserveFileItem[i].FileName, i, 1f);
        }

     //   Debug.Log(string.Format("[TableFilePatcher] Client Reserve File Item : {0} =========================", mTableFileHeaderClientUpdate.fileDic.Count));
    }

    private void PrivPatcherTableFileItemRequestDownload()
    {
        if (m_queRequireDownloadFileItem.Count > 0)
        {
            STableFileItem TableItem = m_queRequireDownloadFileItem.Dequeue();
            string PathFileName = ExtractTableFilePathName(TableItem.FileName);
            PrivPatcherTableFileDownload(PathFileName);
        }
        else
        {
            PrivPatcherTableWorkFinish();
        }
    }
  
    private void PrivPatcherTableWorkFinish()
    {
        mWorkStart = false;
        mEventMessage?.Invoke("[TableFilePatcher] Finish Work");
        mEventFinish?.Invoke();
        PrivPatcherTableSaveClient();
        PrivPatcherTableClearAll();     
    }

    private void PrivPatcherTableSaveClient()
    {
        string JsonText = JsonConvert.SerializeObject(mTableFileHeaderClientUpdate, Formatting.Indented);
        ProtPatchWebRequestMakeFile(ExtractTableFilePathName(TableFileClientName), JsonText);
    }

    private void PrivPatcherTableClearAll()
    {
        mTableFileServerStock = null;
        mTableFileHeaderClient = null;
        mTableFileHeaderServer = null;
        mEventError = null;
        mEventFinish = null;
        mEventProgress = null;
        mEventMessage = null;

        mPathNameNote.Clear();
        m_queRequireDownloadFileItem.Clear();
    }


    //---------------------------------------------------------------------
    private void HandlePatcherTableDownloadSize(string _FileName, long _Size)
    {
    //    Debug.Log(string.Format("[TableFilePatcher] DowmloadSize {0} : {1}", _FileName, _Size));
    }

    private void HandlePatcherTableFileClient(string _FileName, byte [] _FileData)
    {
        PrivPatcherTableFileHeaderReadClient(_FileName, _FileData);
    //    Debug.Log(string.Format("[TableFilePatcher] TableClient {0} =========================", _FileName));
    }

    private void HandlePatcherTableFileServer(string _FileName, byte[] _FileData)
    {
        PrivPatcherTableFileHeaderReadServer(_FileData);
    //    Debug.Log(string.Format("[TableFilePatcher] TableServer {0} =========================", _FileName));
    }

    private void HadlePatcherTableDownload(string _FileName, byte[] _FileData)
    {
        string workFileName = Path.GetFileName(_FileName);
   //     Debug.Log(string.Format("[TableFilePatcher] File Download {0} ========================", _FileName));
        PrivPatcherTableFileVerificationByMD5(workFileName, FindHashTableFileServerHeader(workFileName), _FileData);
    }

    private void HandlePatcherTableError(CPatcherBase.E_PatchError _errorType, string _message = null)
    {
        mWorkStart = false;
        mEventError?.Invoke(_errorType, _message);
        ZLog.LogError(ZLogChannel.System, string.Format("[TableFilePatcher] Patch Error : {0} ", _errorType.ToString()));
    }

    private void HandlePatcherTableProgress(string _FileName, long _downloadedByte, long _totalByte,  float _Progress, uint _loadCurrent, uint _loadMax)
    {
        if (mTableFileHeaderServer == null) return;

        int CountTotal = mTableFileHeaderServer.fileDic.Count;
        int CountCurrent = mTableFileHeaderClientUpdate.fileDic.Count;
        string FileName = Path.GetFileName(_FileName);
        mEventProgress?.Invoke(FileName, CountCurrent, CountTotal, _Progress);
    }

    //--------------------------------------------------------------------
    private string ExtractTableFilePathName(string _FileName)
    {
        mPathNameNote.Clear();
        mPathNameNote.AppendFormat("{0}/{1}", TableFileRoot, _FileName);
        return mPathNameNote.ToString();
    }

    private string FindHashTableFileServerHeader(string _FileName)
    {
        string Result = "";

        if (mTableFileHeaderServer.fileDic.ContainsKey(_FileName))
        {
            Result = mTableFileHeaderServer.fileDic[_FileName].EncryptionKey;
        }
        return Result;
    }
}
