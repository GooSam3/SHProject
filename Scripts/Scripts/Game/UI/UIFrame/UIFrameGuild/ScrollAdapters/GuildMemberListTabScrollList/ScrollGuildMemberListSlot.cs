using System;
using UnityEngine;
using UnityEngine.UI;
using WebNet;
using ZNet.Data;

public class ScrollGuildMemberListSlot : MonoBehaviour
{
    #region SerializedField
    #region Preference Variable
    #endregion

    #region UI Variables
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private Button btnBan;
    [SerializeField] private Button btnAppointViceMaster;
    [SerializeField] private Button btnDegradeViceMaster;
    [SerializeField] private Button btnAppointMaster;
    [SerializeField] private Button btnAddFriend;
    [SerializeField] private Image imgSelectedGradient;

    [SerializeField] private Image imgGuildMasterIndicator;
    [SerializeField] private Image imgGuildSubMasterIndicator;

    [SerializeField] private Image imgThisPlayerIndicator;
    [SerializeField] private Image imgIcon;
    [SerializeField] private Image imgAttendance;

    [SerializeField] private Text txtName;
    [SerializeField] private Text txtLogInStatus;
    [SerializeField] private Text txtGrade;
    [SerializeField] private Text txtNotice;
    [SerializeField] private Text txtExp_DonateExp; // 개인,기부 공헌도 
    // [SerializeField] private Text txtDonateExp; // 기부 공헌도 
    [SerializeField] private Text txtWeekExp_WeekDonateExp; // 주간, 기부 공헌도 
                                                            // [SerializeField] private Text txtWeekDonateExp; // 주간 기부 공헌도 
    [SerializeField] private Text txtBattlePoint;

    [SerializeField] private Color logInTxtColor;
    [SerializeField] private Color logoutTxtColor;
    #endregion
    #endregion

    #region System Variables
    public ulong GuildID { get; private set; }

    private bool isSlotMe;

    private Action _onClicked;
    private Action _onClicked_ban;
    private Action _onClicked_addFriend;
    private Action _onClicked_approveViceMaster;
    private Action _onClicked_degradeViceMaster;
    private Action _onClicked_approveMaster;

    #endregion

    #region Properties 
    #endregion

    #region Unity Methods
    #endregion

    #region Overrides 
    #endregion

