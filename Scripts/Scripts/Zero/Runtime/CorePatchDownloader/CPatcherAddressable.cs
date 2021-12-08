using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;
using System.Collections.Generic;
using System.Linq;
using System.IO;


// CPatcherAddressable 
// ZeroGames : 정구삼 
// [개요] 유니티 2019에서 패키지로 지원하는 Addressable 랩핑 클래스.  
// [주의!] Addressable Profiles 원도우를 열고 항목 RemotoLoadPath에  아래 심볼 이름을 입력. "ScriptReserve"
// 1) AddressableAssetSetting 윈도우를 열고 BuildCatalog를 체크하여 Catalog 파일을 출력한 후 같은 경로의 FTP에 올려줘야 한다.
// 2) InitializeAsync 는 비동기 함수로 원격 경로로 부터 Catalog를 다운로드해서 PersistantDataPath에 저장한다.
// 3) AddressableSetting : RemoteBuildPath = {CPatcherBase.DownloadURL} 입력해서 원격 URL 주소를 동적으로 연결한다.

public class CPatcherAddressable : CPatcherBase
{
    private AsyncOperationHandle   mDownloadProgress;   
    private bool                 mPatchStart = false;
    private bool                 mInitialize = false;
    private int                  mCurrentLable = 0;
    private long                 mCurrentDownloadSize = 0;
    private uint                 mCurrentAssetBundleCount = 0;
    private long                 mTotalDownloadSize = 0;
    private uint                 mTotalAssetBundleCount = 0;
    private List<string>          m_listPatchLable = null;
    System.Text.StringBuilder      mNote = new System.Text.StringBuilder();
    //---------------------------------------------------
    protected override void OnPatcherInitialize()
    {
        Addressables.InitializeAsync().Completed += HandlePatchInitialize;
    }

    protected override void OnPatcherUpdate(float DeltaTime)
    {
        if (mPatchStart)
        {
            if (mDownloadProgress.IsValid() && mDownloadProgress.IsDone == false && mDownloadProgress.Status != AsyncOperationStatus.Failed)
            {
                // 다운로드가 진행중일때는 다운로드를 출력한다. 다운로드가 없을 경우(패치 없음)는 0이 리턴되며 대신 loadCurrent가 증가한다.
                DownloadStatus status = mDownloadProgress.GetDownloadStatus();
                // 에셋번들 로드 카운트
                uint loadCurrent = ExtractAssetBundleCurrent(mDownloadProgress) + mCurrentAssetBundleCount;
                long currentDownload = status.DownloadedBytes + mCurrentDownloadSize;
                float percent =  mTotalDownloadSize == 0 ? status.Percent :  (float)((double)currentDownload / mTotalDownloadSize);
                ProtPatchProgress(ExtractAssetBundleLoadName(mDownloadProgress), currentDownload, mTotalDownloadSize, percent, loadCurrent, mTotalAssetBundleCount);
            }
        }
    }

    //---------------------------------------------------
    public void DoPatcherAddressableStart(List<string> _listAssetBundleLableName)
    {
        if (mInitialize == false)
		{
            PrivPatchError(E_PatchError.NotInitialized);
            return;
		}

        if (mPatchStart)
        {
            PrivPatchError(E_PatchError.AlreadyPatchProcess, null);
            return;
        }

        mCurrentDownloadSize = 0;
        mPatchStart = true;

        PrivPatchStart(_listAssetBundleLableName);
    }

    public void DoPatcherAddressableDowloadSize(List<string> _listAssetBundleLableName, UnityEngine.Events.UnityAction<long> _eventFinish)
	{
        if (mInitialize == false)
        {
            PrivPatchError(E_PatchError.NotInitialized);
            return;
        }

        if (mPatchStart)
        {
            PrivPatchError(E_PatchError.AlreadyPatchProcess, null);
            return;
        }

        PrivPatchDownloadSize(_listAssetBundleLableName, _eventFinish);
    }

