using UnityEngine;

// 스크롤바 - 컨텐츠 패널을 따라 다닌다.
// 컨텐츠에 종속적인 객체의 레이어 문제를 해결하기 위하 고안되었다.

public class CUIContentsFollow : CUIWidgetBase
{
	[SerializeField]
	private RectTransform Follow;


	//------------------------------------------------------
	protected virtual void Update()
	{
		if (Follow)
		{
			UpdateFollowCopy();
		}
	}

	//--------------------------------------------------------
	private void UpdateFollowCopy()
	{
		RectTransform MyTransform = transform as RectTransform;
		MyTransform.sizeDelta = Follow.sizeDelta;
		MyTransform.offsetMin = Follow.offsetMin;
		MyTransform.offsetMax = Follow.offsetMax;
		MyTransform.anchorMin = Follow.anchorMin;
		MyTransform.anchorMax = Follow.anchorMax;
		MyTransform.anchoredPosition = Follow.anchoredPosition;
		MyTransform.pivot = Follow.pivot;
	}
}
