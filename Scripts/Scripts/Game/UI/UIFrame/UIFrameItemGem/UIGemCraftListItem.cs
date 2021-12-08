using GameDB;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UIGemCraftListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Image GradeBoard;
	[SerializeField] private Text Name;
	#endregion

	#region System Variable
	[SerializeField] private Make_Table Item = null;
	#endregion

	public void Initialize(Make_Table _table)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		if (_table == null)
			return;

		Item = _table;

		Icon.sprite = UICommon.GetItemIconSprite(Item.SuccessGetItemID);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(Item.SuccessGetItemID);
		Name.text = DBLocale.GetText(DBItem.GetItem(Item.SuccessGetItemID).ItemTextID);
	}

	public void OnSelectGem()
	{
		if (UIManager.Instance.Find(out UIFrameItemGem _gem))
			_gem.SelectCraftGem(Item);
	}
}