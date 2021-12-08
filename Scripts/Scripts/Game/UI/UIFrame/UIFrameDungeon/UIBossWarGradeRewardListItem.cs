using System;
using UnityEngine;
using UnityEngine.UI;

public class UIBossWarGradeRewardListItem : MonoBehaviour
{
	uint itemTid;

	[SerializeField] private Image background;
	[SerializeField] private Image itemIcon;
	[SerializeField] private Text itemCount;
	[SerializeField] private Text itemEnchantCount;

	private const string FORMAT_GRADE_BG = "img_grade_0{0}";
	private const string FORMAT_ENCHANT_STEP = "+{0}";

	private Action<uint> selectEvent = null;

	public void DoUpdate(uint _itemTid, uint count, Action<uint> itemEvent)
	{
		itemIcon.gameObject.SetActive(true);

		if (DBItem.GetItem(_itemTid, out GameDB.Item_Table table) == false)
		{
			itemIcon.sprite = null;
			itemIcon.color = Color.magenta;
			return;
		}

		itemIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);

		bool isUseGradeColor = table.Grade > 0;

		background.gameObject.SetActive(isUseGradeColor);
		
		if(isUseGradeColor)
		{
			background.sprite = ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, table.Grade));
		}

		bool isStackItem = table.ItemStackType == GameDB.E_ItemStackType.Stack || table.ItemStackType == GameDB.E_ItemStackType.AccountStack;

		itemCount.gameObject.SetActive(isStackItem || count > 1);

		if(isStackItem)
		{
			string countStr = count.ToString();

			if(count == 0)
			{
				countStr = string.Empty;
			}

			itemCount.text = countStr;
		}

		bool isEnchanted = table.Step > 1;

		itemEnchantCount.gameObject.SetActive(isEnchanted);
		if (isEnchanted)
			itemEnchantCount.text = string.Format(FORMAT_ENCHANT_STEP, table.Step);

		itemTid = _itemTid;

		if (selectEvent == null)
		{
			selectEvent = itemEvent;
		}
	}

	public void UIRewardItemClick()
	{
		ZLog.Log(ZLogChannel.Default, $"UIListItemClick {itemTid}");
		if (selectEvent != null)
		{
			selectEvent(itemTid);
		}
	}
}
