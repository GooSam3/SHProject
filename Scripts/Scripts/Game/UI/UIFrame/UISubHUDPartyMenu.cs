using MmoNet;
using System.Collections.Generic;
using UnityEngine;

public class UISubHUDPartyMenu : ZUIFrameBase
{
    [SerializeField]
    private List<UISubHUDPartyMemberSlot> MemberSlots = new List<UISubHUDPartyMemberSlot>();

    [SerializeField]
    private GameObject ObjCreateParty;

    [SerializeField]
    private GameObject ObjPartyMembers;

    [SerializeField]
    private UISubHUDPartyInvite PartyInvite;

    protected override void OnShow(int _LayerOrder)
    {
        ObjCreateParty.SetActive(false);
        ObjPartyMembers.SetActive(false);
        PartyInvite.Close();

        //슬롯 초기화
        for (int i = 0; i < MemberSlots.Count; ++i)
        {
            MemberSlots[i].Init(i);
        }

        //최초 갱신
        HandlePartyUpdate();

        //TODO :: Show가 두번 불려서 두번 호출됨. (요기서 따로 예외처리를 해야하나)
        ZPartyManager.Instance.Req_CheckRefreshParty(() =>
        {
            //파티 갱신 이벤트 등록
            ZPartyManager.Instance.DoAddEventUpdateParty(HandlePartyUpdate);

            ZPartyManager.Instance.DoAddEventPartyInvite(HandleInvite);

            //mmo에서 파티 정보 업데이트
            ZPartyManager.Instance.DoAddEventUpdatePartyInfoByMmo(HandleUpdatePartyInfoByMmo);
        });
    }

    protected override void OnRemove()
    {
        base.OnRemove();
        RemoveEvent();
    }

    protected override void OnHide()
    {
        base.OnHide();
        RemoveEvent();
    }

    /// <summary> 등록된 이벤트 제거 </summary>
    private void RemoveEvent()
    {
        ZPartyManager.Instance.DoRemoveEventUpdateParty(HandlePartyUpdate);

        ZPartyManager.Instance.DoRemoveEventPartyInvite(HandleInvite);

        ZPartyManager.Instance.DoRemoveEventUpdatePartyInfoByMmo(HandleUpdatePartyInfoByMmo);
    }

    #region ===== :: Button Action :: =====
    public void OnClickCreateParty()
    {
        //이미 파티중이다.
        if (ZPartyManager.Instance.IsParty)
            return;

        ZPartyManager.Instance.Req_PartyCreate();
    }
    #endregion

    #region ===== :: Event Handle :: =====
    private void HandlePartyUpdate()
    {
        ObjCreateParty.SetActive(!ZPartyManager.Instance.IsParty);
        ObjPartyMembers.SetActive(ZPartyManager.Instance.IsParty);

        if(ZPartyManager.Instance.IsParty)
        {
            var members = new List<ZPartyMember>(ZPartyManager.Instance.m_dicMember.Values);
            for (int i = 0; i < MemberSlots.Count; ++i)
            {
                ulong memberCharacterId = 0;

                if (members.Count > i)
                {
                    memberCharacterId = members[i].CharacterId;
                }
                    
                MemberSlots[i].SetMember(memberCharacterId, CloseMemberSlotButtonGroup);
            }
        }
    }

    private void CloseMemberSlotButtonGroup()
    {
        foreach(var slot in MemberSlots)
        {
            slot.CloseButtonGroup();
        }
    }

    private void HandleInvite(uint partyUid, ZPartyMember sender)
    {
        //이미 파티중이다.
        if (ZPartyManager.Instance.IsParty)
            return;

        PartyInvite.Invite(partyUid, sender);
    }

    /// <summary> Mmo에서 파팅 정보를 받음 </summary>
    private void HandleUpdatePartyInfoByMmo(S2C_UpdatePartyInfo info)
    {       
        //각 슬롯에 정보 업데이트
        foreach(var slot in MemberSlots)
        {
            if (null == slot.Member)
                continue;

            bool bReset = true;
                        
            for (int i = 0; i < info.PartymembersLength; ++i)
            {
                var memberInfo = info.Partymembers(i).Value;

                if (memberInfo.CharId != slot.Member.CharacterId)
                    continue;
                var pos = memberInfo.Pos.Value;
                slot.SetMemberByMmo(memberInfo.Objectid, new Vector3(pos.X, pos.Y, pos.Z), memberInfo.MaxHp, memberInfo.CurHp, memberInfo.MaxMp, memberInfo.CurMp);
                bReset = false;
                break;
            }

            if(bReset)
            {
                slot.ResetMemberByMmo();
            }
        }
    }
    #endregion
}