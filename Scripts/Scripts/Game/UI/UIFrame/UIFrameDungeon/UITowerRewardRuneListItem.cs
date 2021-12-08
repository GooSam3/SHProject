using UnityEngine;
using UnityEngine.UI;
using ZDefine;
using ZNet.Data;

public class UITowerRewardRuneListItem : MonoBehaviour
{
	private const string FORMAT_GRADE_BG = "img_grade_0{0}";
	private const string FORMAT_GRADE_NUM = "+{0}";
	private const string FORMAT_GRADE_STAR = "icon_star_set_0{0}";

	[SerializeField] private Image background;
	[SerializeField] private Image itemIcon;

	[SerializeField] private ZImage gradeImage;
	[SerializeField] private Text gradeText;

	[SerializeField] private GameObject objSelected;
	[SerializeField] private GameObject objAlarmDot;
	[SerializeField] private GameObject objEquiped;

	private uint itemId;

	public void DoUpdate(GameDB.Item_Table itemTable)
	{
		itemId = itemTable.ItemID;

		SetItem(itemTable.ItemID, 1);
	}

	public void UIRewardItemClick()
	{
		ZPoolManager.Instance.Spawn(E_PoolType.UI, nameof(UIPopupItemInfo), (obj) => {
			UIPopupItemInfo popupItemInfo = obj.GetComponent<UIPopupItemInfo>();

			if (popupItemInfo != null) {
				var frameDungeon = UIManager.Instance.Find<UIFrameDungeon>();
				frameDungeon.SetInfoPopup(popupItemInfo);
				popupItemInfo.transform.SetParent(frameDungeon.transform);
				popupItemInfo.Initialize(E_ItemPopupType.Reward, itemId);
			}
		});
	}

	private void SetItem(uint tid, ulong count)
	{
		itemIcon.gameObject.SetActive(true);
		if (DBItem.GetItem(tid, out GameDB.Item_Table table) == false) {
			itemIcon.sprite = null;
			itemIcon.color = Color.magenta;
			return;
		}

		// 아이콘
		itemIcon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);

		// 등급
		bool isUseGradeColor = table.Grade > 0;
		background.gameObject.SetActive(isUseGradeColor);
		gradeText.gameObject.SetActive(isUseGradeColor);
		gradeImage.gameObject.SetActive(isUseGradeColor);

		if (isUseGradeColor) {
			background.sprite = ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, table.Grade));
			gradeText.text = string.Format(FORMAT_GRADE_NUM, table.Grade);
			Sprite questIcon = ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_STAR, table.Grade));
			if (questIcon != null) {
				gradeImage.sprite = questIcon;
			}
		}

		// 잠금여부
		ZItem item = Me.CurCharData.GetItem(tid);
		bool isLock = false;
		if (item != null)
			isLock = item.IsLock;

		objSelected.SetActive(false);
		objAlarmDot.SetActive(false);
		objEquiped.SetActive(false);
	}
}
