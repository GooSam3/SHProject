using UnityEngine;
using ZDefine;

public abstract class PCContentReplaceBase : PCRContentBase
{
    [SerializeField] protected UIPCRReplaceScrollAdapter scrollReplace;
    [SerializeField] protected ZText textNotice;

    [SerializeField] protected MileageChangePetPopup popupDetail;

    protected E_PetChangeViewType ViewType;

    private UIFramePetChangeBase.C_ContentUseObject Checker;

    public void Initailize(E_PetChangeViewType target, UIFramePetChangeBase.C_ContentUseObject checker)
    {
        ViewType = target;
        Checker = checker;
        textNotice.text = DBLocale.GetText("Not_CardChange_Message");

        ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPetChangeReplaceListItem), (obj)=>
        {
            scrollReplace.Initialize();
            scrollReplace.SetEvent(OnReplace, OnConfirm, OnDetail);
            InitializeList();

            ZPoolManager.Instance.Return(obj);
        });
    }

	public override void HideContent()
	{
		base.HideContent();

        if (popupDetail.gameObject.activeSelf)
        {
            popupDetail.OnClose();
            popupDetail.gameObject.SetActive(false);
        }
    }

    public virtual void OnFrameShow() 
    {
        InvokeRepeating(nameof(RefreshRemainTime),1f,  1f);
    }

    public virtual void OnFrameHide() 
    {
        // 펠로우 ui 작업 후 세팅 popupDetail.Release();
        CancelInvoke();
    }

    protected virtual void RefreshRemainTime()
    {
        if (Checker.isOn == false)
            return;

        scrollReplace.RefreshRemainTime();
    }

    protected virtual void InitializeList()
    {
        if (popupDetail.gameObject.activeSelf)
        {
            popupDetail.OnClose();
            popupDetail.gameObject.SetActive(false);
        }
    }

    public override void ShowContent()
    {
        InitializeList();

        bool bIsEmpty = scrollReplace.Data.Count <= 0;
        textNotice.gameObject.SetActive(bIsEmpty);
    }

    protected abstract void OnReplace(GachaKeepData data);

    protected abstract void OnConfirm(GachaKeepData data);

    protected void OnDetail(GachaKeepData data)
    {
        popupDetail.Open(data, OnReplace, OnConfirm, () => {
            popupDetail.OnClose();
            popupDetail.gameObject.SetActive(false);
        });
        popupDetail.gameObject.SetActive(true);
    }
}
