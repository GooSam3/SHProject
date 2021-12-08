using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using frame8.Logic.Misc.Other.Extensions;
using GameDB;
using System;
using System.Collections.Generic;

public class UICharDetailInfoScrollAdapter : OSA<BaseParamsWithPrefab, UICharacterDetailStatHolder>
{
	public SimpleDataHelper<ScrollCharacterDetailStatData> Data
	{
		get; private set;
	}

	protected override UICharacterDetailStatHolder CreateViewsHolder(int itemIndex)
	{
		var instance = new UICharacterDetailStatHolder();
		instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);
		return instance;
	}

	protected override void UpdateViewsHolder(UICharacterDetailStatHolder newOrRecycled)
	{
		ScrollCharacterDetailStatData model = Data[newOrRecycled.ItemIndex];
		newOrRecycled.UpdateViews(model);
	}

	public void RefreshData()
	{
		for (int i = 0; i < base.GetItemsCount(); i++)
		{
			if (GetItemViewsHolder(i) != null)
				UpdateViewsHolder(GetItemViewsHolder(i));
		}
	}

	private void Initialize()
	{
		if (Data == null)
			Data = new SimpleDataHelper<ScrollCharacterDetailStatData>(this);
	}

	private void ClearData()
	{
		Data.List.Clear();
	}

	public void SetScrollData(ZPawnMyPc _myEntity, Action _callback = null)
	{
		Initialize();

		ClearData();

		#region 사용자 변경 로직
		List<ScrollCharacterDetailStatData> list = new List<ScrollCharacterDetailStatData>();
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_HP,Title = DBLocale.GetText("FINAL_MAX_HP"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_HP)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_MP,Title = DBLocale.GetText("FINAL_MAX_MP"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_MP)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_SHORT_ATTACK,Title = DBLocale.GetText("FINAL_MAX_SHORT_ATTACK"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_SHORT_ATTACK)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_LONG_ATTACK,Title = DBLocale.GetText("FINAL_MAX_LONG_ATTACK"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_LONG_ATTACK)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_MAGIC_ATTACK,Title = DBLocale.GetText("FINAL_MAX_MAGIC_ATTACK"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_MAGIC_ATTACK)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_ACCURACY,Title = DBLocale.GetText("FINAL_SHORT_ACCURACY"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_ACCURACY)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_ACCURACY,Title = DBLocale.GetText("FINAL_LONG_ACCURACY"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_ACCURACY)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_ACCURACY,Title = DBLocale.GetText("FINAL_MAGIC_ACCURACY"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_ACCURACY)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MELEE_DEFENCE,Title = DBLocale.GetText("FINAL_MELEE_DEFENCE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MELEE_DEFENCE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_DEFENCE,Title = DBLocale.GetText("FINAL_MAGIC_DEFENCE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_DEFENCE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_CRITICAL,Title = DBLocale.GetText("FINAL_SHORT_CRITICAL"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_CRITICAL)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_CRITICAL,Title = DBLocale.GetText("FINAL_LONG_CRITICAL"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_CRITICAL)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_CRITICAL,Title = DBLocale.GetText("FINAL_MAGIC_CRITICAL"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_CRITICAL)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_CRITICAL_MINUS,Title = DBLocale.GetText("FINAL_SHORT_CRITICAL_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_CRITICAL_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_CRITICAL_MINUS,Title = DBLocale.GetText("FINAL_LONG_CRITICAL_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_CRITICAL_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_CRITICAL_MINUS,Title = DBLocale.GetText("FINAL_MAGIC_CRITICAL_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_CRITICAL_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_CRITICALDAMAGE,Title = DBLocale.GetText("FINAL_SHORT_CRITICALDAMAGE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_CRITICALDAMAGE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_CRITICALDAMAGE,Title = DBLocale.GetText("FINAL_LONG_CRITICALDAMAGE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_CRITICALDAMAGE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_CRITICALDAMAGE,Title = DBLocale.GetText("FINAL_MAGIC_CRITICALDAMAGE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_CRITICALDAMAGE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_CRITICALDAMAGE_MINUS,Title = DBLocale.GetText("FINAL_SHORT_CRITICALDAMAGE_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_CRITICALDAMAGE_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_CRITICALDAMAGE_MINUS,Title = DBLocale.GetText("FINAL_LONG_CRITICALDAMAGE_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_CRITICALDAMAGE_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_CRITICALDAMAGE_MINUS,Title = DBLocale.GetText("FINAL_MAGIC_CRITICALDAMAGE_MINUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_CRITICALDAMAGE_MINUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_REDUCTION, Title = DBLocale.GetText("FINAL_SHORT_REDUCTION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_REDUCTION) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_REDUCTION,Title = DBLocale.GetText("FINAL_LONG_REDUCTION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_REDUCTION) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_REDUCTION,Title = DBLocale.GetText("FINAL_MAGIC_REDUCTION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_REDUCTION)});
		
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_REDUCTION_IGNORE, Title = DBLocale.GetText("FINAL_SHORT_REDUCTION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_REDUCTION_IGNORE) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_REDUCTION_IGNORE, Title = DBLocale.GetText("FINAL_LONG_REDUCTION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_REDUCTION_IGNORE) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_REDUCTION_IGNORE,Title = DBLocale.GetText("FINAL_MAGIC_REDUCTION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_REDUCTION_IGNORE)});

		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_EVASION,Title = DBLocale.GetText("FINAL_SHORT_EVASION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_EVASION)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_EVASION,Title = DBLocale.GetText("FINAL_LONG_EVASION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_EVASION)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_EVASION,Title = DBLocale.GetText("FINAL_MAGIC_EVASION"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_EVASION)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_SHORT_EVASION_IGNORE,Title = DBLocale.GetText("FINAL_SHORT_EVASION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_SHORT_EVASION_IGNORE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_LONG_EVASION_IGNORE,Title = DBLocale.GetText("FINAL_LONG_EVASION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_LONG_EVASION_IGNORE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAGIC_EVASION_IGNORE,Title = DBLocale.GetText("FINAL_MAGIC_EVASION_IGNORE"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAGIC_EVASION_IGNORE)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_HP_RECOVERY,Title = DBLocale.GetText("FINAL_HP_RECOVERY"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_HP_RECOVERY)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MP_RECOVERY,Title = DBLocale.GetText("FINAL_MP_RECOVERY"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MP_RECOVERY)});

		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_HP_RECOVERY_TIME, Title = DBLocale.GetText("FINAL_HP_RECOVERY_TIME"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_HP_RECOVERY_TIME) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MP_RECOVERY_TIME, Title = DBLocale.GetText("FINAL_MP_RECOVERY_TIME"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MP_RECOVERY_TIME) });

		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_POTION_RECOVERY_PLUS,Title = DBLocale.GetText("FINAL_POTION_RECOVERY_PLUS"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_POTION_RECOVERY_PLUS)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_POTION_RECOVERY_PER,Title = DBLocale.GetText("FINAL_POTION_RECOVERY_PER"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_POTION_RECOVERY_PER)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_POTION_RECOVERY_TIME_PER, Title = DBLocale.GetText("POTION_RECOVERY_TIME_PER"), Value = _myEntity.GetAbility(E_AbilityType.POTION_RECOVERY_TIME_PER)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAZ_RATE_DOWN_PER,Title = DBLocale.GetText("FINAL_MAZ_RATE_DOWN_PER"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAZ_RATE_DOWN_PER)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAZ_RATE_UP_PER,Title = DBLocale.GetText("FINAL_MAZ_RATE_UP_PER"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAZ_RATE_UP_PER)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.FINAL_MAX_WEIGH,Title = DBLocale.GetText("FINAL_MAX_WEIGH"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_MAX_WEIGH)});
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.GOLD_DROP_AMT_PER, Title = DBLocale.GetText("GOLD_DROP_AMT_PER"), Value = _myEntity.GetAbility(E_AbilityType.GOLD_DROP_AMT_PER) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.EXP_DROP_AMT_PER, Title = DBLocale.GetText("EXP_DROP_AMT_PER"), Value = _myEntity.GetAbility(E_AbilityType.EXP_DROP_AMT_PER) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.SKILL_COOLTIME_PER, Title = DBLocale.GetText("SKILL_COOLTIME_PER"), Value = _myEntity.GetAbility(E_AbilityType.SKILL_COOLTIME_PER) });
		list.Add(new ScrollCharacterDetailStatData() {Type = E_AbilityType.SKILL_MP_PER, Title = DBLocale.GetText("SKILL_MP_PER"), Value = _myEntity.GetAbility(E_AbilityType.SKILL_MP_PER) });

		list.Add(new ScrollCharacterDetailStatData() { Type = E_AbilityType.FINAL_GOLD_DROP_AMT, Title = DBLocale.GetText("FINAL_GOLD_DROP_AMT"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_GOLD_DROP_AMT) });
		list.Add(new ScrollCharacterDetailStatData() { Type = E_AbilityType.FINAL_EXP_DROP_AMT, Title = DBLocale.GetText("FINAL_EXP_DROP_AMT"), Value = _myEntity.GetAbility(E_AbilityType.FINAL_EXP_DROP_AMT) });
		Data.InsertItemsAtEnd(list);
		#endregion

		Data.NotifyListChangedExternally();
		RefreshData();
		_callback?.Invoke();
	}

	/// <summary>상세 스탯 업데이트</summary>
	/// <param name="_stats">변경 스탯.</param>
	public void UpdateDetailStats(Dictionary<E_AbilityType, float> _stats = null)
	{
		foreach (var stat in _stats)
		{
			var findstat = Data.List.Find(dataStats => (dataStats.Type == stat.Key));
			if (findstat != null)
			{
				findstat.Value = stat.Value;
			}
		}
		Data.NotifyListChangedExternally();
		RefreshData();
	}

	/// <summary>갱신 될 스탯 업데이트</summary>
	/// <param name="_stats">변경 스탯.</param>
	public void UpdatePreviewStats(Dictionary<E_AbilityType, float> _stats = null)
	{
		if (_stats.Count == 0)
		{
			for (int i = 0; i < Data.Count; i++)
			{
				Data[i].PreviewValue = 0;
			}
			RefreshData();
			return;
		}

		for (int i = 0; i < Data.Count; i++)
			Data[i].PreviewValue = 0;
		int previewValue = 0;

		foreach (var stat in _stats)
		{
			var findstat = Data.List.Find(dataStats => (dataStats.Type == stat.Key));
			if (findstat != null)
			{
				previewValue = (int)stat.Value - (int)findstat.Value;
				findstat.PreviewValue = previewValue;
			}
		}
		
		Data.NotifyListChangedExternally();
		RefreshData();
	}
}

public class ScrollCharacterDetailStatData
{
	public E_AbilityType Type;
	public string Title;
	public float Value;
	public float PreviewValue = 0;
}

public class UICharacterDetailStatHolder : BaseItemViewsHolder
{
	private ZText Title;
	private ZText Value;

	public override void CollectViews()
	{
        base.CollectViews();
		root.GetComponentAtPath("Title", out Title);
		root.GetComponentAtPath("Value", out Value);
	}

	public void UpdateViews(ScrollCharacterDetailStatData _model)
	{
		Title.text = string.Format("{0}", _model.Title);
		if(_model.PreviewValue == 0)
			Value.text = string.Format("{0}", DBAbility.ParseAbilityValue(DBAbility.GetAbility(_model.Type).AbilityID, _model.Value));
		else
			Value.text = string.Format("{0}<color=green>({1})</color>", DBAbility.ParseAbilityValue(DBAbility.GetAbility(_model.Type).AbilityID, _model.Value), (int)_model.PreviewValue);
	}
}