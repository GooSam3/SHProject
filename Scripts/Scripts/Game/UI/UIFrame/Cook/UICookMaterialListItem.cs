using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UICookMaterialListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Text Count;
	[SerializeField] private Text Name;
	[SerializeField] private GameObject Make;
	#endregion

	#region System Variable
	public Item_Table Item = null;
	public uint ItemCount = 0;
	#endregion

	public void Initialize(Item_Table _item, uint _cnt)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		if (_item == null)
			return;

		Item = _item;
		ItemCount = _cnt;

		Icon.sprite = UICommon.GetItemIconSprite(Item.ItemID);
		Name.text = DBLocale.GetText(Item.ItemTextID);
		Make.SetActive(DBMake.IsMakable(Item.ItemID));

		SetCountText(1);
	}

	public void OnMakeMaterialItem()
	{
		if (UIManager.Instance.Find(out UIFrameItemMake _make))
		{
			if (_make.SelectMakeTid == 0)
				return;

			var makeData = DBMake.GetMakeData(_make.SelectMakeTid);

			if (makeData == null || Item == null)
				return;

			_make.SelectMakeType = makeData.MakeType;
			_make.SelectMakeSubType = makeData.MakeTapType;
			_make.SelectMakeClassType = Item.UseCharacterType;
			_make.OnSelectMainTab(Convert.ToInt32(makeData.MakeType));
			_make.OnSelectMakeItem(makeData);
		}
	}

	public void SetCountText(ulong _makeCnt)
	{
		ulong havMaterialCnt = Me.CurCharData.GetItem(Item.ItemID) != null ? Me.CurCharData.GetItem(Item.ItemID).cnt : 0;
		if (havMaterialCnt == 0 || havMaterialCnt < ItemCount * _makeCnt)
			Count.color = new Color(255, 0, 0, 255);

		Count.text = (ItemCount * _makeCnt).ToString() + " / " + havMaterialCnt.ToString();
	}
}