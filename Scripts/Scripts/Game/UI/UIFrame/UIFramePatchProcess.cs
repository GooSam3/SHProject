using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIFramePatchProcess : ZUIFrameBase
{
	[SerializeField] private Text TxtTitle = null;
	[SerializeField] private Text TxtDesc = null;
    [SerializeField] private int  NextImageDelay = 8;
    private Slider mProgressBar = null;
    private int    mCurrentImage = 0;
    private float  mCurrentTime = 0;
    private string mPrevAssetBundleName = string.Empty;
  
    private List<RawImage> m_listGameTipBG = new List<RawImage>();
    //-----------------------------------------------------
    protected override void OnInitialize() 
    {
        base.OnInitialize();
        mProgressBar = GetComponentInChildren<Slider>();
        GetComponentsInChildren(true, m_listGameTipBG);
        SelectGameTip(0);
    }

	protected override void OnHide()
	{
		base.OnHide();
        ZLog.EndProfile(mPrevAssetBundleName);
    }
	//-----------------------------------------------------
	private void Update()
	{
        mCurrentTime += Time.deltaTime;
        if (mCurrentTime >= NextImageDelay)
		{    
            HandleNextGameTip();
		}
	}

	//-----------------------------------------------------
	public void UpdatePatchAssetBundle(string _assetBundleName, float _progress, long _downloadedByte, long _totalByte, uint _loadCurrent, uint _loadMax)
	{
        long megaSizeTotal =  _totalByte / 1048576;
        long megaDownSizeTotal = _downloadedByte / 1048576;
        float percent = _progress * 100f;
        string Description = null;
        if (_downloadedByte == _totalByte)
		{
            TxtTitle.text = "Patch_Load_AsssetBundle";
            if (_loadMax == 0) _loadMax = 1;
            _progress = (float)_loadCurrent / (float)_loadMax;
            percent = _progress * 100f;
            Description = string.Format("{0}%", percent.ToString("N2"));
        }
        else if (_progress < 0.01f)
		{
            _progress = 0; 
            megaDownSizeTotal = 0;
            Description = "Patch_Download_Prepare";
        }
        else
		{            
            TxtTitle.text = "Patch_Download_AssetBundle";
            Description = string.Format("{0}% {1} / {2}MB", percent.ToString("N2"), megaDownSizeTotal.ToString(), megaSizeTotal.ToString());
        }

        TxtDesc.text = Description;
        mProgressBar.value = _progress;

        PrintLogAssetBundleProfile(_assetBundleName);
    }

    public void UpdatePatchTableDownload(int _currentCount, int _totalCount, float _progress)
	{
        float Section = (float)1f / (float)_totalCount;
        float Progress = (float)(_currentCount - 1) / (float)_totalCount;
        Progress += (Section * _progress);
        int Percent = Mathf.CeilToInt(Progress * 100f);

        string Description = string.Format("{0}%", Percent.ToString(), _currentCount, _totalCount);
        TxtDesc.text = Description;
        mProgressBar.value = Progress; 
    }

    public void UpdatePatchTableRead(int _currentCount, int _totalCount)
	{
        float Progress = (float)_currentCount / (float)_totalCount;
        int Percent = Mathf.CeilToInt(Progress * 100f);
        string Description = string.Format("{0}%", Percent.ToString());
        TxtDesc.text = Description;
        mProgressBar.value = Progress;
	}

    public void SetPatchInfo(string _pathchName, long _totalSize)
	{       
        TxtTitle.text = _pathchName;
    }

    //-----------------------------------------------------------------------------
    public void HandleNextGameTip()
	{
        mCurrentTime = 0;
        SelectGameTip(mCurrentImage + 1);
    }

    //------------------------------------------------------------------------------
    private void SelectGameTip(int _index)
	{
        int SelectIndex = _index;
        if (_index < 0)
		{
            SelectIndex = m_listGameTipBG.Count - 1;
		}

        if (_index >= m_listGameTipBG.Count)
		{
            SelectIndex = 0;
		}

        mCurrentImage = SelectIndex;

        for (int i = 0; i < m_listGameTipBG.Count; i++)
		{
            if (i == SelectIndex)
			{
                m_listGameTipBG[i].gameObject.SetActive(true);
            }
            else
			{
                m_listGameTipBG[i].gameObject.SetActive(false);
            }
        }
	}

    private void PrintLogAssetBundleProfile(string _assetBundName)
	{
        if (_assetBundName == string.Empty) return;

        if (mPrevAssetBundleName != _assetBundName)
		{
            ZLog.EndProfile(mPrevAssetBundleName);
            ZLog.BeginProfile(_assetBundName);
            mPrevAssetBundleName = _assetBundName;
        }
	}
}
