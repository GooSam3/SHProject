/// <summary> RemotePc </summary>
public class ZPawnRemotePc: ZPawnCharacter
{
    protected override void OnInitializeEntityData(EntityDataBase data)
    {
        base.OnInitializeEntityData(data);

        SetRelationship();
        SetEvent();
    }

    /// <summary> 각 종 이벤트 추가 </summary>
    private void SetEvent()
    {
        if (false == ZPartyManager.hasInstance)
            return;

        ZPartyManager.Instance.DoAddEventUpdateParty(HandleUpdateParty);
    }

    protected override void OnDestroyImpl()
    {
        base.OnDestroyImpl();

        if (false == ZPartyManager.hasInstance)
            return;

        ZPartyManager.Instance.DoRemoveEventUpdateParty(HandleUpdateParty);
    }

    /// <summary> 해당 캐릭터와 나와 관계 처리 및 이벤트 처리 (파티, 길드, 친구, 적대 유저 등) </summary>
    private void SetRelationship()
    {
        HandleUpdateParty();        
        HandleUpdateHostile();
        HandleUpdateFriend();
    }
    #region ===== :: Event Handle :: =====
    /// <summary> 나와의 파티 관계 업데이트 </summary>
    protected void HandleUpdateParty()
    {
        ZPartyManager.Instance.TryGet(CharacterData.CharacterId, out var member);
        SetCustomConditionControl(E_CustomConditionControl.PartyMember, null != member);
    }

    /// <summary> 나와의 적대 관계 업데이트 </summary>
    protected void HandleUpdateHostile()
    {

    }

    /// <summary> 나와의 친구 관계 업데이트 </summary>
    private void HandleUpdateFriend()
    {

    }
    #endregion
}
