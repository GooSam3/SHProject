using GameDB;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

[Serializable]
public class FxInteractableGroup // 이펙트 시작할때 비활성화될 오브젝트들
{
    [SerializeField]
    private List<Selectable> listSelectable;

    private List<bool> listState = new List<bool>();

    /// <summary>
    /// 선택가능한 객체의 상태를 변경함
    /// 
    /// true 일시 원래 상태로 복구 (false 시 상태 저장값)
    /// false 일시 선택 불가상태로 변경
    /// 
    /// 만일을 대비하여 연출 끝나고 바로 변경하길바람(컨텐츠 내 UI 업데이트 전)
    /// </summary>
    /// <param name="_state"></param>
    /// <param name="_force">강제로 할당(컨텐츠 초기화시)</param>
    public void SetInteractable(bool _state, bool _force = false)
    {
        if(_state == false)
        {
            listState.Clear();
            foreach(var iter in listSelectable)
            {
                listState.Add(iter.interactable);
            }
            listSelectable.ForEach(item => item.interactable = _state);
        }
        else
        {
            for(int i =0;i<listSelectable.Count;i++)
            {
                bool state = false;

                if (listState.Count > i)
                    state = listState[i];

                if (_force == false)
                    listSelectable[i].interactable = state;
                else
                    listSelectable[i].interactable = _state;
            }
        }
        
        
    }
}

[Serializable]// particlesystem.main.duration 과 실제 이펙트의 가동시간이 상이함 -> 개별설정
public class UIFxParticle
{
    public ParticleSystem fx;
    public float duration;

    public void Play()
    {
        if (fx.gameObject.activeSelf == false)
            fx.gameObject.SetActive(true);

        fx.Play(true);
    }

    public void Stop()
    {
        if (fx.gameObject.activeSelf == true)
            fx.gameObject.SetActive(false);

        fx.Stop(true);
    }
}

public static class UICommon
{

    // 추후 스프라이트 이름 관리 클래스(ex UICommon.SpriteName) 생성 및 이동
    private const string TEXT_COLOR_FORMAT = "<color={0}>{1}</color>";
    public const string HEX_COLOR_DEFAULT = "#FFFFFF";
    private const string FORMAT_GRADE_BG = "img_grade_0{0}";
    private const string FORMAT_RUNE_STAR = "icon_star_set_0{0}";
    private const string FORMAT_ENCHANT_STEP = "+{0}";
    private const string FORMAT_RUNE_SET_LOCALE = "{0}_Pet_Equip_Name_Tooltip";

    private const string SPRITE_STAR_ON = "img_grade_star_01";
    private const string SPRITE_STAR_OFF = "Icon_star_01_bg";

    private const string FORMAT_SPRITE_ATTACKTYPE_MELEE = "icon_style_melee_{0}";
    private const string FORMAT_SPRITE_ATTACKTYPE_RANGE = "icon_style_range_{0}";
    private const string FORMAT_SPRITE_ATTACKTYPE_ALL = "icon_skill_share_{0}";

    /// <summary>
    /// prefix + 속성 타입.lower 
    /// </summary>
    public const string ATTRIBUTE_SPRITE_PREFIX = "icon_char_attribute_";

    public enum E_SIZE_OPTION
    {
        Small,
        Midium,
        Big,
    }

    public static E_NoticeOption noticeOption = E_NoticeOption.TypeB;

    // Me.GetCurrency 로 이전 
    /// <summary>
    ///// 재화 정보
    ///// </summary>
    ///// <param name = "_itemTid"> 아이템 Tid </param>
    //public static ulong GetCurrency(uint _itemTid)
    //{
    //    for (int i = 0; i < Me.CurCharData.InvenList.Count; i++)
    //        if (Me.CurCharData.InvenList[i].item_tid == _itemTid)
    //            return Me.CurCharData.InvenList[i].cnt;

    //    return 0;
    //}

    /// <summary>
    /// 아이템 아이콘 변경
    /// </summary>
    /// <param name = "_itemTid"> 아이템 Tid </param>
    public static Sprite GetItemIconSprite(uint _itemTid)
    {
        Sprite spr = null;
        Item_Table table = null;

        if (_itemTid != 0 && DBItem.GetItem(_itemTid, out table) && !string.IsNullOrEmpty(table.IconID))
            spr = ZManagerUIPreset.Instance.GetSprite(table.IconID);

        if (spr == null && table!=null)
            spr = ZManagerUIPreset.Instance.GetSprite($"item_empty_{table.ItemType.ToString().ToLower()}_icon");

        return spr;
    }


