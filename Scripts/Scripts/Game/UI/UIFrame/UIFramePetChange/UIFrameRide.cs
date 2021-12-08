public class UIFrameRide : UIFramePetChangeBase
{
    public override bool IsBackable => true;

    public override void Initialize()
    {
        (ContentListView as PCContentListViewPR).Initialize(ViewType, SetChangeModel, ListContentObject[0],()=>SubSceneController?.PlayLevelupFx());
        ContentCombine.Initialize(ViewType, ListContentObject[1]);
        ContentReplace.Initailize(ViewType, ListContentObject[3]);
        ContentCollection.Initialize(ViewType, SetChangeModel, ListContentObject[4]);

        base.Initialize();
    }

    public override void OnClickClose()
    {
        base.OnClickClose();

        if (IsLoadScene == false)
            return;

        UIManager.Instance.Close<UIFrameRide>();
    }

    // 탈것 장착은 mmo에서 날라오는 관계로..
    protected override void OnShow(int _LayerOrder)
    {
        ContentListView.OnFrameShow();
        ContentReplace.OnFrameShow();

        base.OnShow(_LayerOrder);

        ContentCombine.ReloadListData();
    }

    protected override void OnHide()
    {
        ContentListView.OnFrameHide();
        ContentReplace.OnFrameHide();

        base.OnHide();
    }

    protected override void SetMainContent(E_PetChangeContentType type)
    {
        base.SetMainContent(type);

        switch (type)
        {
            case E_PetChangeContentType.Content_1:
                ContentListView.ShowContent();
                break;

            case E_PetChangeContentType.Content_2:
                ContentCombine.ShowContent();
                break;

            case E_PetChangeContentType.Content_3:
                break;
          
            case E_PetChangeContentType.Content_4:
                ContentReplace.ShowContent();
                break;
            case E_PetChangeContentType.Content_5:
                ContentCollection.ShowContent();
                break;
        }
    }
}
