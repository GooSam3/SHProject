using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIFrameDialogChoiseItem : MonoBehaviour
{
	[SerializeField]
	private Text TextDesc;

	[ReadOnly]
	[SerializeField]
	private int ChoiseNum;

	private Action<int> mEventChoise;

	/// <summary> 일반 다이얼로그 출력 </summary>
	public void Set(string descLocaleId, int index, Action<int> onClickChoise)
	{
		mEventChoise = onClickChoise;
		ChoiseNum = index;
		TextDesc.text = DBLocale.GetText(descLocaleId);
	}

	public void OnClick()
	{
		mEventChoise?.Invoke(ChoiseNum);
	}
}
