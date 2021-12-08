using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UIItemMakeSubTabListItem : MonoBehaviour
{
	#region UI Variable
	public Image Bg;
	[SerializeField] private Text Name;
	#endregion

	#region System Variable
	public E_MakeTapType Type = E_MakeTapType.Weapon_Knight;
	#endregion

	public void Initialize(E_MakeTapType _type)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		Type = _type;
		Name.text = DBLocale.GetText(_type.ToString());
	}

	public void OnSubMenu()
	{
		if (UIManager.Instance.Find(out UIFrameItemMake _make))
			_make.OnSelectSubTab(Type);
	}
}