    #region Public Methods
    public void SetData(
        ulong charID
        , bool isMe
        , bool isSelected
        , E_GuildMemberGrade gradeType
        , Sprite iconSprite
        , string name
        , string grade
        , string notice
        , ulong exp
        , ulong donateExp
        , ulong weekExp
        , ulong weekDonateExp
        , uint battlePoint
        , bool attendance
        , bool isLogInNow)
    {
        isSlotMe = isMe;

        var myGrade = Me.CurCharData.GuildGrade;
        bool canAddFriend =
            isMe == false &&
            Me.CurCharData.friendList.Exists(t => t.CharId == charID) == false &&
            Me.CurCharData.requestfriendList.Exists(t => t.CharId == charID) == false;
        btnAddFriend.interactable = canAddFriend;
        imgThisPlayerIndicator.gameObject.SetActive(isMe);

        SetBtnActive(false, false, false, false, false);
        imgSelectedGradient.gameObject.SetActive(false);

        // 등급에 따른 버튼 활성화 세팅 

        // 나면은 아무것도 안띄움 
        if (isMe == false && isSelected)
        {
            imgSelectedGradient.gameObject.SetActive(true);

            // 내가 마스터일때 
            if (myGrade == E_GuildMemberGrade.Master)
            {
                if (gradeType == E_GuildMemberGrade.SubMaster)
                {
                    SetBtnActive(
                        ban: true
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: true
                        , appointMaster: true);
                }
                else if (gradeType == E_GuildMemberGrade.Normal)
                {
                    SetBtnActive(
                        ban: true
                        , addFriend: true
                        , appointViceMaster: true
                        , degradeViceMaster: false
                        , appointMaster: true);
                }
            }
            // 내가 부마스터일때 
            else if (myGrade == E_GuildMemberGrade.SubMaster)
            {
                if (gradeType == E_GuildMemberGrade.Master)
                {
                    SetBtnActive(
                        ban: false
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: false
                        , appointMaster: false);
                }
                else if (gradeType == E_GuildMemberGrade.SubMaster)
                {
                    SetBtnActive(
                        ban: false
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: true /// 부길마는 부길마를 해임할수있음 .. 
                        , appointMaster: false);
                }
                else if (gradeType == E_GuildMemberGrade.Normal)
                {
                    SetBtnActive(
                        ban: true
                        , addFriend: true
                        , appointViceMaster: true
                        , degradeViceMaster: false
                        , appointMaster: false);
                }
            }
            // 내가 일반 길드원일때 
            else if (myGrade == E_GuildMemberGrade.Normal)
            {
                if (gradeType == E_GuildMemberGrade.Master)
                {
                    SetBtnActive(
                        ban: false
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: false
                        , appointMaster: false);
                }
                else if (gradeType == E_GuildMemberGrade.SubMaster)
                {
                    SetBtnActive(
                        ban: false
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: false
                        , appointMaster: false);
                }
                else if (gradeType == E_GuildMemberGrade.Normal)
                {
                    SetBtnActive(
                        ban: false
                        , addFriend: true
                        , appointViceMaster: false
                        , degradeViceMaster: false
                        , appointMaster: false);
                }
            }
        }

        imgIcon.sprite = iconSprite;
        imgAttendance.gameObject.SetActive(attendance);

        txtName.text = name;
        txtGrade.text = grade;
        txtNotice.text = notice;
        txtLogInStatus.text = isLogInNow ? "접속중" : "미접속";
        txtLogInStatus.color = isLogInNow ? logInTxtColor : logoutTxtColor;
        txtExp_DonateExp.text = string.Format("{0}\n{1}", exp.ToString("n0"), donateExp.ToString("n0"));
        txtWeekExp_WeekDonateExp.text = string.Format("{0}\n{1}", weekExp.ToString("n0"), weekDonateExp.ToString("n0"));
        txtBattlePoint.text = battlePoint.ToString();
        imgGuildMasterIndicator.gameObject.SetActive(gradeType == E_GuildMemberGrade.Master);
        imgGuildSubMasterIndicator.gameObject.SetActive(gradeType == E_GuildMemberGrade.SubMaster);

        canvasGroup.alpha = isLogInNow ? 1f : 0.25f;
    }

    public void AddListener_OnClicked(Action callback)
    {
        _onClicked += callback;
    }

    public void AddListener_Ban(Action callback)
    {
        _onClicked_ban += callback;
    }

    public void AddListener_AddFriend(Action callback)
    {
        _onClicked_addFriend += callback;
    }

    public void AddListener_ApproveViceMaster(Action callback)
    {
        _onClicked_approveViceMaster += callback;
    }

    public void AddListener_DegradeViceMaster(Action callback)
    {
        _onClicked_degradeViceMaster += callback;
    }

    public void AddListener_ApproveMaster(Action callback)
    {
        _onClicked_approveMaster += callback;
    }
    #endregion

    #region Private Methods
    void SetBtnActive(
        bool ban
        , bool addFriend
        , bool appointViceMaster
        , bool degradeViceMaster
        , bool appointMaster)
    {
        btnBan.gameObject.SetActive(ban);
        btnAppointViceMaster.gameObject.SetActive(appointViceMaster);
        btnDegradeViceMaster.gameObject.SetActive(degradeViceMaster);
        btnAppointMaster.gameObject.SetActive(appointMaster);
        btnAddFriend.gameObject.SetActive(addFriend);
    }
    #endregion

    #region OnClick Event (인스펙터 연결)
    public void OnClicked()
    {
        _onClicked?.Invoke();
    }

    // 강퇴 클릭시 
    public void OnClickKickOut()
    {
        _onClicked_ban?.Invoke();
    }

    public void OnClickRequestFriend()
    {
        _onClicked_addFriend?.Invoke();
        btnAddFriend.interactable = false;
    }

    public void OnClickAppointViceMaster()
    {
        _onClicked_approveViceMaster?.Invoke();
    }

    public void OnClickDegradeViceMaster()
    {
        _onClicked_degradeViceMaster?.Invoke();
    }

    public void OnClickAppointGuildMaster()
    {
        _onClicked_approveMaster?.Invoke();
    }
    #endregion
}
