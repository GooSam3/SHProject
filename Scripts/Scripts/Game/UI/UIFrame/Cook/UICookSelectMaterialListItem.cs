using GameDB;
using UnityEngine;
using UnityEngine.UI;
using ZDefine;

public class UICookSelectMaterialListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Image Icon;
	[SerializeField] private Image GradeBoard;
	[SerializeField] private Text Name;
	[SerializeField] private GameObject PlusObj;
	[SerializeField] private GameObject MaterialObj;
	#endregion

	#region System Variable
	public ZItem Item = null;
	#endregion

	public void Initialize(ZItem _item)
	{
		//transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		Icon.gameObject.SetActive(_item != null);
		GradeBoard.gameObject.SetActive(_item != null);
		PlusObj.SetActive(_item == null);
		MaterialObj.SetActive(_item != null);

		Item = _item;

		if (_item == null)
		{
			Name.text = string.Empty;
			return;
		}

		Icon.sprite = UICommon.GetItemIconSprite(Item.item_tid);
		GradeBoard.sprite = UICommon.GetItemGradeSprite(Item.item_tid);
		Name.text = DBLocale.GetText(DBItem.GetItem(Item.item_tid).ItemTextID);
	}

	public void OnDeselectMaterial()
	{
		if (UIManager.Instance.Find(out UIFrameCook _cook))
			_cook.OnDeselectCombineCook(Item);
	}

	public void OnSelect()
	{

	}
}