using GameDB;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISubHUDPartyMemberSlot : MonoBehaviour
{
    [SerializeField]
    private GameObject ObjMemberGroup;
    [SerializeField]
    private GameObject ObjInviteGroup;    
    [SerializeField]
    private GameObject ObjButtonGroup;
    [SerializeField]
    private GameObject ObjMasterMark;
    [SerializeField]
    private GameObject ObjChangeMasterButton;
    [SerializeField]
    private GameObject ObjKickButton;
    [SerializeField]
    private GameObject ObjExitButton;
    [SerializeField]
    private GameObject ObjOutOfMap;
    [SerializeField]
    private Image ImgClassIcon;
    [SerializeField]
    private Text TextName;
    [SerializeField]
    private Slider HpBar;
    [SerializeField]
    private Slider MpBar;
    [SerializeField]
    private Text InviteText;

    [SerializeField]
    private GameObject ObjBuffs;
    [SerializeField]
    private GameObject ObjDebuffs;

    [SerializeField]
    [ReadOnly]
    private int SlotIndex;

    private UISubHUDPartyBuffSlot[] Buffs;
    private UISubHUDPartyBuffSlot[] Debuffs;

    /// <summary> 파티 맴버 정보 </summary>
    public ZPartyMember Member { get; private set; }

    /// <summary> 마스터 인지 여부 </summary>
    private bool IsMaster { get { return Member?.IsMaster ?? false; } }

    /// <summary> 나다 </summary>
    private bool IsMyPc { get { return (Member?.CharacterId ?? 0) == ZPartyManager.Instance.CharacterId; } }

    /// <summary> 내 Pawn </summary>
    private ZPawn Pawn = null;

    private uint EntityId = 0;

    private Vector3 CachecPosition = Vector3.zero;

    public Vector3 CurrentPostion { get { return Pawn ? Pawn.Position : CachecPosition; } }

    private Action OnClickButtonGroup = null;

    private void Awake()
    {
        Buffs = ObjBuffs.GetComponentsInChildren<UISubHUDPartyBuffSlot>();
        Debuffs = ObjDebuffs.GetComponentsInChildren<UISubHUDPartyBuffSlot>();
    }

    /// <summary> 슬롯 초기화 </summary>
    public void Init(int slotIndex)
    {
        SlotIndex = slotIndex;
        ObjMasterMark.SetActive(false);
        ObjOutOfMap.SetActive(false);
        EntityId = 0;
    }

    /// <summary> 맴버 셋팅 </summary>
    public void SetMember(ulong memberCharacterId, Action onClickButtonGroup)
    {
        ZPartyManager.Instance.TryGet(memberCharacterId, out var member);
        Member = member;

        OnClickButtonGroup = onClickButtonGroup;
        ObjMemberGroup.SetActive(null != Member);
        ObjInviteGroup.SetActive(null == Member && ZPartyManager.Instance.m_dicMember.Count == SlotIndex && ZPartyManager.Instance.IsMaster);
        ObjMasterMark.SetActive(null != Member && Member.IsMaster);        
        ObjButtonGroup.SetActive(false);

        ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleEventCreateEntity);
        ZPawnManager.Instance.DoRemoveEventRemoveEntity(HandleEventRemoveEntity);

        if (null != member)
        {
            if(DBCharacter.TryGet(member.CharacterTid, out var charTable))
            {
                //클래스 icon 셋팅
                ImgClassIcon.sprite = UICommon.GetClassIconSprite(charTable.CharacterType, UICommon.E_SIZE_OPTION.Small);
            }

            TextName.text = Member.Nickname;

            //내 캐릭터는 mmo에서 party 정보가 오지 않는다. 그냥 entityId 셋팅
            EntityId = member.EntityId;
        }
        else
        {
            InviteText.text = DBLocale.GetText("Party_Apply");
            EntityId = 0;
        }

        UpdateHp(0, 1);
        UpdateMp(0, 1);

        ObjOutOfMap.SetActive(false == IsMyPc && 0 >=  EntityId && null != Member);

        SetPawn();
    }

    /// <summary> mmo에서 멤버 정보 업데이트 </summary>
    public void SetMemberByMmo(uint entityId, Vector3 pos, float maxHp, float currentHp, float maxMp, float currentMp)
    {
        EntityId = entityId;
        CachecPosition = pos;

        SetPawn();
        UpdateHp(currentHp, maxHp);
        UpdateMp(currentMp, maxMp);

        ObjOutOfMap.SetActive(false);
    }

    /// <summary> mmo에 멤버 정보가 없음 (다른 맵이거나 멤버가 없는 슬롯)</summary>
    public void ResetMemberByMmo()
    {
        //빈슬롯이다.
        if (null == Member)
            return;

        //내 캐릭터는 따로 정보를 받지 않는다.
        //if (IsMyPc)
        //    return;

        UpdateHp(0, 1);
        UpdateMp(0, 1);

        ObjOutOfMap.SetActive(true);
    }

    /// <summary> hp 업데이트 </summary>
    private void UpdateHp(float cur, float max)
    {
        HpBar.value = cur / max;
    }

    /// <summary> mp 업데이트 </summary>
    private void UpdateMp(float cur, float max)
    {
        MpBar.value = cur / max;
    }

    /// <summary> Pawn 및 pawn 이벤트 셋팅 </summary>
    private void SetPawn()
    {
        ZPawnManager.Instance.TryGetEntity(EntityId, out var pawn);
        SetPawn(pawn);
    }

    /// <summary> Pawn 및 pawn 이벤트 셋팅 </summary>
    private void SetPawn(ZPawn pawn)
    {
        if (null != Pawn)
        {
            Pawn.DoRemoveEventHpUpdated(UpdateHp);
            Pawn.DoRemoveEventMpUpdated(UpdateMp);
            Pawn.DoRemoveOnAblityActionChanged(UpdateBuff);
        }

        Pawn = pawn;

        ResetBuff(ref Buffs);
        ResetBuff(ref Debuffs);

        if (null != Pawn)
        {
            Pawn.DoAddEventHpUpdated(UpdateHp);
            Pawn.DoAddEventMpUpdated(UpdateMp);
            Pawn.DoAddOnAblityActionChanged(UpdateBuff);

            UpdateHp(Pawn.CurrentHp, Pawn.MaxHp);
            UpdateMp(Pawn.CurrentMp, Pawn.MaxMp);

            //Pawn이 있음. 제거 이벤트 등록
            ZPawnManager.Instance.DoAddEventRemoveEntity(HandleEventRemoveEntity);
        }
        else if(0 < EntityId)
        {            
            //Pawn이 없음. 생성 이벤트 등록
            ZPawnManager.Instance.DoAddEventCreateEntity(HandleEventCreateEntity);
        }
    }

    /// <summary> 버프 갱신 </summary>
    private void UpdateBuff(Dictionary<uint, EntityAbilityAction> abilityActions)
    {
        int buffIndex = 0;
        int debuffIndex = 0;
        foreach (var abilityAction in abilityActions.Values)
        {
            if (abilityAction.Table.HudBuffSignType == E_HudBuffSignType.Not)
            {
                continue;
            }   

            switch (abilityAction.Table.BuffType)
            {
                case E_BuffType.Buff:
                    {
                        SetBuff(abilityAction.Table.BuffIconString, ref buffIndex, ref Buffs);
                    }
                    break;
                case E_BuffType.DeBuff:
                    {
                        SetBuff(abilityAction.Table.BuffIconString, ref debuffIndex, ref Debuffs);
                    }
                    break;
            }
        }
        ResetBuff(ref buffIndex, ref Buffs);
        ResetBuff(ref debuffIndex, ref Debuffs);
    }

    private void ResetBuff(ref UISubHUDPartyBuffSlot[] slots)
    {
        for (int i = 0; i < slots.Length; ++i)
        {
            slots[i].SetBuff("");
        }
    }

    private void ResetBuff(ref int index, ref UISubHUDPartyBuffSlot[] slots)
    {
        for(int i = index; i < slots.Length; ++i)
        {
            slots[i].SetBuff("");
        }
    }

    private void SetBuff(string iconName, ref int index, ref UISubHUDPartyBuffSlot[] slots)
    {
        if (slots.Length <= index)
            return;

        slots[index].SetBuff(iconName);
        ++index;
    }

    private void UpdateButton()
    {
        if(null == Member)
        {
            ObjButtonGroup.SetActive(false);
            return;
        }

        ObjChangeMasterButton.SetActive(ZPartyManager.Instance.IsMaster && !IsMyPc);
        ObjKickButton.SetActive(ZPartyManager.Instance.IsMaster && !IsMyPc);
        ObjExitButton.SetActive(IsMyPc);
    }

    #region ===== :: Button Action :: =====
    public void OnCkickShowButton()
    {
        if (null != Member)
        {
            bool bActive = !ObjButtonGroup.activeSelf;
            OnClickButtonGroup?.Invoke();
            ObjButtonGroup.SetActive(bActive);
            UpdateButton();
        }   
    }

    public void CloseButtonGroup()
    {
        ObjButtonGroup.SetActive(false);
    }

    public void OnClickChangeMaster()
    {
        if(null == Member)
            return;

        //내가 마스터가 아니다.
        if (false == ZPartyManager.Instance.IsMaster)
            return;

        //이미 파장임
        if (IsMyPc)
            return;

        UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Party_Title_Text"), string.Format(DBLocale.GetText("Party_Delegate"), Member.Nickname), () =>
        {
            ZPartyManager.Instance.Req_PartyChangeMaster(Member.ServerIdx, Member.CharacterId, Member.CharacterTid, Member.Nickname);
        });        
    }

    public void OnClickKick()
    {
        if (null == Member)
            return;

        //내가 마스터가 아니다.
        if (false == ZPartyManager.Instance.IsMaster)
            return;

        //이미 파장임
        if (IsMyPc)
            return;

        UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Party_Title_Text"), string.Format(DBLocale.GetText("Party_Banishment"), Member.Nickname), () =>
        {
            ZPartyManager.Instance.Req_PartyKickOut(Member.ServerIdx, Member.CharacterId, Member.CharacterTid, Member.Nickname);
        });
    }

    public void OnClickExit()
    {
        if (null == Member)
            return;

        //내 캐릭터가 아님
        if (!IsMyPc)
            return;

        UIMessagePopup.ShowPopupOkCancel(DBLocale.GetText("Party_Title_Text"), DBLocale.GetText("Party_Secession"), () =>
        {
            ZPartyManager.Instance.Req_PartyOut();
        });
    }

    /// <summary> 초대 버튼 </summary>
    public void OnClickInvite()
    {
        //이미 파티원이 있다
        if (null != Member)
            return;

        //내가 마스터가 아니다.
        if (false == ZPartyManager.Instance.IsMaster)
            return;

        //TODO :: 닉네임 입력 팝업 필요.
        //ZPartyManager.Instance.Req_InviteParty();
        UIMessagePopup.ShowInputPopup(DBLocale.GetText("Party_Title_Text"), "초대할 캐릭터의 닉네임을 입력해주세요.", (targetName) =>
        {
            ZPartyManager.Instance.Req_InviteParty(targetName);
        }, null, DBConfig.NickName_Length_Max);
    }
    #endregion

    #region ===== :: Event Handle :: =====
    private void HandleEventCreateEntity(uint entityId, ZPawn pawn)
    {
        if(entityId != EntityId)
        {
            return;
        }

        //pawn 생성 이벤트 제거
        ZPawnManager.Instance.DoRemoveEventCreateEntity(HandleEventCreateEntity);

        SetPawn(pawn);
    }

    private void HandleEventRemoveEntity(uint entityId)
    {
        if (entityId != EntityId)
        {
            return;
        }

        //pawn 삭제 이벤트 제거
        ZPawnManager.Instance.DoRemoveEventRemoveEntity(HandleEventRemoveEntity);

        SetPawn(null);
    }
    #endregion
}