    public class ItemDescInfo
    {
        public string strDesc;
        public string Value;
    }
    /// <summary>
    /// 아이템 상세정보 받아오기
    /// </summary>
    /// <param name="_itemTid">아이템 Tid</param>
    /// <returns></returns>
    public static List<ItemDescInfo> GetItemDesc(uint _itemTid)
    {
        List<ItemDescInfo> ItemDetailInfoList = new List<ItemDescInfo>();
        ItemDetailInfoList.Clear();

        var tableData = DBItem.GetItem(_itemTid);

        List<uint> listAbilityActionIds = new List<uint>();
        Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>> abilitys = new Dictionary<GameDB.E_AbilityType, System.ValueTuple<float, float>>();

        if (tableData.AbilityActionID_01 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_01);
        if (tableData.AbilityActionID_02 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_02);
        if (tableData.AbilityActionID_03 != 0)
            listAbilityActionIds.Add(tableData.AbilityActionID_03);

        foreach (var abilityActionId in listAbilityActionIds)
        {
            var abilityActionData = DBAbility.GetAction(abilityActionId);
            switch (abilityActionData.AbilityViewType)
            {
                case GameDB.E_AbilityViewType.ToolTip:
                    var itemDescInfo = new ItemDescInfo();
                    itemDescInfo.strDesc = string.Format("{0}{1}", "", DBLocale.ParseAbilityTooltip(abilityActionData, "2", "1"));
                    itemDescInfo.Value = null;

                    ItemDetailInfoList.Add(itemDescInfo);
                    break;
                case GameDB.E_AbilityViewType.Not:
                default:
                    var enumer = DBAbility.GetAllAbilityData(abilityActionId).GetEnumerator();
                    while (enumer.MoveNext())
                    {
                        if (!abilitys.ContainsKey(enumer.Current.Key))
                        {
                            abilitys.Add(enumer.Current.Key, enumer.Current.Value);
                        }
                    }
                    break;
            }
        }

        foreach (var ability in abilitys)
        {
            if (!DBAbility.IsParseAbility(ability.Key))
                continue;

            float abilityminValue = (uint)abilitys[ability.Key].Item1;
            float abilitymaxValue = (uint)abilitys[ability.Key].Item2;

            var itemDescInfo = new ItemDescInfo();

            itemDescInfo.strDesc = DBLocale.GetText(DBAbility.GetAbilityName(ability.Key));
            var newValue = DBAbility.ParseAbilityValue(ability.Key, abilityminValue, abilitymaxValue);
            itemDescInfo.Value = string.Format("{0}", newValue);

            ItemDetailInfoList.Add(itemDescInfo);
        }

        return ItemDetailInfoList;
    }

    /// <summary>
    /// 아이템 등급 보드 변경
    /// </summary>
    /// <param name = "_itemTid"> 아이템 Tid </param>
    public static Sprite GetItemGradeSprite(uint _itemTid)
    {
        Sprite spr = null;
        Item_Table table = DBItem.GetItem(_itemTid);
        spr = GetGradeSprite(table?.Grade??0);

        return spr;
    }

