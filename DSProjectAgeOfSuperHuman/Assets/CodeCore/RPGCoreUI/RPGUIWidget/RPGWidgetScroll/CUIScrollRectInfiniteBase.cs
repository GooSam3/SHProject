using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// 2020.08 Zerogames  정구삼
// [개 요]1) 기존 ScrollRect를 서포트하여 인덱스 기반 무한 스크롤을 구현하였다.
//        2) 아이템 갯수와 상관 없이 뷰포트 크기만큼만 셀을 할당하고 스크롤 하므로 드로우콜과 마스킹 비용을 절감할수 있다. 
//        3) 가로 / 세로 스크롤을 지원한다.
// [셋 팅] GridLayoutGroup 컴포넌트를 사용 / mGrid.constraint == GridLayoutGroup.Constraint.FixedColumnCount : 가로세로 스크롤 제공 
// 또한 ContentSizeFitter를 사용하면 안된다 (본 컴포넌트가 동적으로 변경해서 사용한다)
// ScrollRect.content는 Stretch 로 셋팅 되어야 한다.(Viewport 크기로 맞춰야 한다) 

abstract public class CUIScrollRectInfiniteBase : CUIScrollRectBase
{
	private GridLayoutGroup mGrid = null;
	private int mViewPortCellWidth = 0;                 //뷰포트 내부에 표시될 셀 수
	private int mViewPortCellHeight = 0;

	private int mGridCellWidth = 0;                     // 전체 스크롤바의 가로세로 셀 수  
	private int mGridCellHeight = 0;

	private int mCellScrollSizeX = 0;                   // 스크롤시 Panding Size;
	private int mCellScrollSizeY = 0;

	private int mContrainCountOrigin = 0;
	private int mTotalCount = 0;                        // 출력될 전체 셀 갯수 	
	private int mTotalViewCount = 0;                    // 뷰포트에 출력될 셀 갯수 : 이 크기만큼 인스턴스가 할당된다.
	private Vector2Int mScrollIndex = Vector2Int.zero;  // 죄상단 기준 스크롤 기준점. 셀 단위이다. 

	private List<CUIWidgetTemplateItemBase> m_listViewPortItem = new List<CUIWidgetTemplateItemBase>();
	//----------------------------------------------------------------
	protected override void OnUIWidgetInitialize(CUIFrameBase _uIFrameParent)
	{
		base.OnUIWidgetInitialize(_uIFrameParent);
		mGrid = GetComponentInChildren<GridLayoutGroup>();
		mContrainCountOrigin = mGrid.constraintCount;
		mScrollRect.movementType = ScrollRect.MovementType.Clamped;  // 나머지 스크롤은 계산에 맞지 않음	
		mGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		mScrollRect.content.pivot = new Vector2(0f, 1f); // 좌표 산출기준 좌상단
	}

	protected override void OnUIScrollValueChange(Vector2 _changeValue)
	{
		base.OnUIScrollValueChange(_changeValue);
	}

	protected void Update()
	{
		UpdateUIScrollInfinite();
	}

	//-----------------------------------------------------------------
	protected void ProtUIScrollInfiniteInitialize(int _totalCount, bool _scrollReset = true)
	{
		mTotalCount = _totalCount;
		Vector2Int ViewportSize = Vector2Int.zero;
		Vector2 ContentsPosition = mContentTransform.anchoredPosition;
		ViewportSize.x = Mathf.Abs((int)mScrollRect.viewport.rect.width);
		ViewportSize.y = Mathf.Abs((int)mScrollRect.viewport.rect.height);
		
		PrivUIScrollInfiniteVerticalSize(mGrid.cellSize, mGrid.spacing, ViewportSize, mContrainCountOrigin, _totalCount);
		PrivUIScrollInfiniteRefreshAll();	

		if (_scrollReset == false)
		{
			mContentTransform.anchoredPosition = ContentsPosition;
			Update();
		}
		mScrollRect.velocity = Vector2.zero;
	}

