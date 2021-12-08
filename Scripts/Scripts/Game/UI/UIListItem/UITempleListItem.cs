using GameDB;
using System;
using UnityEngine;
public class UITempleListItem : MonoBehaviour
{
    
    [SerializeField] private ZImage iconImage;
    [SerializeField] private ZText titleText;
    [SerializeField] private ZText areaNameText;

    [SerializeField] public GameObject checkBoxImage;
    
    private uint _stageID;
    private Action<uint> selectEvent = null;

    public void DoInit(uint stageID, Action<uint> itemEvent)
    {
        _stageID = stageID;
        gameObject.SetActive(true);
        if (selectEvent == null)
        {
            selectEvent = itemEvent;
        }
    }

    public void DoUpdate(uint selectTempleTid)
    {

        var stageTable = DBStage.Get(_stageID);
        Temple_Table templeTable= null;
     
        if (DBTemple.TryGet(stageTable.LinkTempleID, out templeTable) == false)
        {
            ZLog.LogError(ZLogChannel.Default, $"[Table오류] TempleTable ->LinkTempleID {stageTable.LinkTempleID}가 StageTable에 없습니다. ");
        }
     
        var templeList = ZNet.Data.Me.CurCharData.TempleInfo.GetStage(stageTable.StageID);
        this.titleText.text = DBLocale.GetText(stageTable.StageTextID); 

        iconImage.sprite = null;
        areaNameText.text = string.Empty;

        if (templeTable.TempleAreaName != null)
        {
            this.areaNameText.text = DBLocale.GetText(templeTable.TempleAreaName);
        }
        if (templeTable.Icon != null)
        {
            iconImage.sprite = ZManagerUIPreset.Instance.GetSprite(templeTable.Icon);
        }

        checkBoxImage.SetActive(_stageID == selectTempleTid);
     
    }

    public void UITempleListItemClick()
    {
        ZLog.Log(ZLogChannel.Default, $"OnUITempleListItemClick {_stageID}");
        if(selectEvent!= null)
        {
            selectEvent(_stageID);
        }
    }

}
