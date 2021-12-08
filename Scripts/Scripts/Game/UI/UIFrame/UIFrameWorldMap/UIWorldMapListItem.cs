using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorldMapViewHolder : ZAdapterHolderBase<C_WorldMapData>
{
    UIWorldMapListItem listItem;

    public override void CollectViews()
    {
        listItem = root.GetComponent<UIWorldMapListItem>();
        listItem.Initilialze();
        base.CollectViews();
    }

    public override void SetSlot(C_WorldMapData data)
    {
        listItem.SetSlot(data);
    }

    public void SetEvent(Action<C_WorldMapData> onClick)
    {
        listItem.SetEvent(onClick);
    }
}


// 프리팹 하나로 합치고싶지만.. 시간관계상..
public class UIWorldMapListItem : MonoBehaviour
{
    [Serializable]
    private class ColorTransision
    {
        public Color on;
        public Color off;
        public bool  OnOff;
        public Graphic target;

        public void SetState(bool b)
        {
            OnOff = b;
            target.color = b ? on : off;
        }
    }

        // world
    [SerializeField] private GameObject objWorld;
    [SerializeField] private GameObject objLock;
    [SerializeField] private GameObject objWorldSelect;
    [SerializeField] private Text txtStageName;

    // local(portal)
    [SerializeField] private GameObject objLocal;
    [SerializeField] private GameObject objLocalSelect;

    [SerializeField] private ColorTransision colorFavorite;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtPortalName;

    [SerializeField] private List<ColorTransision> listToggle;

    private Action<C_WorldMapData> onClick;

    private C_WorldMapData data;
    private UIFrameWorldMap mWorldMap = null;

    public void SetEvent(Action<C_WorldMapData> _onClick)
    {
        onClick = _onClick;
    }

    public void Initilialze()
    {
        SetDefault();
        mWorldMap = UIManager.Instance.Find<UIFrameWorldMap>();
    }

    public void SetDefault()
    {
        objWorld.SetActive(false);
        objLocal.SetActive(false);
    }

    public void SetSlot(C_WorldMapData _data)
    {
        SetDefault();

        data = _data;

        switch (_data.dataType)
        {
            case C_WorldMapData.E_WorldMapDataType.None:
                return;
            case C_WorldMapData.E_WorldMapDataType.World:
                SetWorldSlot(data);
                break;
            case C_WorldMapData.E_WorldMapDataType.Local:
                SetLocalSlot(data);
                break;
        }
    }

    public void SetWorldSlot(C_WorldMapData wData)
    {
        var table = wData.worldInfo.StageTable;

        objWorldSelect.SetActive(ZGameModeManager.Instance.StageTid == wData.worldInfo.StageID);
        txtStageName.text = $"{wData.index}. {DBLocale.GetText(table.StageTextID)}";
        objWorld.SetActive(true);
    }

    public void SetLocalSlot(C_WorldMapData lData)
    {
        var table = lData.localInfo;

        objLocalSelect.SetActive(false);

        txtPortalName.text = $"{lData.index}.{DBLocale.GetText(table.ItemTextID)}";
        colorFavorite.SetState(mWorldMap.CheckFavorite(table.PortalID));
        objLocal.SetActive(true);

        listToggle.ForEach(item => item.SetState(lData.isSelected));
    }

    public void OnClickButton()
    {
        onClick?.Invoke(data);
    }

    public void HandleFavoriteButton()
	{
        bool isOn = !colorFavorite.OnOff;
        mWorldMap.SetWorldMapFavoriteItem(data.localInfo.PortalID, isOn);
        colorFavorite.SetState(isOn);
    }
}
