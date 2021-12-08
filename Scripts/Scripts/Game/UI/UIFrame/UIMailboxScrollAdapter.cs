using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using ZDefine;
using ZNet.Data;

public class UIMailboxScrollAdapter : OSA<BaseParamsWithPrefab, UIMailboxMailListItem>
{
	// 스크롤 데이터 리스트
	public SimpleDataHelper<ScrollMailData> Data { get; private set; }

	/// <summary>
	/// 홀더를 생성 (Content 사이즈에 맞춰서 최대 수량을 계산해서 생성함)
	/// Init(오브젝트 본체, 스크롤뷰에 담을(Content)위치, itemIndex(오브젝트 인덱스)).
	/// </summary>
	/// <param name="itemIndex">생성하려는 Holder Index</param>
	protected override UIMailboxMailListItem CreateViewsHolder(int itemIndex)
	{
		var instance = new UIMailboxMailListItem();

		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

		return instance;
	}

	/// <summary>리스트 홀더를 갱신해준다.(스크롤이 Active 또는 Cell Group이 이동될 때(Drag))</summary>
	/// <param name="_holder">홀더 오브젝트</param>
	protected override void UpdateViewsHolder(UIMailboxMailListItem _holder)
	{
		ScrollMailData data = Data[_holder.ItemIndex];
		_holder.UpdateTitleByItemIndex(data);
	}

	/// <summary>스크롤 데이터 리스트에 아이템 추가 및 삭제 (자동으로 갱신)</summary>
	protected override void OnItemIndexChangedDueInsertOrRemove(UIMailboxMailListItem shiftedViewsHolder, int oldIndex, bool wasInsert, int removeOrInsertIndex)
	{
		base.OnItemIndexChangedDueInsertOrRemove(shiftedViewsHolder, oldIndex, wasInsert, removeOrInsertIndex);
		shiftedViewsHolder.UpdateTitleByItemIndex(Data[shiftedViewsHolder.ItemIndex]);
	}

	private void Initialize()
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollMailData>(this);
	}

	public void SetScrollData()
	{
		Initialize();

		#region 사용자 변경 로직
		for (int i = 0; i < Me.CurCharData.MailList.Count; i++)
		{
			if (Data.List.Find(item => item.MailData.MailIdx == Me.CurCharData.MailList[i].MailIdx) == null)
				Data.InsertOne(i, new ScrollMailData() { MailData = Me.CurCharData.MailList[i] });
		}
        #endregion
    }
}

///<summary>Scroll Item Define (Scroll 전용 사용자 정의 자료구조 선언)</summary>
public class ScrollMailData
{
	public MailData MailData;

	public void Reset(ScrollMailData _data)
	{
		MailData = _data.MailData;
	}
}