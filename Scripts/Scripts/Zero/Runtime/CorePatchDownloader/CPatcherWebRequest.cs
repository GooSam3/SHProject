using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class CPatcherWebRequest : CPatcherBase
{
    private enum EFileReadMode
    {
        None,
        ReadFromCache,
        ReadFromURL,
    }

    private string      mCachePath;
    private string      mPathFileName;
    private byte[]      mFileDataBuffer = null;
    private int         mTotalReadSize = 0;
    private bool        mStartRequest = false;
    private bool        mAllocBuffer = false;

    private EFileReadMode                   mFileReadMode = EFileReadMode.None;
    private Task<int>                       mTaskFileRead;
    private UnityWebRequestAsyncOperation     mAsyncFileDownload = null;
    private FileStream                      mFileReadStream = null;
    
    //------------------------------------------------
    public void DoPatcherDownloadFile(string _PathFileName, bool _Caching, bool _AllocBuffer)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PrivPatcherError(E_PatchError.NetworkDisable, null);
            return;
        }


        if (mStartRequest)
        {
            PrivPatcherError(E_PatchError.AlreadyPatchProcess);
            return;
        }

        mStartRequest = true;
        mAllocBuffer = _AllocBuffer;
        mPathFileName = _PathFileName;
        if (_Caching)
        {
            PrivPatcherDownloadFileURLCaching(_PathFileName);
        }
        else
        {
            PrivPatcherDownloadFileURL(_PathFileName);
        }
    }

    public void DoPatcherDeleteFile(string _DeletePathFileName, bool _Recursive)
    {
        if (_Recursive)
        {
            PrivPatcherDeleteFileRecursive(_DeletePathFileName);
        }
        else
        {
            PrivPatcherDeleteFile(_DeletePathFileName);
        }
    }

    public void DoPatcherMakeFile(string _PathFileName, string _TextData)
    {
        string FullPathName = ExtractPatcherFilePath(mCachePath, _PathFileName);
        string DirectoryName = Path.GetDirectoryName(FullPathName);
        if (Directory.Exists(DirectoryName) == false)
        {
            Directory.CreateDirectory(DirectoryName);
        }

        File.WriteAllText(FullPathName, _TextData);
    }

    public void DoPatcherMakeFile(string _PathFileName, byte  [] _FileData)
    {
        string FullPathName = ExtractPatcherFilePath(mCachePath, _PathFileName);
        string DirectoryName = Path.GetDirectoryName(FullPathName);
        if (Directory.Exists(DirectoryName) == false)
        {
            Directory.CreateDirectory(DirectoryName);
        }
        File.WriteAllBytes(FullPathName, _FileData);
    }

    public bool CheckPatcherFileExistInCacheFolder(string _FileName)
    {
        string FilePath = ExtractPatcherFilePath(mCachePath, _FileName);
        return File.Exists(FilePath);
    }

    public string GetPatcherCachingPath()
    {
        PrivPatcherFindCachingDirectory();
        return mCachePath;
    }

    //-----------------------------------------------
    protected override void OnPatcherInitialize() 
    {
        PrivPatcherFindCachingDirectory();

        if (Directory.Exists(mCachePath) == false)
        {
            Directory.CreateDirectory(mCachePath);
        }
    }

    protected override void OnPatcherUpdate(float DeltaTime) 
    { 
        if (mFileReadMode == EFileReadMode.ReadFromCache)
        {
            UpdatePatcherLoadFromCache();
        }
        else if (mFileReadMode == EFileReadMode.ReadFromURL)
        {
            UpdatePatcherLoadFromURL();
        }
     
    }
    //--------------------------------------------
    private void UpdatePatcherLoadFromCache()
    {
        if (mTaskFileRead.IsCompleted)
        {
            PrivPatcherFinishWork(mFileDataBuffer);
        }
        else
        {
            int ReadByte = mTaskFileRead.Result;
            if (ReadByte > 0)
            {
                float Percent = ReadByte / mTotalReadSize;
                ProtPatchProgress(mPathFileName, (uint)ReadByte, (uint)mTotalReadSize, Percent, 0, 0);
            }
        }
    }

    private void UpdatePatcherLoadFromURL()
    {
        if (mAsyncFileDownload.webRequest.isNetworkError || mAsyncFileDownload.webRequest.isHttpError)
        {
            PrivPatcherError(E_PatchError.HTTPError);
        }
        else
        {
            if (mAsyncFileDownload.isDone)
            {
                PrivPatcherDownloadComplete();
            }
            else
            {
                ProtPatchProgress(mPathFileName, 0, 0, mAsyncFileDownload.progress, 0, 0);
            }
        }
    }

    //-------------------------------------------
    private void PrivPatcherDownloadFileURLCaching(string FileName)
    {
        string FilePath = ExtractPatcherFilePath(mCachePath, FileName);
        if (File.Exists(FilePath))
        {
            PrivPatcherLoadFromCacheFile(FilePath);
        }
        else
        {
            PrivPatcherLoadFromCachingURL(FileName); // 이 경우 케싱되지 않고 다운로드 되었다.
        }
    }

    private void PrivPatcherDownloadFileURL(string FileName)
    {
        mFileReadMode = EFileReadMode.ReadFromURL;
        string FileURL = ExtractPatcherFilePath(DownloadURL, FileName);

        UnityWebRequest pRequest = UnityWebRequest.Get(FileURL); // 기본 버퍼 다운로더가 된다.
        mAsyncFileDownload = pRequest.SendWebRequest();
    }

    private void PrivPatcherLoadFromCacheFile(string FilePath)
    {
        mFileReadMode = EFileReadMode.ReadFromCache;

        mFileReadStream = new FileStream(FilePath, FileMode.Open, FileAccess.Read);
        mTotalReadSize = (int)mFileReadStream.Length;
        mFileDataBuffer = new byte[mTotalReadSize];

        ProtPatchDownloadSize(mPathFileName, mTotalReadSize);

        mTaskFileRead = mFileReadStream.ReadAsync(mFileDataBuffer, 0, mTotalReadSize);        
    }

    private void PrivPatcherLoadFromCachingURL(string PathFileName)
    {
        mFileReadMode = EFileReadMode.ReadFromURL;
        string FileURL = ExtractPatcherFilePath(DownloadURL, PathFileName);
        string FileSavePath = ExtractPatcherFilePath(mCachePath, PathFileName);
        
        UnityWebRequest pRequest = UnityWebRequest.Get(FileURL);
        DownloadHandlerFile FileDownloader = new DownloadHandlerFile(FileSavePath);
        FileDownloader.removeFileOnAbort = true;
        pRequest.downloadHandler = FileDownloader;
        mAsyncFileDownload = pRequest.SendWebRequest();
    }

    private void PrivPatcherError(E_PatchError _errorType, string _message = null)
    {
        mStartRequest = false;
        mFileReadMode = EFileReadMode.None;
        PrivPatcherDeleteFile(mPathFileName);
        ProtPatchError(_errorType, _message);
    }

    private void PrivPatcherDownloadComplete()
    {
        DownloadHandlerFile HandlerFile = mAsyncFileDownload.webRequest.downloadHandler as DownloadHandlerFile;

        if (HandlerFile == null)
        {
            PrivPatcherFinishWork(mAsyncFileDownload.webRequest.downloadHandler.data);
        }
        else
        {
            if (mAllocBuffer)
            {
                string FileSavePath = ExtractPatcherFilePath(mCachePath, mPathFileName);
                PrivPatcherLoadFromCacheFile(FileSavePath);
            }
            else
            {
                PrivPatcherFinishWork(null);
            }
        }
    }

    //----------------------------------------------------------------------------------

    private void PrivPatcherDeleteFileRecursive(string _DeletePathFileName)
    {
        string deleteFileName = Path.GetFileName(_DeletePathFileName);
        RecursivePatcherDeleteFile(mCachePath, deleteFileName);
    }

    private void PrivPatcherDeleteFile(string _DeletePathFileName)
    {
        string deletePahtFileName = ExtractPatcherFilePath(mCachePath, _DeletePathFileName);

        if (File.Exists(deletePahtFileName))
        {
            File.Delete(deletePahtFileName);
        }
    }

    private void PrivPatcherFinishWork(byte [] aData)
    {
        if (mFileReadStream != null)
        {
            mFileReadStream.Close();
            mFileReadStream = null;
        }

        mStartRequest = false;
        mFileReadMode = EFileReadMode.None;
        mFileDataBuffer = aData;
        mTaskFileRead = null;
        mAsyncFileDownload = null;
        ProtPatchFinish(mPathFileName, mFileDataBuffer);
    }

    private void RecursivePatcherDeleteFile(string PathName, string _DeleteFileName)
    {
        string[] aFileName = Directory.GetFiles(PathName);

        for (int i = 0; i < aFileName.Length; i++)
        {
            string FileName = Path.GetFileName(aFileName[i]);
            if (FileName.Equals(_DeleteFileName))
            {
                File.Delete(aFileName[i]);
                return;
            }
        }

        string[] aDirectory = Directory.GetDirectories(PathName);
        for (int i = 0; i < aDirectory.Length; i++)
        {
            RecursivePatcherDeleteFile(aDirectory[i], _DeleteFileName);    
        }
    }

    private void PrivPatcherFindCachingDirectory()
    {
        if (mCachePath == null)
        {
            List<string> CachePath = new List<string>();
            Caching.GetAllCachePaths(CachePath);
            if (CachePath.Count > 0)
            {
                mCachePath = CachePath[0];
            }
        }
    }

    //---------------------------------------------
    private string ExtractPatcherFilePath(string Path, string FileName)
    {
        return string.Format("{0}/{1}", Path, FileName);
    }
}  
