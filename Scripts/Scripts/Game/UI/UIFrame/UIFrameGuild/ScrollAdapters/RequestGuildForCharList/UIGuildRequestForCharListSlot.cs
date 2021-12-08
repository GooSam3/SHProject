using System;
using UnityEngine;
using UnityEngine.UI;

public class UIGuildRequestForCharListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private Image imgIcon;
    [SerializeField] private Text txtLevel;
    [SerializeField] private Text txtName;
    [SerializeField] private Text txtMemberCnt;
    [SerializeField] private Text txtMasterName;
    [SerializeField] private Text txtIntroduction;
    #endregion
    #endregion

    #region System Variables
    public ulong GuildID { get; private set; }
    private Action<ulong> OnCanceled;
    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        ulong guildID
        , Sprite iconSprite
        , uint level
        , string name
        , uint curMemberCnt
        , uint maxMemberCnt
       , string masterName
        , string introduction)
    {
        GuildID = guildID;
        imgIcon.sprite = iconSprite;
        txtLevel.text = level.ToString();
        txtName.text = name;
        txtMemberCnt.text = string.Format("{0}/{1}", curMemberCnt, maxMemberCnt);
        txtMasterName.text = masterName;
        txtIntroduction.text = introduction;
    }

    public void AddListener_OnCanceled(Action<ulong> onCanceled)
    {
        OnCanceled = onCanceled;
    }
    #endregion

    #region Private Methods
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClickCancelRequest()
    {
        //#region TEST 
        //OnCanceled?.Invoke(GuildID);
        //return;
        //#endregion
        UIFrameGuildTabBase.OpenTwoButtonQueryPopUp(
            title: "확인"
            , content: "길드 가입 신청을 취소하시겠습니까?"
            , onConfirmed: () =>
           {
               ZWebManager.Instance.WebGame.REQ_GuildRequestJoinCancel(GuildID,
                   (revPacket, resList) =>
               {
                   OnCanceled?.Invoke(GuildID);
               });
           });
    }
    #endregion
}