	protected void ProtUIScrollInfiniteSetPosition(int _targetIndex)
    {		
		int PosX = _targetIndex % mViewPortCellWidth;
		int PosY = _targetIndex / mViewPortCellWidth;

		if (PosX + mViewPortCellWidth >= mGridCellWidth)
        {
			PosX = mGridCellWidth - mViewPortCellWidth;
        }

		if (PosY + mViewPortCellHeight >= mGridCellHeight)
        {
			PosY = mGridCellHeight - mViewPortCellHeight + 2;
        }

		int PosXContent = PosX * mCellScrollSizeX;
		int PosYContent = PosY * mCellScrollSizeY;

		mScrollRect.content.anchoredPosition = new Vector2(PosXContent, PosYContent);
		UpdateUIScrollInfinite();
	}
	/// <summary>
	/// // 미니맵등 외부에서 스크롤 좌표를 입력할때
	/// </summary>
	protected void ProtUIScrollInfinitSetPosition(Vector2 _position) 
	{
		float positionX = Mathf.Clamp(_position.x, -mScrollRect.content.sizeDelta.x, 0);
		float positionY = Mathf.Clamp(_position.y, 0, mScrollRect.content.sizeDelta.y);

		mScrollRect.content.anchoredPosition = new Vector2(positionX, positionY);
		UpdateUIScrollInfinite();
	}
	
	//------------------------------------------------------------------
	private void UpdateUIScrollInfinite()
	{
		Vector2Int ScrollIndexCurrent = ExtractUIScrollInfiniteCenterIndex();
		Vector2Int ScrollIndex = mScrollIndex - ScrollIndexCurrent;
		bool refresh = false;
		if (ScrollIndex.x != 0)
		{
			refresh = true;
			PrivUIScrollInfiniteRefreshWidth(ScrollIndexCurrent, ScrollIndex.x);
		}

		if (ScrollIndex.y != 0)
		{
			refresh = true;
			PrivUIScrollInfiniteRefreshHeight(ScrollIndexCurrent, ScrollIndex.y);
		}

		if (refresh)
		{
			PrivUIScrollInfiniteInvalidate();
		}

		mScrollIndex = ScrollIndexCurrent;

		OnUIScrollInifiniteUpdate();
	}

	private void PrivUIScrollInfiniteRefreshAll()
	{
		PrivUIScrollInfiniteDetachAllContentsChild();
		PrivUIScrollInfiniteMakeClipingScale();
		PrivUIScrollInfiniteRefreshIndex();
		PrivUIScrollInfiniteContents();
	}

	private void PrivUIScrollInfiniteVerticalSize(Vector2 _cellSize, Vector3 _spacing, Vector2Int _viewPortSize, int _constrainCount, int _totalCount)
	{
		int SpacingCountX = _constrainCount - 1;
		if (SpacingCountX < 0) SpacingCountX = 0;

		int ContentsWidth = ((int)_cellSize.x * _constrainCount) + ((int)_spacing.x * (SpacingCountX)) - _viewPortSize.x;
		int ContentsHeightCount = _totalCount / _constrainCount;
		if (_totalCount % _constrainCount > 0)
		{
			ContentsHeightCount++;
		}

		int SpacingCountY = ContentsHeightCount - 1;
		if (SpacingCountY < 0) SpacingCountY = 0;

		int ContentsHeight = ((int)_cellSize.y * ContentsHeightCount) + ((int)_spacing.y * (SpacingCountY)) - _viewPortSize.y;

		int ViewPortCellWidth = (int)(_viewPortSize.x / _cellSize.x) + 1;    // 위 아래 양 옆에 한칸씩 여분을 만들어야 한다. 
		int ViewPortCellHeight = (int)(_viewPortSize.y / _cellSize.y) + 1;  

		int RestX = _viewPortSize.x - (int)(_spacing.x * ((int)(_viewPortSize.x / _cellSize.x) - 1)); 
		int RestY = _viewPortSize.y - (int)(_spacing.y * ((int)(_viewPortSize.y / _cellSize.y) - 1));

		RestX = RestX % (int)_cellSize.x;
		RestY = RestY % (int)_cellSize.y;

		if (RestX > 0) RestX = 1;
		if (RestY > 0) RestY = 1;

		mViewPortCellWidth = ViewPortCellWidth + RestX;
		mViewPortCellHeight = ViewPortCellHeight + RestY;

		mGridCellWidth = _constrainCount;
		mGridCellHeight = ContentsHeightCount;

		if (mViewPortCellWidth > mGridCellWidth) mViewPortCellWidth = mGridCellWidth;
		if (mViewPortCellHeight > mGridCellHeight) mViewPortCellHeight = mGridCellHeight;

		mCellScrollSizeX = (int)(mGrid.cellSize.x + mGrid.spacing.x);
		mCellScrollSizeY = (int)(mGrid.cellSize.y + mGrid.spacing.y);

		mScrollRect.content.sizeDelta = new Vector2(ContentsWidth, ContentsHeight);
	}