    public static Sprite GetGradeSprite(byte grade)
    {
        return ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_GRADE_BG, grade));
    }

    /// <summary>
    /// 캐릭터 속성 아이콘 변경
    /// </summary>
    /// <param name = "_charTid"> 캐릭터 Tid </param>
    public static Sprite GetCharAttributeSprite(uint _charTid)
    {
        Sprite spr = null;

        if (_charTid != 0)
            spr = ZManagerUIPreset.Instance.GetSprite(ATTRIBUTE_SPRITE_PREFIX + (DBCharacter.GetAttributeType(_charTid).ToString().ToLower()));

        return spr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attributeType"> 속성 타입 </param>
    public static Sprite GetCharAttributeSprite(E_UnitAttributeType attributeType)
    {
        if (attributeType == E_UnitAttributeType.None)
        {
            ZLog.LogError(ZLogChannel.UI, "Wrong Attribute Type.");
            return null;
        }

        return ZManagerUIPreset.Instance.GetSprite(ATTRIBUTE_SPRITE_PREFIX + attributeType.ToString().ToLower());
    }

    /// <summary>
    /// 속성과연결된 스프라이트 반환, 사이즈 존재
    /// </summary>
    public static Sprite GetAttributeSprite(E_UnitAttributeType type, E_SIZE_OPTION opt = E_SIZE_OPTION.Small)
    {
        switch (opt)
        {
            case E_SIZE_OPTION.Small:
                return ZManagerUIPreset.Instance.GetSprite(string.Format("icon_element_{0}_s", type.ToString().ToLower()));

            case E_SIZE_OPTION.Midium:
                return ZManagerUIPreset.Instance.GetSprite(string.Format("icon_element_{0}_m", type.ToString().ToLower()));
        }
        return null;
    }

    /// <summary>
    /// 펫 모험 지도 내 핀컬러 가져옴, 스프라이트 이름에서 마지막 언더바 뒤에 해당하는 숫자 참조
    /// ex) a_b_00 => 00
    /// </summary>
    /// <param name="str">icon name</param>
    /// <returns>color</returns>
    /// <seealso cref="ResourceSet.ColorPalette.PetAdv_Pin_00"~06/>
    public static Color GetPetAdventurePinColor(string str)
    {
        var res = str.Split('_');

        if(res.Length > 0)
        {
            if(int.TryParse(res[res.Length-1], out int num))
            {
                switch(num)
                {
                    case 0: return ResourceSetManager.Palette.PetAdv_Pin_00;
                    case 1: return ResourceSetManager.Palette.PetAdv_Pin_01;
                    case 2: return ResourceSetManager.Palette.PetAdv_Pin_02;
                    case 3: return ResourceSetManager.Palette.PetAdv_Pin_03;
                    case 4: return ResourceSetManager.Palette.PetAdv_Pin_04;
                    case 5: return ResourceSetManager.Palette.PetAdv_Pin_05;
                    case 6: return ResourceSetManager.Palette.PetAdv_Pin_06;
                }
            }
        }

        return Color.white;
    }

    public static Sprite GetRuneGradeStarSprite(int grade)
    {
        if (grade <= 0)
            return null;

        return ZManagerUIPreset.Instance.GetSprite(string.Format(FORMAT_RUNE_STAR, grade));
    }

    public static Sprite GetRuneSetTypeSprite(E_RuneSetType type, bool isBigImage = false)
    {
        if (DBItem.GetRuneSetTable(type, out var table) == false)
            return null;

        string iconID = $"{table.IconID}{(isBigImage ? "_b" : "")}";

        return ZManagerUIPreset.Instance.GetSprite(string.Format(table.IconID, iconID));
    }

    // 1줄짜리 2줄로(능력치슬롯쪽에서 사용)
    public static string[] GetRuneSetAbilityTextArray(E_RuneSetType type)
    {
        return DBLocale.GetText(string.Format(FORMAT_RUNE_SET_LOCALE, type.ToString())).Split('\n');
    }

    public static string GetRuneSetAbilityText(E_RuneSetType type)
    {
        return DBLocale.GetText(string.Format(FORMAT_RUNE_SET_LOCALE, type.ToString()));
    }

    public static string GetEnchantText(int step)
    {
        if (step <= 0)
            return string.Empty;

        return string.Format(FORMAT_ENCHANT_STEP, step);
    }

    public static Sprite GetClassIconSprite(string classIconId, E_SIZE_OPTION opt = E_SIZE_OPTION.Small)
    {
        switch (opt)
        {
            case E_SIZE_OPTION.Small:
                return ZManagerUIPreset.Instance.GetSprite(string.Format("{0}_s", classIconId).ToLower());

            case E_SIZE_OPTION.Midium:
                return ZManagerUIPreset.Instance.GetSprite(string.Format("{0}_m", classIconId).ToLower());
        }
        return null;
    }

    public static Sprite GetChangeQuestTypeSprite(E_ChangeQuestType type, E_SIZE_OPTION opt = E_SIZE_OPTION.Small)
    {
        string sizeForm = "m";

        if(opt == E_SIZE_OPTION.Small)
        {
            sizeForm = "s";
        }

        switch (type)
        {
            case E_ChangeQuestType.None:
                return GetSprite(string.Format(FORMAT_SPRITE_ATTACKTYPE_ALL, sizeForm));
            case E_ChangeQuestType.AttackShort:
                return GetSprite(string.Format(FORMAT_SPRITE_ATTACKTYPE_MELEE, sizeForm));
            case E_ChangeQuestType.AttackLong:
                return GetSprite(string.Format(FORMAT_SPRITE_ATTACKTYPE_RANGE, sizeForm));
        }

        return null;
    }

    public static Sprite GetClassIconSprite(E_CharacterType characterType, E_SIZE_OPTION opt = E_SIZE_OPTION.Small)
    {
        switch (characterType)
        {
            case E_CharacterType.All:
                {
                    return GetClassIconSprite("icon_skill_share", opt);
                }
            case E_CharacterType.Knight:
                {
                    return GetClassIconSprite("icon_gladiator_01", opt);
                }
            case E_CharacterType.Assassin:
                {
                    return GetClassIconSprite("icon_assassin_01", opt);
                }
            case E_CharacterType.Archer:
                {
                    return GetClassIconSprite("icon_archer_01", opt);
                }
            case E_CharacterType.Wizard:
                {
                    return GetClassIconSprite("icon_magician_01", opt);
                }
        }
        return null;
    }

    /// <summary>
    /// 캐릭터 속성과 연관된 TID 반환
    /// </summary>
    /// 
    public static uint GetCharTid(string _charTid, E_UnitAttributeType _charAttribute)
    {
        uint tid = 0;

        for (int i = 0; i < DBCharacter.UsableCharTableList.Count; i++)
        {
            if (DBCharacter.UsableCharTableList[i].CharacterTextID == _charTid && DBCharacter.UsableCharTableList[i].AttributeType == _charAttribute)
            {
                tid = DBCharacter.UsableCharTableList[i].CharacterID;
                return tid;
            }
        }

        return tid;
    }

    /// <summary> 로그인 씬 이전에서만 사용하는 팝업 </summary>
    public static void OpenConsolePopup(UnityAction<UIPopupConsole> _callback)
    {
        GameObject consolePopup = GameObject.Instantiate(Resources.Load<GameObject>("UI/Prefab/UIFrame/UIPopupConsole"));
        UIPopupConsole popup = consolePopup.GetComponent<UIPopupConsole>();
        _callback?.Invoke(popup);
    }

    /// <summary>System Popup 호출</summary>
    /// <param name="_callback"> 팝업 생성이 끝난 후 실행시킬 콜백 </param>
    public static void OpenSystemPopup(UnityAction<UIPopupSystem> _callback)
    {
        UIManager.Instance?.Open<UIPopupSystem>((uiAssetName, popup) =>
        {
            _callback?.Invoke(popup);
        });
    }

    /// <summary>System Popup | 확인 버튼만 존재 </summary>
    /// <param name="_btnCB">null이라도 팝업은 닫힘</param>
    public static void OpenSystemPopup_One(string _title, string _content, string _btnTxt, UnityAction<UIPopupBase> _btnCB = null)
    {
        UIManager.Instance?.Open<UIPopupSystem>((uiAssetName, _popup) =>
        {
            _popup.Open(_title, _content,
                new string[]
                {
                    _btnTxt
                },
                new Action[]
                {
                    delegate { _btnCB?.Invoke(_popup); _popup.Close(); }
                });
        });
    }


    public static void OpenCostConfirmPopup(UnityAction<UIPopupCostConfirm> _callback)
    {
        UIManager.Instance?.Open<UIPopupCostConfirm>(delegate
        {
            _callback?.Invoke(UIManager.Instance.Find<UIPopupCostConfirm>());
        });
    }

    [Obsolete("GameObjectExtensions.SetLayersRecursively() 활용바람", true)]
    public static void SetAllChildLayer(GameObject _obj, int _layer)
    {
        Transform[] obj = _obj.GetComponentsInChildren<Transform>();
        foreach (Transform t in obj)
        {
            t.gameObject.layer = _layer;
        }
    }

    public static void RotateObjectDrag(UnityEngine.EventSystems.BaseEventData eventData, GameObject _rotateObj)
    {
        if (_rotateObj == null || ReferenceEquals(null, _rotateObj))
            return;

        UnityEngine.EventSystems.PointerEventData pointData = eventData as UnityEngine.EventSystems.PointerEventData;

        if (Mathf.Abs(pointData.pressPosition.x - pointData.position.x) > 5f)
        {
            Vector3 newRot = _rotateObj.transform.localEulerAngles;
            newRot.y -= pointData.delta.x;

            if (_rotateObj != null)
            {
                _rotateObj.transform.localEulerAngles = newRot;
            }
        }
    }

    public static void SetNoticeMessage(string _txt, Color _color, float _displayTime, UIMessageNoticeEnum.E_MessageType _type)
    {
        switch (_type)
        {
            case UIMessageNoticeEnum.E_MessageType.Notice:     UIManager.Instance?.Open<UIFrameMessageNotice>(delegate { UIManager.Instance.Find<UIFrameMessageNotice>().AddMessage(UIMessageNoticeEnum.E_NoticeType.Main, new UIMessageNoticeData() { Content = _txt, Color = _color, FadeHoldTime = _displayTime }); }); break;
            case UIMessageNoticeEnum.E_MessageType.SubNotice:  UIManager.Instance?.Open<UIFrameMessageNotice>(delegate { UIManager.Instance.Find<UIFrameMessageNotice>().AddMessage(UIMessageNoticeEnum.E_NoticeType.Sub, new UIMessageNoticeData() { Content = _txt, Color = _color, FadeHoldTime = _displayTime }); }); break;
            case UIMessageNoticeEnum.E_MessageType.BackNotice: UIManager.Instance.Find<UIFrameMessageCharacter>().AddMessage(new UIMessageNoticeData() { Content = _txt, Color = _color, FadeHoldTime = _displayTime });break;
        }
    }

    static float LastBeAttackTime;

    /// <summary>
    /// [박윤성] 자동모드에서 상대 플레이어에게 공격을 받았을때 알람띄우기
    /// (소리, 진동, 메세지)
    /// </summary>
    public static void SetAlramBeAttacked()
    {
        //중복으로 여러번 뜨는거 방지
        if (LastBeAttackTime == 0 || (Time.time - LastBeAttackTime) > 15)
        {
            //메세지
            string noticeStr = DBLocale.GetText("Alert_Attack_By_OtherPlayer");
            SetNoticeMessage(noticeStr, Color.red, 2f, UIMessageNoticeEnum.E_MessageType.BackNotice);
        }
        LastBeAttackTime = Time.time;

        //진동
        Handheld.Vibrate();
    }

    public static void SetKillMessage( string _txt, E_KillMessage _type, float duration = 2.0f )
    {
        UIManager.Instance?.Open<UIKillMessage>( delegate { UIManager.Instance.Find<UIKillMessage>().AddMessage( _txt, _type, duration ); } );
    }

    public static void FadeInOut(Action _callBack, E_UIFadeType _fadeType, float _duration = 1.0f)
    {
        UIManager.Instance?.Open<UIFrameFadeInOut>(delegate { UIManager.Instance.Find<UIFrameFadeInOut>().FadeInOut(_callBack, _fadeType, _duration); });
    }

    public static string GetColoredText(string hexColor, string text)
    {
        return string.Format(TEXT_COLOR_FORMAT, hexColor, text);
    }

    // 아이템의 이름을 반환, 등급별 색상 적용
    public static string GetItemText(Item_Table itemTable)
	{
        string enchantStep = string.Empty;

        if (itemTable.Step > 0)
            enchantStep = $"+{itemTable.Step}";

        return DBUIResouce.GetItemGradeFormat($"{DBLocale.GetText(itemTable.ItemTextID)}{enchantStep}", itemTable.Grade);
	}

    public static Sprite GetSprite(string str)
    {
        return ZManagerUIPreset.Instance.GetSprite(str);
    }

    public static Sprite GetStarSprite(bool isOn)
    {
        if (isOn)
            return ZManagerUIPreset.Instance.GetSprite(SPRITE_STAR_ON);
        else
            return ZManagerUIPreset.Instance.GetSprite(SPRITE_STAR_OFF);
    }

    public static string GetProgressText(int left, int right, bool bracket = true)
    {
        if (bracket)
            return DBLocale.GetText("UI_Common_Amount_Simple", left, right);
        else
            return DBLocale.GetText("Collection_Page_No", left, right);
    }

    public static uint GetEquipSocketCount(List<uint> _socketList)
    {
        uint cnt = 0;

        for (int i = 0; i < _socketList.Count; i++)
            if (_socketList[i] > 0)
                cnt += 1;

        return cnt;
    }

    public static bool GetEquipSocketCheck(List<uint> _socketList)
    {
        for (int i = 0; i < _socketList.Count; i++)
        {
            if (_socketList[i] != 0)
                return true;
        }
        return false;
    }

    #region ========:: 공용 기능 ::========
    /// <summary> 시련의 성역에서 나갈 때 확인 팝업 </summary>
    public static void OpenPopup_ExitTrialSanctuary()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(string.Empty, "시련의 성역에서 나가시겠습니까? \n나가게 될 경우 재 입장 시 처음부터 진행 됩니다.",
                new string[]
                {
                    ZUIString.LOCALE_CANCEL_BUTTON,
                    ZUIString.LOCALE_OK_BUTTON
                },
                new Action[]
                {
                    delegate { _popup.Close(); },
                    delegate { ZGameManager.Instance.TryEnterStage(DBConfig.Town_Portal_ID, false, 0, 0); _popup.Close(); }
                });
        });
    }

    /// <summary> 캐릭터 선택화면으로 이동여부 물어보는 팝업 </summary>
    public static void OpenPopup_GoCharacterSelectState()
    {
        UICommon.OpenSystemPopup((UIPopupSystem _popup) =>
        {
            _popup.Open(string.Empty, "캐릭터 선택화면으로 이동하시겠습니까?",
                new string[]
                {
                    ZUIString.LOCALE_CANCEL_BUTTON,
                    ZUIString.LOCALE_OK_BUTTON
                },
                new Action[]
                {
                    delegate { _popup.Close(); },
                    delegate { ZGameManager.Instance.GoCharacterSelectState(); _popup.Close(); }
                });
        });
    }

    #endregion


    public static void TrySumValue<T>(this Dictionary<T, float> dic, T key, float value) where T : Enum
    {
        if (dic.ContainsKey(key) == false)
            dic.Add(key, value);
        else
            dic[key] += value;
    }

    public static void SafeStartCoroutine(this MonoBehaviour mono, IEnumerator coroutine)
    {
        mono.StopCoroutine(coroutine);
        mono.StartCoroutine(coroutine);
    }
}

