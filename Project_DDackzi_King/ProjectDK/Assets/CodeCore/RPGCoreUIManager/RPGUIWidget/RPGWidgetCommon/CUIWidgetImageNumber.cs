using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CUIWidgetImageNumber : CUIWidgetTemplateItemBase
{
	[SerializeField]
	private List<Sprite> NumberSprite = new List<Sprite>();
	[SerializeField]
	private List<CImage> NumberImage = new List<CImage>();
	private Vector3 m_vecOrigin = Vector3.zero;
	private int m_iMaxDigit = 0;

	//-----------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase pParentFrame)
	{
		base.OnUIWidgetInitialize(pParentFrame);
		m_iMaxDigit = NumberImage.Count;
		PrivImageNumberReset();
	}

	private void Update()
	{
		PrivImageNumberUpdatePosition();
	}

	//-------------------------------------------------------------
	public void DoImageNumber(List<uint> pListNumber, Vector3 vecWorldPosition)
	{
		m_vecOrigin = vecWorldPosition;
		PrivImageNumberUpdatePosition();
		PrivImageNumberReset();
		for (int i = 0; i < pListNumber.Count; i++)
		{
			int iSpriteIndex = pListNumber.Count - i - 1;
			int iImageIndex = (int)pListNumber[i];

			if (iSpriteIndex >= NumberImage.Count) continue;
			if (iImageIndex >= NumberSprite.Count) continue;

			Sprite pSprite = NumberSprite[iImageIndex];
			CImage pImage = NumberImage[iSpriteIndex];
			pImage.sprite = pSprite;
			pImage.SetNativeSize();
			pImage.gameObject.SetActive(true);
		}
	}

	public void HandleImageNumberFinish()
	{
		DoWidgetItemReturn();
	}

	//--------------------------------------------------------------
	private void PrivImageNumberReset()
	{
		for (int i = 0; i < NumberImage.Count; i++)
		{
			NumberImage[i].gameObject.SetActive(false);
		}
	}

	private void PrivImageNumberUpdatePosition()
	{
		Vector3 vecScreenPosition = Camera.main.WorldToScreenPoint(m_vecOrigin);
		SetUIPosition(vecScreenPosition.x, vecScreenPosition.y);
	}
}