	private void PrivUIScrollInfiniteDetachAllContentsChild()
	{
		for (int i = 0; i < m_listViewPortItem.Count; i++)
		{
			DoTemplateReturn(m_listViewPortItem[i]);
		}
		m_listViewPortItem.Clear();

		mGrid.padding.left = 0;
		mGrid.padding.right = 0;
		mGrid.padding.top = 0;
		mGrid.padding.bottom = 0;
	}

	private void PrivUIScrollInfiniteMakeClipingScale()
	{
		mTotalViewCount = mViewPortCellWidth * mViewPortCellHeight;
		mGrid.constraintCount = mViewPortCellWidth;
	}

	private Vector2Int ExtractUIScrollInfiniteCenterIndex()
	{
		Vector2Int StartIndex = Vector2Int.zero;
		StartIndex.x = (int)((mScrollRect.content.anchoredPosition.x) / (mGrid.cellSize.x + (mGrid.spacing.x)));
		StartIndex.y = (int)((mScrollRect.content.anchoredPosition.y) / (mGrid.cellSize.y + (mGrid.spacing.y)));

		StartIndex.x = Mathf.Abs(StartIndex.x);
		StartIndex.y = Mathf.Abs(StartIndex.y);

		if (StartIndex.x > mGridCellWidth - mViewPortCellWidth)
		{
			StartIndex.x = mGridCellWidth - mViewPortCellWidth;
		}

		if (StartIndex.y > mGridCellHeight - mViewPortCellHeight)
		{
			StartIndex.y = mGridCellHeight - mViewPortCellHeight;
		}

		int PaddingX = StartIndex.x * mCellScrollSizeX;
		int PaddingY = StartIndex.y * mCellScrollSizeY;

		mGrid.padding.left = PaddingX;
		mGrid.padding.top = PaddingY;

		return StartIndex;
	}

	private void PrivUIScrollInfiniteRefreshIndex()
	{
		for (int h = 0; h < mViewPortCellHeight; h++)
		{
			for (int w = 0; w < mViewPortCellWidth; w++)
			{
				int IndexX = w;
				if (IndexX < mGridCellWidth)
				{
					int IndexY = h;
					if (IndexY < mGridCellHeight)
					{
						int IndexCell = (IndexY * mGridCellWidth) + IndexX;
						PrivGridLayoutGroupCreatItem(IndexCell);
					}
				}
			}
		}
	}

	private void PrivUIScrollInfiniteContents()
	{
		mScrollRect.content.anchoredPosition = Vector2.zero;
		mScrollIndex = Vector2Int.zero;
	}

