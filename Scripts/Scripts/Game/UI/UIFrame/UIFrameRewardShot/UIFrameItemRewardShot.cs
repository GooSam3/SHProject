using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WebNet;

public class UIFrameItemRewardShot : ZUIFrameBase
{
    [SerializeField] private UIItemRewardScrollAdapter ScrollAdapter;
    [SerializeField] private Animation RewardShotAni;

    protected override void OnInitialize()
    {
        base.OnInitialize();
        ScrollAdapter.Initialize();
    }

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    public void AddItem(GainInfo _item)
    {
        ScrollAdapter.SetScrollData(_item);
        PlayReward();
    }

    public void AddItem(List<GainInfo> _itemList)
    {
        ScrollAdapter.SetScrollData(_itemList);
        PlayReward();
    }

    /// <summary>
    /// 201104 이윤선 추가 : 외부 컨버팅 한번 줄이기 위해 추가 
    /// </summary>
    /// <param name="_items"></param>
    public void AddItem(List<GetItemInfo> _items)
    {
        if (_items == null)
            return;

        var list = new List<GainInfo>();
        _items.ForEach(t => list.Add(new GainInfo(t)));
        ScrollAdapter.SetScrollData(list);
        PlayReward();
    }

    public void AddItem(List<GuildDungeonClearReward> _itemList)
	{
        ScrollAdapter.SetScrollData(_itemList);
        PlayReward();
	}

    /// <summary>
    /// ljh : 패킷으로 출력
    /// </summary>
    /// <param name="_items"></param>
    public void AddItem(WebNet.ItemInfo? _items)
    {
        if (_items.HasValue == false)
        {
            Close();
            return;
        }

        var data = _items.Value;

        var list = new List<GainInfo>();

        for (int i = 0; i < data.AccountStackLength; i++)
        {
            list.Add(new GainInfo(data.AccountStack(i).Value));
        }
        for (int i = 0; i < data.EquipLength; i++)
        {
            list.Add(new GainInfo(data.Equip(i).Value));
        }
        for (int i = 0; i < data.RuneLength; i++)
        {
            list.Add(new GainInfo(data.Rune(i).Value));
        }
        for (int i = 0; i < data.StackLength; i++)
        {
            list.Add(new GainInfo(data.Stack(i).Value));
        }

        if (list.Count <= 0)
        {
            Close();
            return;
        }

        AddItem(list);
    }

    public void AddItem(List<WebNet.ItemInfo> _items)
    {
        var dic = new Dictionary<(GainType,uint), GainInfo>();

        foreach (var iter in _items)
		{
            var data = iter;

            for (int i = 0; i < data.AccountStackLength; i++)
            {
                var key = (GainType.TYPE_ITEM, data.AccountStack(i).Value.ItemTid);
                if (dic.ContainsKey(key) == true)
                    dic[key] = new GainInfo(data.AccountStack(i).Value);
                else
                    dic.Add(key, new GainInfo(data.AccountStack(i).Value));
            }
            for (int i = 0; i < data.EquipLength; i++)
            {
                var key = (GainType.TYPE_ITEM, data.Equip(i).Value.ItemTid);
                if (dic.ContainsKey(key) == true)
                    dic[key] = new GainInfo(data.Equip(i).Value);
                else
                    dic.Add(key, new GainInfo(data.Equip(i).Value));
            }
            for (int i = 0; i < data.RuneLength; i++)
            {
                var key = (GainType.TYPE_ITEM, data.Rune(i).Value.ItemTid);
                if (dic.ContainsKey(key) == true)
                    dic[key] = new GainInfo(data.Rune(i).Value);
                else
                    dic.Add(key, new GainInfo(data.Rune(i).Value));
            }
            for (int i = 0; i < data.StackLength; i++)
            {
                var key = (GainType.TYPE_ITEM, data.Stack(i).Value.ItemTid);
                if (dic.ContainsKey(key) == true)
                    dic[key] = new GainInfo(data.Stack(i).Value);
                else
                    dic.Add(key, new GainInfo(data.Stack(i).Value));
            }
        }

        if (dic.Count <= 0)
        {
            Close();
            return;
        }

        AddItem(dic.Values.ToList());
    }

    /// <summary>
    /// 201104 이윤선 추가 : 통합형 . 호출자의 Get...Info 형태로의 컨버팅은 불가피함 .. 
    /// +++ 현시점 아이템만 지원되게끔 구현이 돼있어서 우선 주석함.
    /// </summary>
    //public void AddItems(
    //    List<GetItemInfo> _items
    //    , List<GetPetInfo> _pets
    //    , List<GetChangeInfo> _changes)
    //{
    //    int capacity = (_items != null ? _items.Count : 0) + (_pets != null ? _pets.Count : 0) + (_changes != null ? _changes.Count : 0);

    //    if (capacity == 0)
    //        return;

    //    var list = new List<GainInfo>(capacity);

    //    if (_items != null)
    //        _items.ForEach(t => list.Add(new GainInfo(t)));
    //    if (_pets != null)
    //        _pets.ForEach(t => list.Add(new GainInfo(t)));
    //    if (_changes != null)
    //        _changes.ForEach(t => list.Add(new GainInfo(t)));

    //    ScrollAdapter.SetScrollData(list);
    //    PlayReward();
    //}


    private void PlayReward()
    {
        RewardShotAni.Play();

        this.transform.localPosition = Vector3.zero;
        this.transform.localScale = Vector3.one;

        Invoke(nameof(PlayEnd), 1.5f);
    }

    private void PlayEnd()
    {
        UIManager.Instance.Close<UIFrameItemRewardShot>();
    }
}
