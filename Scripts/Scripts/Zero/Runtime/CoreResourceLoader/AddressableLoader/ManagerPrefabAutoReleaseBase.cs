using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

//  오브젝트 풀링 기반으로 에셋번들 메모리 사이즈에 따른 자율 해제 리소스 핸들을 구현 

public abstract class ManagerPrefabAutoReleaseBase : ManagerPrefabPoolBase
{       
    public enum E_LoadingQuality
    {
        Low,        // 퍼포먼스 비용 가장 적으나 전체 로딩시간은 길다.  
        Medium,     // 일반   디바이스에서 사용할수 있다.
        High,       // 고성능 디바이스에서 사용할 수 있다.
    }

    protected enum E_AutoReleaseFilterType
    {
        None,                    // 필터링 하지 않는다
        ReferenceCountAndTime,     // 레퍼런스가 가장 작고 오래된 리소스를 1개 해재
        ReferenceCountOnly,        // 레퍼런스가 가장 작은 객체를 모두 해재 
        TimeOnly,                 // 오래된 리소스를 1개 해제
    }

    public class SLoadingQuality
    {
        public uint AutoPoolSize;
        public int  UploadSliceTime;
        public int  UploadBufferSize;
        public SLoadingQuality(uint _poolSize, int _sliceTime, int _bufferSize) { AutoPoolSize = _poolSize; UploadSliceTime = _sliceTime; UploadBufferSize = _bufferSize; }
    }

    private class SAssetBundleInfo
    {
        public string       BundleName;
        public string       FilePath;
        public long         DiskSize = 0;
        public int          RefCounter = 0;
        public AssetBundle   Bundle = null;
    }

    private class SAddressableInfo
    {
        public string               AddressableName;
        public List<SAssetBundleInfo> DependencyAssetBundle = new List<SAssetBundleInfo>();
        public uint                 RefCount = 0;
        public float                LastRefTime = 0;
    }

    private SLoadingQuality mQualityLow = new SLoadingQuality(512, 1, 4);
    private SLoadingQuality mQualityMedium = new SLoadingQuality(1024, 2, 16);
    private SLoadingQuality mQualityHigh = new SLoadingQuality(1536, 3, 32);

    [SerializeField]
    private E_LoadingQuality        LoadingQuality = E_LoadingQuality.Low;
    [SerializeField]
    private E_AutoReleaseFilterType  ReleaseType = E_AutoReleaseFilterType.ReferenceCountAndTime;

    private bool mActivate = true;
    //----------------------------------------------------------------------

    private Dictionary<string, SAddressableInfo> m_dicAddressableInfo = new Dictionary<string, SAddressableInfo>();
    private Dictionary<string, SAssetBundleInfo> m_dicAssetBundleInfo = new Dictionary<string, SAssetBundleInfo>();
    private uint   mPoolSize = 0;  
    protected long mAssetBundleUseSize = 0;
    //------------------------------------------------------------------------
    protected override void OnUnityAwake()
    {
        base.OnUnityAwake();
        InitializeLoadingQuality();
    }

    protected override void Update()
	{
        base.Update();

        if (mActivate) 
		{

		}
	}

    protected override void OnPrefabInstanceOrigin(string _addressableName, GameObject _originInstance) 
    {
        if (m_dicAddressableInfo.ContainsKey(_addressableName)) return;

        SAddressableInfo AddInfo = new SAddressableInfo();
        AddInfo.AddressableName = _addressableName;
        m_dicAddressableInfo[_addressableName] = AddInfo;

        Addressables.LoadResourceLocationsAsync(_addressableName).Completed += (AsyncOperationHandle<IList<IResourceLocation>> ResourceLocation) =>
        {
            if (ResourceLocation.Result.Count > 0)
            {
                if (ResourceLocation.Result[0].Dependencies != null)
                {
                    for (int i = 0; i < ResourceLocation.Result[0].Dependencies.Count; i++)
                    {
                        IResourceLocation Location = ResourceLocation.Result[0].Dependencies[i];
                        AssetBundleRequestOptions RequestOptions = Location.Data as AssetBundleRequestOptions;
                       
                        SAssetBundleInfo BundleInfo = AddAssetBundleInfo(RequestOptions.BundleName, Location.InternalId, RequestOptions.BundleSize);
                        AddInfo.DependencyAssetBundle.Add(BundleInfo);
                    }
                }
            }
        };
    }
    protected override void OnPrefabInstanceClone(string _addressableName, GameObject _cloneInstance) 
    {
        if (m_dicAddressableInfo.ContainsKey(_addressableName))
		{
            SAddressableInfo AddInfo = m_dicAddressableInfo[_addressableName];
            AddInfo.RefCount++;
            AddInfo.LastRefTime = Time.time;
		}
    }
    
    protected override void OnPrefabRemoveClone(string _addressableName, GameObject _removeClone) 
    {
        if (m_dicAddressableInfo.ContainsKey(_addressableName))
        {
            SAddressableInfo AddInfo = m_dicAddressableInfo[_addressableName];
            AddInfo.RefCount--;
        }
    }
    protected override void OnPrefabRemoveOrigin(string _addressableName, GameObject _removeOrigin) 
    {
        if (m_dicAddressableInfo.ContainsKey(_addressableName))
        {
            SAddressableInfo AddInfo = m_dicAddressableInfo[_addressableName];

            for (int i = 0; i < AddInfo.DependencyAssetBundle.Count; i++)
			{
                AddInfo.DependencyAssetBundle[i].RefCounter--;
			}
        }       
    }

    //-------------------------------------------------------------------------
    private void InitializeLoadingQuality()
    {
        SLoadingQuality QualityInfo = mQualityLow;
        switch (LoadingQuality)
        {
            case E_LoadingQuality.Low:
                QualityInfo = mQualityLow;
                break;
            case E_LoadingQuality.Medium:
                QualityInfo = mQualityMedium;
                break;
            case E_LoadingQuality.High:
                QualityInfo = mQualityHigh;
                break;
        }
        mPoolSize = QualityInfo.AutoPoolSize * 1048576;  // 메가바이트 => 바이트 변환 상수 
        //QualitySettings.asyncUploadTimeSlice = QualityInfo.UploadSliceTime;
        //QualitySettings.asyncUploadBufferSize = QualityInfo.UploadBufferSize;
        Application.backgroundLoadingPriority = ThreadPriority.Low;
    }

    private SAssetBundleInfo AddAssetBundleInfo(string _assetBundleName, string _assetBundleFileName, long _fileSize)
    {
        SAssetBundleInfo BundleInfo = null;// new SAssetBundleInfo();
        if (m_dicAssetBundleInfo.ContainsKey(_assetBundleName))
		{
            BundleInfo = m_dicAssetBundleInfo[_assetBundleName];
		}
        else
		{
            BundleInfo = new SAssetBundleInfo();
            BundleInfo.BundleName = _assetBundleName;
            BundleInfo.FilePath = _assetBundleFileName;
            BundleInfo.DiskSize = _fileSize;

            m_dicAssetBundleInfo[_assetBundleName] = BundleInfo;
            mAssetBundleUseSize += _fileSize;

            List<AssetBundle> BundleList = AssetBundle.GetAllLoadedAssetBundles().ToList();
            for (int i = 0; i < BundleList.Count; i++)
            {
                if (BundleList[i].name == _assetBundleName)
				{
                    BundleInfo.Bundle = BundleList[i];
                    break;
				}
            }
		}

        BundleInfo.RefCounter++;
        return BundleInfo;
	}
}
