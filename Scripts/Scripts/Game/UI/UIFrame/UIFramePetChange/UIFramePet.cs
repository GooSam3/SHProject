using UnityEngine;

public class UIFramePet : UIFramePetChangeBase
{
    [SerializeField] PetContentAdventure ContentAdventure;
    public override bool IsBackable => true;
    public override void Initialize()
    {
        (ContentListView as PCContentListViewPR).Initialize(ViewType, SetChangeModel, ListContentObject[0], ()=>SubSceneController?.PlayLevelupFx());
        ContentCombine.Initialize(ViewType, ListContentObject[1]);
        ContentAdventure.Initialize(ListContentObject[2]);
        ContentReplace.Initailize(ViewType, ListContentObject[3]);
        ContentCollection.Initialize(ViewType,SetChangeModel, ListContentObject[4]);

        base.Initialize();
    }

    public override void OnClickClose()
    {
        base.OnClickClose();

        if (IsLoadScene == false)
            return;

        UIManager.Instance.Close<UIFramePet>();
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

        ContentReplace.OnFrameShow();

        ContentCombine.ReloadListData();
    }

    protected override void OnHide()
    {
        ContentReplace.OnFrameHide();

        ContentAdventure.OnHideFrame();

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
                ContentAdventure.ShowContent();
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
