using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISubHUDCurrency : ZUIFrameBase
{
	[SerializeField] private GameObject ActiveRoot;
	[SerializeField] private UISubHUDCurrencyItem[] itemList;

	private bool isSpecialCurrencyOn;

	protected override void OnUnityEnable()
	{
		base.OnUnityEnable();

		ZNet.Data.Me.CurCharData.InvenUpdate -= OnInvenUpdate;
		ZNet.Data.Me.CurCharData.InvenUpdate += OnInvenUpdate;
		ZNet.Data.Me.CurCharData.Itemupdate -= OnItemupdate;
		ZNet.Data.Me.CurCharData.Itemupdate += OnItemupdate;
	}

	protected override void OnUnityDisable()
	{
		base.OnUnityDisable();

		ZNet.Data.Me.CurCharData.InvenUpdate -= OnInvenUpdate;
		ZNet.Data.Me.CurCharData.Itemupdate -= OnItemupdate;
	}

    protected override void OnShow(int _LayerOrder)
    {
        base.OnShow(_LayerOrder);

		ShowBaseCurrency();
	}

	/// <summary> 골드와 다이어만 표시, 스페셜 재화 표시 이후에는 재 호출할 필요가 있다 </summary>
	public void ShowBaseCurrency()
	{
		isSpecialCurrencyOn = false;

		SetCurrency( new List<uint>() { DBConfig.Diamond_ID, DBConfig.Gold_ID } );
	}

	/// <summary> 골드 및 다이어 말고도 다른 재화를 표시하고 싶을때 </summary>
	public void ShowSpecialCurrency( List<uint> currencyItemList )
	{
		isSpecialCurrencyOn = true;

		SetCurrency(currencyItemList);
	}

	private void SetCurrency(List<uint> currencyItemList)
	{
		if (currencyItemList.Count > itemList.Length) {
			ZLog.LogError(
				ZLogChannel.Default, $"표시하려는 재화가 준비된item보다 많다, {currencyItemList.Count}/{itemList.Length }");
		}

		for (int i = 0; i < itemList.Length; ++i) {
			if (currencyItemList.Count > i) {
				itemList[i].Show(currencyItemList[i]);
			}
			else {
				itemList[i].Hide();
			}
		}
	}

	public void HideCurrency()
	{
		for( int i = 0; i < itemList.Length; ++i ) {
			itemList[ i ].Hide();
		}
	}

	private void OnInvenUpdate(bool _setCheck = false)
	{
		RefreshCurrency();
	}

	private void OnItemupdate(uint itemTid)
	{
		// 기본재화만 표시중일때는 골드나 다이어가 아니면 갱신패스(최적화)
		if( isSpecialCurrencyOn == false ) {
			if( itemTid != DBConfig.Gold_ID && 
				itemTid != DBConfig.Diamond_ID ) {
				return;
			}
		}

		RefreshCurrency(itemTid);
	}

	public void RefreshCurrency()
    {
		for( int i = 0; i < itemList.Length; ++i ) {
			itemList[ i ].Refresh();
		}
    }

    public void RefreshCurrency(uint ItemTid)
    {
        for (int i = 0; i < itemList.Length; ++i)
        {
            if(itemList[i].itemTid == ItemTid)
                itemList[i].Refresh();
        }
    }

    public void RefreshCurrency(List<uint> ItemTids)
    {
        if (ItemTids.Count <= 0)
            return;

        for (int i = 0; i < itemList.Length; ++i)
        {
            if (ItemTids.Contains(itemList[i].itemTid))
                itemList[i].Refresh();
        }
    }

    /// <summary> 재화프레임 오픈/클로즈와 상태와 관계없이 가볍게 하위루트를 끄고 켜기 위한 기능 </summary>
    public void WeakShowCurrency()
	{
		ActiveRoot.SetActive( true );

		RefreshCurrency();
	}

	public void WeakHideCurrency()
	{
		ActiveRoot.SetActive( false );
	}

}