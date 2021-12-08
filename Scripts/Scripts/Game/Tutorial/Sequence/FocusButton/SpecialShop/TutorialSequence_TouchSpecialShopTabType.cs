using GameDB;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 스페셜 상점 ui에서 해당 탭 타입을 선택한다. </summary>
public class TutorialSequence_TouchSpecialShopTabType : TutorialSequence_FocusButton<UIFrameSpecialShop>
{
	/// <summary> 해당 ui에서 버튼을 찾는데 사용할 경로 </summary>
	protected override string Path { get { return ""; } }

	private E_SpecialShopType ShopType;

	protected override Selectable GetSelectable()
	{
		if (null == OwnerUI.MainCtgUIs)
			return null;

		var categoryType = OwnerUI.MainCtgUIs.Find((item) => { return item.Key == ShopType; });

		if (null == categoryType || false == categoryType.gameObject.activeInHierarchy)
			return null;

		return categoryType.gameObject.GetComponent<Selectable>();
	}


	protected override Transform GetHighlightObject()
	{
		return mSelectable.transform;
	}

	protected override bool Check()
	{
		return true;
	}

	protected override void SetParams(List<string> args)
	{
		if (false == Enum.TryParse(args[0], true, out ShopType))
			ZLog.LogError(ZLogChannel.Quest, "E_SpecialShopType 이 아니다. 셋팅 확인 바람");
	}
}