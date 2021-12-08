using ZNet;
using ZNet.Data;
using WebNet;

/// <summary> 파티 맴버 정보 </summary>
public class ZPartyMember
{
    public uint EntityId { get; private set; }
    public ulong CharacterId { get; private set; }
    public uint CharacterTid { get; private set; }
    public string Nickname { get; private set; }
    public uint ServerIdx { get; private set; }

    public bool IsMaster { get; private set; }

    public ZPartyMember(PartyMemberDetail info)
    {
        Reset(info);
    }

    public void Reset(PartyMemberDetail info)
    {
        CharacterId = info.CharId;
        CharacterTid = info.CharTid;
        IsMaster = info.IsMaster;
        Nickname = info.Nick;
        ServerIdx = info.ServerIdx;
    }

    public void ChangeMaster(ulong characterId)
    {
        IsMaster = CharacterId == characterId;
    }

    public void SetEntityId(uint entityId)
    {
        EntityId = entityId;
    }
}
