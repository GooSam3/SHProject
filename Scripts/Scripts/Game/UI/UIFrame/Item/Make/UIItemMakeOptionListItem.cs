using UnityEngine;
using UnityEngine.UI;

public class UIItemMakeOptionListItem : MonoBehaviour
{
	#region UI Variable
	[SerializeField] private Text Title;
	[SerializeField] private Text Option;
	#endregion

	#region System Variable
	#endregion

	public void Initialize(string _title, string _option)
	{
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector2.one;

		Title.text = _title;
		Option.text = _option;
	}
}