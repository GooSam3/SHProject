using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class SelectCollectInfo : MonoBehaviour
{
	[SerializeField] private Image ItemIcon;
	[SerializeField] private Image GradeBoard;
	[SerializeField] private Text GradeText;
	[SerializeField] private GameObject Block;
	[SerializeField] private GameObject CollectionCheck;
	[SerializeField] private GameObject CollectionCheckIcon;

	private uint SlotIdx;
	private ZDefine.CollectData CollectData;
	private GameDB.ItemCollection_Table ItemCollectionTable;

	private uint CollectionItemTid;

	public void Reset(uint _slot, ZDefine.CollectData _collectData, GameDB.ItemCollection_Table _itemCollectionTable)
	{
		CollectionItemTid = 0;
		ItemIcon.sprite = null;
		GradeBoard.sprite = null;
		GradeText.text = string.Empty;
		ItemIcon.gameObject.SetActive(false);
		GradeBoard.gameObject.SetActive(false);
		CollectionCheck.gameObject.SetActive(false);
		CollectionCheckIcon.gameObject.SetActive(false);
		Block.gameObject.SetActive(true);

		SlotIdx = _slot;
		CollectData = _collectData;
		ItemCollectionTable = _itemCollectionTable;

		switch (SlotIdx)
		{
			case 0:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_01;
				break;
			case 1:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_02;
				break;
			case 2:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_03;
				break;
			case 3:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_04;
				break;
			case 4:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_05;
				break;
			case 5:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_06;
				break;
			case 6:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_07;
				break;
			case 7:
				CollectionItemTid = ItemCollectionTable.CollectionItemID_08;
				break;
		}
		SetInfo();
		SetBlock();
	}

	private void SetInfo()
	{
		if (CollectionItemTid == 0)
		{
			this.gameObject.SetActive(false);
			return;
		}

		this.gameObject.SetActive(true);
		ItemIcon.gameObject.SetActive(true);
		GradeBoard.gameObject.SetActive(true);

		ItemIcon.sprite = UICommon.GetItemIconSprite(CollectionItemTid);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(CollectionItemTid);

		bool isEnchanted = DBItem.GetItem(CollectionItemTid).Step > 0;
		GradeText.gameObject.SetActive(isEnchanted);
		if (isEnchanted)
			GradeText.text = string.Format("+{0}", DBItem.GetItem(CollectionItemTid).Step);

		CollectionCheck.gameObject.SetActive(ItemInvenCheck(CollectionItemTid));
		CollectionCheckIcon.gameObject.SetActive(ItemInvenCheck(CollectionItemTid));
	}

	private void SetBlock()
	{
		Block.gameObject.SetActive(CollectData == null || !CollectData.MaterialTids.Contains(CollectionItemTid));
	}

	private bool ItemInvenCheck(uint _itemTid)
	{
		for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
		{
			if (_itemTid == Me.CurCharData.InvenList[i].item_tid && (CollectData == null || !CollectData.MaterialTids.Contains(CollectionItemTid)))
				return true;
		}
		return false;
	}

	public void OnClickInfoPopup()
	{
		bool check = CollectData == null || !CollectData.MaterialTids.Contains(CollectionItemTid);
		UIManager.Instance.Find<UIFrameItemCollection>().OpenItemInfoPopup(CollectionItemTid, SlotIdx, ItemCollectionTable.ItemCollectionID, check);
	}
}

	
