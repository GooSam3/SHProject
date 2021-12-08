using GameDB;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UIGemCraftMaterialListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Text Count;
	[SerializeField] private Text Name;
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

		Icon.sprite = UICommon.GetItemIconSprite(_item.ItemID);
		Name.text = DBLocale.GetText(_item.ItemTextID);

		SetCountText(1);
	}

	public void SetCountText(ulong _makeCnt)
	{
		ulong havMaterialCnt = Me.CurCharData.GetItem(Item.ItemID) != null ? Me.CurCharData.GetItem(Item.ItemID).cnt : 0;
		bool check = havMaterialCnt == 0 || havMaterialCnt < ItemCount * _makeCnt;
		if (check)
		{
			if (UIManager.Instance.Find(out UIFrameItemGem _gem))
				_gem.CraftBtn.interactable = false;

			Count.color = new Color(255, 0, 0, 255);
		}

		Count.text = (ItemCount * _makeCnt).ToString() + " / " + havMaterialCnt.ToString();
	}
}