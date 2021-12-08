using System.Text;
using UnityEngine;
using UnityEngine.U2D;

public class ZUIScrollMinimap : CUGUIScrollRectListBase
{
	private SpriteAtlas		mSpriteAtlas = null;
	private Sprite[]			mSpriteList = null;
	private string			mCurrentMapAtlasName = null;
	private StringBuilder		mNote = new StringBuilder();
	private bool				mMinimapLoad = false;

	private Vector2 mStageSize = Vector2.zero;
	private Vector2 mStageOffset = Vector2.zero;
	private Vector2 mStageRatePerMinimap = Vector2.zero;
	private Vector2 mStageScale = Vector2.zero;
	private Vector2 mMinimapSize = Vector2.zero;
	private Vector2 mMinimapSizeHalf = Vector2.zero;

	private Vector2 mViewPortSize = Vector2.zero;
	private Vector2 mViewPortSizeHalf = Vector2.zero;
	//---------------------------------------------------------------
	protected override void OnUIScrollRectListRefreshItem(int _index, CUGUIWidgetSlotItemBase _newItem)
	{
		base.OnUIScrollRectListRefreshItem(_index, _newItem);
		ZUIScrollMinimapItem minimapItem = _newItem as ZUIScrollMinimapItem;
		minimapItem.DoMinimapItemSprite(mSpriteList[_index], _index);
	}

	protected override void OnUIWidgetInitialize(CUIFrameBase _uIFrameParent)
	{
		base.OnUIWidgetInitialize(_uIFrameParent);
		mViewPortSizeHalf = mScrollRect.viewport.rect.size / 2f;
		mViewPortSize = mScrollRect.content.rect.size;
	}

	//-----------------------------------------------------------------
	public void DoMinimapSetTexture(string _addresaableName, Vector2 _stageSize, Vector2 _stageScale, Vector2 _stageOffset)
	{
		if (mCurrentMapAtlasName == _addresaableName) return;
		mCurrentMapAtlasName = _addresaableName;
		mStageSize = _stageSize;
		mStageOffset = _stageOffset;
		mStageScale = _stageScale;
		LoadMinimapAtlas(_addresaableName);
	}

	public void DoMinimapCenterPosition(Vector2 _worldPosition)
	{
		if (mMinimapLoad == false) return;

		Vector2 scrollPosition = ExtractPositionWorldToScroll(_worldPosition);
		scrollPosition.x += mViewPortSizeHalf.x;
		scrollPosition.y -= mViewPortSizeHalf.y;
		ProtUIScrollListSetPosition(scrollPosition);
	}

	public Vector2 ExtractPositionWorldToScroll(Vector2 _worldPosition)
	{
		Vector2 scrollPosition = _worldPosition;

		scrollPosition.x *= -1f; // 스크롤 Pivot이 0 , 1로 고정됨에 따라 x, y축을 뒤집어 준다. 
		scrollPosition.y = mStageSize.y - scrollPosition.y;
		scrollPosition *= mStageRatePerMinimap;

		scrollPosition = (mMinimapSizeHalf) - ((mMinimapSizeHalf - scrollPosition) * mStageScale);
		scrollPosition += mStageOffset;

		return scrollPosition;
	}

	public bool IsMinimapLoad()
	{
		return mMinimapLoad;
	}

	//----------------------------------------------------------------
	private void LoadMinimapAtlas(string _addressableName)
	{
		mMinimapLoad = false;

		if (_addressableName == null) return;

		ZResourceManager.Instance.Load(_addressableName, (string _loadedName, SpriteAtlas _loadedAtlas) =>
		{
			if (null == _loadedAtlas)
			{
				ZLog.LogWarn(ZLogChannel.UI, $"미니맵용 Atlas[{_addressableName}]가 존재하지 않습니다.");
				return;
			}

			mMinimapLoad = true;
			RefreshMinimapAtlas(_loadedAtlas);
		});
	}

	private void RefreshMinimapAtlas(SpriteAtlas _spriteAtlas)
	{
		RemoveSpriteAtlas();

		mSpriteAtlas = _spriteAtlas;
		int totalCount = mSpriteAtlas.spriteCount;
		mSpriteList = new Sprite[totalCount];
		string AtlasName = _spriteAtlas.name;
		for (int i = 0; i < totalCount; i++)
		{
			mNote.Clear();
			mNote.AppendFormat("{0}_{1}", AtlasName, i);
			Sprite loadSprite = _spriteAtlas.GetSprite(mNote.ToString());
			mSpriteList[i] = loadSprite;
		}

		ProtUIScrollListInitialize(totalCount, true);
		mMinimapSize = mScrollRect.content.rect.size;
		mMinimapSizeHalf = mMinimapSize / 2f;
		mMinimapSizeHalf.x *= -1;
		mStageRatePerMinimap = mMinimapSize / mStageSize;
	}

	private void RemoveSpriteAtlas()
	{
		if (mSpriteAtlas == null) return;

		for (int i = 0; i < mSpriteList.Length; i++)
		{
			DestroyImmediate(mSpriteList[i]);
		}
		mSpriteList = null;
		mSpriteAtlas = null;
	}
}