    //---------------------------------------------------
    private void PrivPatchStart(List<string> _listAssetBundleLableName)
    {
        mCurrentDownloadSize = 0;
        mCurrentLable = 0;
        m_listPatchLable = _listAssetBundleLableName;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            PrivPatchError(E_PatchError.NetworkDisable, null);
        }
        else
        {
            PrivPatchClearAssetBundle(); // 받은 카탈로그와 비교하여 구 버전 에셋번들을 모두 삭제한다.
            PrivPatchDownloadNextWork(mCurrentLable);
        }
    }

    private void PrivPatchDownloadNextWork(int _workIndex)
	{
        if (_workIndex < m_listPatchLable.Count)
		{
            mCurrentLable = _workIndex; 
            string labelName = GetCurrentLabelName(mCurrentLable);
            ProtPatchLabelStart(labelName);
            mDownloadProgress = Addressables.DownloadDependenciesAsync(labelName);
            mDownloadProgress.Completed += HandlePatchEnd;
        }
        else
		{
            PrivPatchEnd();
        }
    }

    private void PrivPatchDownloadSize(List<string> _listAssetBundleLableName, UnityEngine.Events.UnityAction<long> _eventFinish)
	{
        int Count = 0;
        mTotalDownloadSize = 0;
        for (int i = 0; i < _listAssetBundleLableName.Count; i++)
		{
            Addressables.GetDownloadSizeAsync(_listAssetBundleLableName[i]).Completed += (_downloadSize) => {

				if (_downloadSize.Status == AsyncOperationStatus.Succeeded)
				{
					mTotalDownloadSize += _downloadSize.Result;

				}
				else
				{
					Debug.LogWarning("[Addressable] Label does not exist : ");
				}

                Count++;

                if (Count >= _listAssetBundleLableName.Count)
                {
                    if (Caching.defaultCache.spaceFree < mTotalDownloadSize)
                    {
                        PrivPatchError(E_PatchError.NotEnoughDiskSpace, $"downloadSize = {mTotalDownloadSize} / cache free = {Caching.defaultCache.spaceFree}");
                    }
                    else
                    {
                        _eventFinish?.Invoke(mTotalDownloadSize);
                    }
                }
            };
        }
    }

    private string GetCurrentLabelName(int _labelIndex)
	{
        string result = "None";

        if (_labelIndex < m_listPatchLable.Count)
		{
            result = m_listPatchLable[_labelIndex];
		}

        return result;
	}

    //----------------------------------------------------
    protected uint ExtractAssetBundleTotal(AsyncOperationHandle _handle)
	{
        uint count = 0;
        List<AsyncOperationHandle> listDependencies = ExtractAssetBundleList(_handle);
        count = (uint)listDependencies.Count;               
        return count;
    }

    protected uint ExtractAssetBundleCurrent(AsyncOperationHandle _handle)
	{
        uint count = 0;
        List<AsyncOperationHandle> listDependencies = ExtractAssetBundleList(_handle);
		for (int i = 0; i < listDependencies.Count; i++)
		{
			//로드가 완료되었을 경우 succeded
		    if (listDependencies[i].Status == AsyncOperationStatus.Succeeded)
			{
				count++;
			}
		}
		return count;
    }

    protected string ExtractAssetBundleLoadName(AsyncOperationHandle _handle)
	{
        string assetBundleName = "";
        List<AsyncOperationHandle> listDependencies = ExtractAssetBundleList(_handle);
        for (int i = 0; i < listDependencies.Count; i++)
		{
            if (listDependencies[i].IsDone == false)
            {
                assetBundleName = ConvertStringToAssetBundleName(listDependencies[i].DebugName);
                break;
            }
        }
        return assetBundleName;
	}

    private List<AsyncOperationHandle> ExtractAssetBundleList(AsyncOperationHandle _handle)
	{
        List<AsyncOperationHandle> listDependencies = new List<AsyncOperationHandle>();
        mDownloadProgress.GetDependencies(listDependencies);

        if (listDependencies.Count > 0)
        {
            AsyncOperationHandle defHandle = listDependencies[0];
            listDependencies.Clear();
            defHandle.GetDependencies(listDependencies);
        }

        return listDependencies;
    }

    private string ConvertStringToAssetBundleName(string _debugName)
	{
        string assetBundleName = string.Empty;
        mNote.Clear();
        bool bracket = false;
        for (int i = 0; i < _debugName.Length; i++)
		{
            if (bracket == false)
			{
                if (_debugName[i] == '(')
				{
                    bracket = true;
				}
			}
            else
			{
                if (_debugName[i] == ')')
				{
                    assetBundleName = mNote.ToString();
                    break;
                }
                else
				{
                    mNote.Append(_debugName[i]);
				}
            }
		}

        return assetBundleName;
	}

    //---------------------------------------------------
    private void HandlePatchInitialize(AsyncOperationHandle<IResourceLocator> Result)
    {
        mInitialize = true;
        ProtPatchInitComplete();
    }

    private void HandlePatchEnd(AsyncOperationHandle AsyncHandle)
    {      
        if (AsyncHandle.Status == AsyncOperationStatus.Succeeded)
        {
            DownloadStatus downloadSize = AsyncHandle.GetDownloadStatus();
            mCurrentDownloadSize += downloadSize.TotalBytes;
            mCurrentAssetBundleCount += ExtractAssetBundleCurrent(AsyncHandle);            
            PrivPatchDownloadNextWork(mCurrentLable + 1);
        }
        else
        {
            PrivPatchError(E_PatchError.PatchFail);
        }      
    }

    //----------------------------------------------------------------------

    private void PrivPatchClearAssetBundle()
    {
        // 다운로드된 에셋번들 목록 확보 
        List<IResourceLocator> listResource = Addressables.ResourceLocators.ToList();
        ResourceLocationMap locationMap = null;
        for (int i = 0; i < listResource.Count; i++)
        {
            locationMap = listResource[i] as ResourceLocationMap;
            if (locationMap != null)
            {
                break;
            }
        }

        if (locationMap == null) return; // Use Asset Database 사용시 

        List<AssetBundleRequestOptions> listAssetBundle = new List<AssetBundleRequestOptions>();
        Dictionary<object, IList<IResourceLocation>>.Enumerator it = locationMap.Locations.GetEnumerator();

        while (it.MoveNext())
        {
            IList<IResourceLocation> resLocation = it.Current.Value;

            if (resLocation.Count == 1)
			{
                if (resLocation[0].ResourceType == typeof(IAssetBundleResource))
                {
                    AssetBundleRequestOptions assetBundle = resLocation[0].Data as AssetBundleRequestOptions;
                    if (assetBundle != null)
                    {
                        listAssetBundle.Add(assetBundle);
                    }
                }
            }
        }

        mTotalAssetBundleCount = (uint)listAssetBundle.Count;

        // 케쉬에 있는 모든 에셋번들 파일이름 확보
        string cachePath = Caching.defaultCache.path;
        string[] arrFileName = Directory.GetDirectories(cachePath);

        for (int i = 0; i < arrFileName.Length; i++)
        {
            string assetBundleName = Path.GetFileNameWithoutExtension(arrFileName[i]);
            AssetBundleRequestOptions assetBundleFile = null;
            for (int j = 0; j < listAssetBundle.Count; j++)
            {
                if (assetBundleName == listAssetBundle[j].BundleName)
                {
                    assetBundleFile = listAssetBundle[j];
                    break;
                }
            }

            // 에셋번들 폴더인지 확인한다.
            List<Hash128> hashList = new List<Hash128>();
            Caching.GetCachedVersions(assetBundleName, hashList);

            if (assetBundleFile != null)
            {
                for (int h = 0; h < hashList.Count; h++)
                {
                    if (hashList[h].ToString() != assetBundleFile.Hash) // 다른 버전의 헤쉬는 폐기한다.
                    {
                        Caching.ClearCachedVersion(assetBundleName, hashList[h]);
                    }
                }
            }
            else
			{                
                if (hashList.Count > 0)  // 에셋번들이나 카탈로그에 없는 번들로써 업데이트 되고 폐기된 폴더이다.
                {
                    Caching.ClearAllCachedVersions(assetBundleName);
                    string deletaPath = string.Format("{0}/{1}", cachePath, assetBundleName);
                    Directory.Delete(deletaPath, true);
                }
            }
        }
    }

    private void PrivPatchError(E_PatchError _errorType, string _message = null)
	{
        mPatchStart = false;
        ProtPatchError(_errorType, _message);
    }

    private void PrivPatchEnd()
	{
        mPatchStart = false;
        ProtPatchFinish(GetCurrentLabelName(mCurrentLable), null);
        ProtPatchDownloadFinish();
    }
}