	private void PrivUIScrollInfiniteRefreshWidth(Vector2Int _startIndex, int _scrollX)
	{
		int ScrollXTotal = Mathf.Abs(_scrollX);
		Vector2Int ScrollStartIndex = _startIndex;

		for (int i = 0; i < ScrollXTotal; i++)
		{
			if (_scrollX < 0)
			{
				ScrollStartIndex.x = _startIndex.x - ((ScrollXTotal - 1) - i);
				PrivUIScrollInfiniteRefreshChildItem();
				PrivUIScrollInfiniteSwapRight(ScrollStartIndex);
			}
			else
			{
				ScrollStartIndex.x = _startIndex.x + ((ScrollXTotal - 1) - i);
				PrivUIScrollInfiniteRefreshChildItem();
				PrivUIScrollInfiniteSwapLeft(ScrollStartIndex);
			}
		}
	}

	private void PrivUIScrollInfiniteRefreshHeight(Vector2Int _startIndex, int _scrollY)
	{
		int ScrollYTotal = Mathf.Abs(_scrollY);
		Vector2Int ScrollStartIndex = _startIndex;

		for (int i = 0; i < ScrollYTotal; i++)
		{
			if (_scrollY < 0)
			{
				ScrollStartIndex.y = _startIndex.y - ((ScrollYTotal - 1) - i);
				PrivUIScrollInfiniteRefreshChildItem();
				PrivUIScrollInfiniteSwapBottom(ScrollStartIndex);
			}
			else
			{
				ScrollStartIndex.y = _startIndex.y + ((ScrollYTotal - 1) - i);
				PrivUIScrollInfiniteRefreshChildItem();
				PrivUIScrollInfiniteSwapTop(ScrollStartIndex);
			}
		}
	}

	private void PrivUIScrollInfiniteSwapRight(Vector2Int _startIndex)
	{
		int IndexOff = 0;
		int IndexOn = 0;

		for (int i = 0; i < mViewPortCellHeight; i++)
		{
			IndexOff = i * mViewPortCellWidth;
			IndexOn = IndexOff + mViewPortCellWidth - 1;

			CUIWidgetTemplateItemBase Item = m_listViewPortItem[IndexOff];
			int ItemSlotIndex = _startIndex.x + (mViewPortCellWidth - 1) + (i* mGridCellWidth) + (_startIndex.y  * mGridCellWidth);

			Item.transform.SetSiblingIndex(IndexOn);
			if (ItemSlotIndex >= mTotalCount)
			{
				Item.DoWTemplatItemShow(false);
			}
			else
			{
				PrivUIScrollInfiniteRefreshItem(ItemSlotIndex, Item);
			}
		}
	}

	private void PrivUIScrollInfiniteSwapLeft(Vector2Int _startIndex)
	{
		int IndexOff = 0;
		int IndexOn = 0;
		
		for (int i = 0; i < mViewPortCellHeight; i++)
		{
			IndexOn = i * mViewPortCellWidth;
			IndexOff = IndexOn + mViewPortCellWidth - 1;

			CUIWidgetTemplateItemBase Item = m_listViewPortItem[IndexOff];
			int ItemSlotIndex = _startIndex.x + ( i * mGridCellWidth) + (_startIndex.y * mGridCellWidth);

			Item.transform.SetSiblingIndex(IndexOn);

			if (ItemSlotIndex >= mTotalCount)
			{
				Item.DoWTemplatItemShow(false);
			}
			else
			{
				PrivUIScrollInfiniteRefreshItem(ItemSlotIndex, Item);
			}
		}
	}

