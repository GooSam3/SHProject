using System.Collections.Generic;
using UnityEngine;

public class UIFrameGuildPopup_GuildBenefit : UIFrameGuildOverlayPopupBase
{
    public class AbilityActionTitleValuePair
    {
        public string title;
        public string value;
    }

    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private ScrollGuildBuffListAdapter ScrollAdapter;
    #endregion
    #endregion

    #region System Variables
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    public override void Initialize(UIFrameGuild guildController)
    {
        base.Initialize(guildController);

        ScrollAdapter.Parameters.ItemPrefab = ZPoolManager.Instance.FindClone(E_PoolType.UI, nameof(UIGuildBuffListSlot)).transform as RectTransform;
        var prefab = ScrollAdapter.Parameters.ItemPrefab;
        prefab.transform.SetParent(transform);
        prefab.localPosition = Vector3.zero;
        prefab.localScale = Vector3.one;
        prefab.gameObject.SetActive(false);

        ScrollAdapter.Initialize();
    }

    public override void Open()
    {
        base.Open();
        Refresh();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void Refresh()
    {
        List<uint> buffTids = new List<uint>();

        foreach (var data in DBGuild.GetGuildBuffDic())
        {
            buffTids.Add(data.Value.GuildBuffID);
        }

        ScrollAdapter.Refresh(buffTids);
    }


    #endregion

    #region OnClick Event (인스펙터 연결)
    #endregion
}
