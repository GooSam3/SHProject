using UnityEngine;

public class UITradeSearchHistoryListItem : MonoBehaviour
{
    #region Variable
    [SerializeField] private ZText Name = null;
    [SerializeField] private uint GroupId = 0;
    #endregion

    public void Initialize(string _name, uint _groupId)
    {
        gameObject.SetActive(true);

        Name.text = _name;
        GroupId = _groupId;
    }

    public void OnSearch()
    {
        if (UIManager.Instance.Find(out UIFrameTrade _trade))
        {
            _trade.SearchBoard.text = Name.text;
            _trade.SearchGroupId = GroupId;
            _trade.SearchRecently.Initialize(Name.text, GroupId);
            _trade.ActiveSearchHistory(false);
            _trade.OnClickSearch();
        }
    }
}