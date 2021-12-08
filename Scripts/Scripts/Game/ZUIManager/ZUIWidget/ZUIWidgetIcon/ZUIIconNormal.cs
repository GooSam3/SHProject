using GameDB;
using UnityEngine;
using UnityEngine.UI;
public class ZUIIconNormal : CUGUIIconCheckBase
{
	private static UIPopupItemInfo mItemTooltip = null;
	//--------------------------------------------------------------
	public void DoUIIconSetting(Item_Table _itemInfo, uint _count)
	{
		Sprite iconSprite = ZManagerUIPreset.Instance.GetSprite(_itemInfo.IconID);
		Sprite gradeSprite = ExtractItemGradeImage(_itemInfo.Grade);
		ProtUIIconSettingSprite(iconSprite, gradeSprite, _itemInfo.ItemID);
		ProtUIIconSettingCount(_count);

		DoUIIconCheck(false);
		ProtUIIconSelect(false);
	}

	public void DoUIIconSelect(bool _select)
	{
		ProtUIIconSelect(_select);
	}
	//----------------------------------------------------------------
	protected override void OnUIIconPointUp(Vector2 _PointPosition)
	{
		base.OnUIIconPointUp(_PointPosition);

		if (pIconID == 0) return;

		ZPoolManager.Instance.Spawn<UIPopupItemInfo>(E_PoolType.UI, (_uiPopupItemInfo) =>
		{
			RemoveItemTooltip();
			mItemTooltip = _uiPopupItemInfo;
			_uiPopupItemInfo.transform.SetParent(mUIFrameParent.transform);
			_uiPopupItemInfo.gameObject.SetActive(true);
			_uiPopupItemInfo.Initialize(E_ItemPopupType.Reward, pIconID, () => {
				ZPoolManager.Instance.Return(_uiPopupItemInfo.gameObject);
			});
		}); 

	}

	protected override void OnUIWidgetFrameShowHide(bool _show)
	{
		base.OnUIWidgetFrameShowHide(_show);
		if (_show == false)
		{
			RemoveItemTooltip();
		}
	}

	protected override void OnUIWidgetRemove()
	{
		base.OnUIWidgetRemove();
		RemoveItemTooltip();
	}

	//----------------------------------------------------------------
	private Sprite ExtractItemGradeImage(uint _grade)
	{
		if (_grade == 0)
		{
			_grade = 1;
		}

		Sprite sprite = null;
		sprite = ZManagerUIPreset.Instance.GetSprite(string.Format("img_grade_{0:D2}", _grade));
		return sprite;
	}

	private void RemoveItemTooltip()
	{
		if (mItemTooltip != null)
		{
			ZPoolManager.Instance.Return(mItemTooltip.gameObject);
			mItemTooltip = null;
		}
	}
}