	private void PrivUIScrollInfiniteSwapTop(Vector2Int _startIndex)
	{
		int IndexOff = 0;
		int IndexOn = 0;

		for (int i = 0; i < mViewPortCellWidth; i++)
		{
			IndexOn = i;
			IndexOff = IndexOn + ((mViewPortCellHeight - 1) * mViewPortCellWidth);

			CUIWidgetTemplateItemBase Item = m_listViewPortItem[IndexOff];
			int ItemSlotIndex = i + (_startIndex.y * mGridCellWidth) + _startIndex.x;
			Item.transform.SetSiblingIndex(IndexOn);
			if (ItemSlotIndex >= mTotalCount)
			{
				Item.DoWTemplatItemShow(false);			
			}
			else
			{
				PrivUIScrollInfiniteRefreshItem(ItemSlotIndex, Item);
			}
		}
	}

	private void PrivUIScrollInfiniteSwapBottom(Vector2Int _startIndex)
	{
		int IndexOff = 0;
		int IndexOn = 0;

		for (int i = 0; i < mViewPortCellWidth; i++)
		{
			IndexOff = i;
			IndexOn = IndexOff + (mViewPortCellHeight * mViewPortCellWidth);

			CUIWidgetTemplateItemBase Item = m_listViewPortItem[IndexOff];
			int ItemSlotIndex = (_startIndex.x + i ) + (mGridCellWidth * (mViewPortCellHeight + _startIndex.y - 1));

			Item.transform.SetSiblingIndex(IndexOn);

			if (ItemSlotIndex >= mTotalCount)
			{
				Item.DoWTemplatItemShow(false);
			}
			else
			{
				PrivUIScrollInfiniteRefreshItem(ItemSlotIndex, Item);
			}
		}
	}

	private void PrivUIScrollInfiniteRefreshChildItem()
	{
		m_listViewPortItem.Clear();
		GetComponentsInChildren(true, m_listViewPortItem);
	}

	private void PrivUIScrollInfiniteInvalidate()
	{
		PrivUIScrollInfiniteArrange();
		LayoutRebuilder.MarkLayoutForRebuild(mScrollRect.content);
	}

	private void PrivUIScrollInfiniteRefreshItem(int _itemIndex, CUIWidgetTemplateItemBase _item)
	{
		if (_itemIndex >= mTotalCount || _itemIndex < 0) return;
		
		_item.DoWTemplatItemShow(true);
		_item.DoTemplateItemRefresh(_itemIndex);
		OnUIScrollInifiniteRefreshItem(_itemIndex, _item);
	}

	private void PrivUIScrollInfiniteArrange()
	{
		if (mScrollRect.content.childCount <= 0) return;

		int StartIndex = 0;
		for (int H = 0; H < mViewPortCellHeight; H++)
		{
			for (int W = 0; W < mViewPortCellWidth; W++)
			{
				int itemIndex = (H * mViewPortCellWidth) + W;
				CUIWidgetTemplateItemBase item = mScrollRect.content.GetChild(itemIndex).GetComponent<CUIWidgetTemplateItemBase>();
				int SlotIndex = item.GetTemplateItemIndex();
				if (W == 0 && H == 0)
				{
					StartIndex = SlotIndex;  
					continue; 
				}

				int nextSlotIndex = StartIndex + W + (H * mGridCellWidth);
				if (SlotIndex != nextSlotIndex)
				{
					PrivUIScrollInfiniteRefreshItem(nextSlotIndex, item);
				}
			}
		}		
	}

	private void PrivGridLayoutGroupCreatItem(int _index)
	{
		if (m_listViewPortItem.Count >= mTotalViewCount) return;

		CUIWidgetTemplateItemBase item = DoTemplateRequestItem();
		m_listViewPortItem.Add(item);
		item.DoTemplateItemRefresh(_index);

		if (_index >= mTotalCount)
		{
			item.DoWTemplatItemShow(false);
		}
		else
        {
			PrivUIScrollInfiniteRefreshItem(_index, item);
		}
	}	
	//-----------------------------------------------------------------
	protected virtual void OnUIScrollInifiniteRefreshItem(int _index, CUIWidgetTemplateItemBase _newItem) {}
	protected virtual void OnUIScrollInifiniteUpdate() { }

}
