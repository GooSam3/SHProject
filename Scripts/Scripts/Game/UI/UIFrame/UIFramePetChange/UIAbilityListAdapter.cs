using Com.TheFallenGames.OSA.Core;
using Com.TheFallenGames.OSA.CustomParams;
using Com.TheFallenGames.OSA.DataHelpers;
using GameDB;
using System.Collections.Generic;
using UnityEngine;

public enum E_UIAbilityViewType
{
    Blank = 0,
    Text = 1,
    Ability = 2,
    AbilityCompare = 3,
}

// 어빌리티 아이디 : 값 페어
public class UIAbilityData
{
    public E_UIAbilityViewType viewType;

    public E_AbilityType type;
    public uint skillTarget;
    public float value;
    public bool useBySkillName;

    public string textLeft;//메인텍스트
    public string textRight;

    // 이전값
    public float compareValue = 0f;

    public UIAbilityData()
    {
        viewType = E_UIAbilityViewType.Ability;
    }

    public UIAbilityData(UIAbilityData origin)
    {
        viewType = origin.viewType;

        type = origin.type;
        skillTarget = origin.skillTarget;
        value = origin.value;
        useBySkillName = origin.useBySkillName;

        textLeft = origin.textLeft;
        textRight = origin.textRight;
    }

    public UIAbilityData(E_UIAbilityViewType _viewType)
    {
        viewType = _viewType;
    }

    public UIAbilityData(E_AbilityType _type, float _value)
    {
        viewType = E_UIAbilityViewType.Ability;
        type = _type;
        value = _value;
    }
}

public class UIAbilityListAdapter : OSA<BaseParamsWithPrefab, UIAbilityViewHolder>
{
    [Header("USE THIS / 0 is defualt"), SerializeField] private int PrefabSize;

    // 설정시 해당 프리팹 생성함
    [Header("SPAWN PREFAB TARGET null is Default"), SerializeField] private MonoAbilitySlotBase spawnTarget;

    public SimpleDataHelper<UIAbilityData> Data
    {
        get; private set;
    }

    protected override void Start()
    {
        //자동 이니셜라이즈 해제용으로 오버라이드만함
    }

    protected override UIAbilityViewHolder CreateViewsHolder(int itemIndex)
    {
        var instance = new UIAbilityViewHolder();

        instance.Init(_Params.ItemPrefab, _Params.Content, itemIndex);

        return instance;
    }

    protected override void UpdateViewsHolder(UIAbilityViewHolder newOrRecycled)
    {
        UIAbilityData data = Data[newOrRecycled.ItemIndex];
        newOrRecycled.SetSlot(data);
    }

    public void Initialize()
    {
        Data = new SimpleDataHelper<UIAbilityData>(this);
        float defaultSize = Parameters.DefaultItemSize;
        // !!

        string prefabName = nameof(UIAbilitySlot);
        if (spawnTarget && ReferenceEquals(spawnTarget, null) == false)
        {
            prefabName = spawnTarget.GetType().Name;
        }

        GameObject obj = ZPoolManager.Instance.FindClone(E_PoolType.UI, prefabName);

        var trans = obj.GetComponent<RectTransform>();

        if (PrefabSize > 0)
        {
            var originSize = trans.sizeDelta;

            originSize.y = PrefabSize;

            trans.sizeDelta = originSize;
        }

        Parameters.ItemPrefab = trans;

        Parameters.ItemPrefab.SetParent(transform);
        Parameters.ItemPrefab.transform.localScale = Vector2.one;
        Parameters.ItemPrefab.transform.localPosition = Vector3.zero;
        Parameters.ItemPrefab.gameObject.SetActive(false);

        Init();
    }

    public void Initialize_SkipPrefabManualLoading()
    {
        Data = new SimpleDataHelper<UIAbilityData>(this);
        Init();
    }

    public void RefreshListData(List<UIAbilityData> listData)
    {
        if (IsInitialized)
        {
            //uint skillTarget = 0;

            //for (int i = 0; i < listData.Count; i++)
            //{
            //    var data = listData[i];

            //    if (data.skillTarget != skillTarget)
            //    {
            //        var temp = new UIAbilityData(data);

            //        skillTarget = temp.skillTarget;

            //        temp.useBySkillName = true;

            //        listData.Insert(i, temp);
            //        i++;
            //        continue;
            //    }
            //    data.useBySkillName = false;
            //}

            Data.ResetItems(listData);

            SetNormalizedPosition(1);
        }
        else
            Initialized += () => RefreshListData(listData);
    }

}
