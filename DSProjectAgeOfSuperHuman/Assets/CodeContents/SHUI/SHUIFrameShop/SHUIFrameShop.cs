using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHUIFrameShop : SHUIFrameBase
{
	public enum EShopType
	{
		None,
		Hero,
		Package,
		Gold,
		Cash,
	}
	[System.Serializable]
	public class SShopType
	{
		public EShopType			ShopType = EShopType.None;
		public CUIWidgetDialogBase Dialog = null;
	}
	[SerializeField]
	private List<SShopType> ShopCategory = new List<SShopType>();

	//---------------------------------------------------------------------------
	protected override void OnUIFrameInitialize()
	{
		base.OnUIFrameInitialize();
	}

	protected override void OnUIFrameInitializePost()
	{
		base.OnUIFrameInitializePost();
		PrivUIFrameShopShowCategory(EShopType.Hero);
	}

	//----------------------------------------------------------------------------
	private void PrivUIFrameShopShowCategory(EShopType eShopType)
	{
		bool bFind = false;
		for (int i = 0; i < ShopCategory.Count; i++)
		{
			if (ShopCategory[i].ShopType == eShopType)
			{
				ShopCategory[i].Dialog.DoUIWidgetShowHide(true);
				bFind = true;
				break;
			}
		}

		if (bFind == false)
		{
			InternalDialogShow(null);
		}
	}

	//-----------------------------------------------------------------------------
	public void HandleShopCategoryHero()
	{
		PrivUIFrameShopShowCategory(EShopType.Hero);
	}

	public void HandleShopCategoryPackage()
	{
		PrivUIFrameShopShowCategory(EShopType.Package);
	}

	public void HandleShopCategoryGold()
	{
		PrivUIFrameShopShowCategory(EShopType.Gold);
	}

	public void HandleShopCategoryCash()
	{
		PrivUIFrameShopShowCategory(EShopType.Cash);
	}
}
