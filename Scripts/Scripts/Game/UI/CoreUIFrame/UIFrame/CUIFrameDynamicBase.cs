using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

abstract public class CUIFrameDynamicBase : CUIFrameWidgetBase
{
    private CUIFrameBase mDynamicUIFrame = null;
    private bool        mLoadingFrame = false;
    private int         mLayerOrder = 0;
    private AsyncOperationHandle<GameObject> mAyncUIFrame;
    //------------------------------------------------------------------
    protected override void OnShow(int _LayerOrder) 
    {       
        mLayerOrder = _LayerOrder;

        if (mLoadingFrame == false)
        {
            mLoadingFrame = true;
            PrivUIFrameDynamicShowStart();
        }
    }
    protected override void OnHide() 
    {
        if (mLoadingFrame)
        {
            mLoadingFrame = false;
            if (mDynamicUIFrame != null)
            {
                Addressables.Release(mAyncUIFrame);
            }
        }
        else
        {
            PrivUIFrameDynamicHide();
        }
    }

    protected override void OnInputEnable(bool _Enable) 
    {
        if (mDynamicUIFrame != null)
        {
            mDynamicUIFrame.ImportInputEnable(_Enable);
        }
    }

    protected override void OnInitialize()
    {

    }

    //----------------------------------------------------------------
    private void PrivUIFrameDynamicShowStart()
    {
        mAyncUIFrame = Addressables.InstantiateAsync(ID, gameObject.transform.parent);
        mAyncUIFrame.Completed += HandleUIFrameDynamicFinish;
    }

    private void PrivUIFrameDynamicHide()
    {
        if (mDynamicUIFrame != null)
        {
            Addressables.ReleaseInstance(mDynamicUIFrame.gameObject);
            mDynamicUIFrame = null;
            Resources.UnloadUnusedAssets();
        }
    }

    private void PrivUIFrameDynamicLoadingFinish(CUIFrameBase _UIFrame)
    {
        SetMonoActive(false); // 자신은 숨겨지지만 UIManager에서는 Show상태로 인식된다. 
        mDynamicUIFrame = _UIFrame;
        mDynamicUIFrame.transform.parent = transform.parent;
        mDynamicUIFrame.ImportInitialize(true);
        mDynamicUIFrame.ImportShow(mLayerOrder);
        mDynamicUIFrame.ImportInputEnable(InputEnable);
    }
    //----------------------------------------------------------------
    private void HandleUIFrameDynamicFinish(AsyncOperationHandle<GameObject> _LoadGameObject)
    {
        mLoadingFrame = false;

        if (_LoadGameObject.Status == AsyncOperationStatus.Failed)
        {
            ZLog.Log(ZLogChannel.Loading, string.Format("[UIFrame] Invalidate UIFrame Name : {0}", ID));
        }
        else
        {
            CUIFrameBase UIFrame = _LoadGameObject.Result.GetComponent<CUIFrameBase>();
            if (UIFrame != null)
            {
                PrivUIFrameDynamicLoadingFinish(UIFrame);
            }
            OnUIFrameDynamicLoadComplete();
        }
    }

    //----------------------------------------------------------------
    protected virtual void OnUIFrameDynamicLoadComplete() { }
}
