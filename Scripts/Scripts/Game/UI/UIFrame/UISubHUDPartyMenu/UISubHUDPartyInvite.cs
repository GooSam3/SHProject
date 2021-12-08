using UnityEngine;
using UnityEngine.UI;

/// <summary> 파티 초대 요청 처리 </summary>
public class UISubHUDPartyInvite : MonoBehaviour
{
    [SerializeField]
    private Animation Anim;
    [SerializeField]
    private Text TextSenderName;
    [SerializeField]
    private Image ImgClassIcon;

    private uint PartyUid;
    private ZPartyMember Sender;

    private bool IsInviting = false;
    

    public void Invite(uint partyUid, ZPartyMember sender)
    {
        if (IsInviting)
        {
            return;
        }

        Anim.Play(PlayMode.StopAll);
        Invoke("TimeOut", Anim.clip.length);

        gameObject.SetActive(true);
        PartyUid = partyUid;
        Sender = sender;

        TextSenderName.text = Sender.Nickname;
        if (DBCharacter.TryGet(Sender.CharacterTid, out var charTable))
        {
            //클래스 icon 셋팅
            ImgClassIcon.sprite = UICommon.GetClassIconSprite(charTable.CharacterType, UICommon.E_SIZE_OPTION.Small);
        }
    }

    private void TimeOut()
    {
        OnClickRefuse();
    }

    public void Close()
    {
        CancelInvoke("TimeOut");
        Sender = null;
        PartyUid = 0;
        gameObject.SetActive(false);
    }

    #region ===== :: Button Action :: =====
    public void OnClickJoin()
    {
        ZPartyManager.Instance.Req_PartyJoin(PartyUid, delegate
        {            
        });

        Close();
    }

    public void OnClickRefuse()
    {
        ZPartyManager.Instance.Req_PartyRefuse(PartyUid, Sender.CharacterId, Sender.ServerIdx, delegate 
        {            
        });

        Close();
    }
    #endregion
}