public enum GainType
{
    TYPE_EXP,
    TYPE_ITEM,
    TYPE_PET,
    TYPE_CHANGE,
    TYPE_ETC,
}

public class GainInfo
{
    public GainType gainType;
    public uint ItemTid;
    public ulong Cnt;
    public string Icon;
    public string CntText;

    public GainInfo()
    {
    }

    public GainInfo(TakeMailInfo getMailInfo)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = getMailInfo.ItemTid;
        Cnt = getMailInfo.Cnt;
    }
    public GainInfo(GetItemInfo getitem)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = getitem.ItemTid;
        Cnt = getitem.ItemCnt;
    }
    public GainInfo(Rune rune)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = rune.ItemTid;
        Cnt = 1;
    }
    public GainInfo(ItemEquipment equip)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = equip.ItemTid;
        Cnt = 1;
    }
    public GainInfo(ItemStack stack)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = stack.ItemTid;
        Cnt = stack.Cnt;
    }
    public GainInfo(AccountItemStack stack)
    {
        gainType = GainType.TYPE_ITEM;
        ItemTid = stack.ItemTid;
        Cnt = stack.Cnt;
    }
    public GainInfo(GetChangeInfo change)
    {
        gainType = GainType.TYPE_CHANGE;
        ItemTid = change.ChangeTid;
        Cnt = change.ChangeCnt <= 0 ? 1 : change.ChangeCnt;
    }
    public GainInfo(Change change)
    {
        gainType = GainType.TYPE_CHANGE;
        ItemTid = change.ChangeTid;
        Cnt = change.Cnt <= 0 ? 1 : change.Cnt;
    }
    public GainInfo(GetPetInfo pet)
    {
        gainType = GainType.TYPE_PET;
        ItemTid = pet.PetTid;
        Cnt = pet.PetCnt <= 0 ? 1 : pet.PetCnt;
    }
    public GainInfo(Pet pet)
    {
        gainType = GainType.TYPE_PET;
        ItemTid = pet.PetTid;
        Cnt = pet.Cnt <= 0 ? 1 : pet.Cnt;
    }
    public GainInfo(GainType _gainType, uint _ItemTid, ulong _Cnt)
    {
        gainType = _gainType;
        ItemTid = _ItemTid;
        Cnt = _Cnt;
    }
}