using GameDB;
using System;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterSelectListItem : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image ClassIcon = null;
    [SerializeField] private Text LevelTxt = null;
    [SerializeField] private Text ClassName = null;
    [SerializeField] private Text NicknameTxt = null;
    [SerializeField] private GameObject AddIcon = null;
    [SerializeField] private GameObject LockIcon = null;
    [SerializeField] private GameObject DeleteTxt = null;
    [SerializeField] private Text DeleteDateTxt = null;
    #endregion

    #region System Variable
    public int SlotIndex = 0;
    [SerializeField] private uint CharTid = 0;
    [SerializeField] private string Nickname = string.Empty;
    [SerializeField] private uint Level = 0;
    [SerializeField] private bool Lock = false;
    [SerializeField] private Action Callback = null;
    [SerializeField] private ZToggle SelectBtn = null;

    public ulong CharId { get; private set; } = 0;
    public uint StageTid { get; private set; } = 0;
    public ulong LastLogoutDt { get; private set; } = 0;
    public ulong DeleteDt { get; private set; } = 0;
    #endregion

    /// <summary>슬롯 정보 초기화 및 초기 세팅</summary>
    /// <param name="_slotIdx"></param>
    /// <param name="_charId"></param>
    /// <param name="_charTid"></param>
    /// <param name="_stageTid">마지막에 위치한 스테이지 ID</param>
    /// <param name="_nickName"></param>
    /// <param name="_charLevel"></param>
    /// <param name="_lock">슬롯 잠김 여부</param>
    /// <param name="_lastLogoutDt">마지막 로그인 시간</param>
    /// <param name="_deleteDt">삭제 남은 시간</param>
    /// <param name="_callBack"></param>
    public void Initialize(int _slotIdx, ulong _charId, uint _charTid, uint _stageTid, string _nickName, uint _charLevel, bool _lock, ulong _lastLogoutDt, ulong _deleteDt, Action _callBack)
    {
        transform.localScale = Vector2.one;

        SlotIndex = _slotIdx;
        CharId = _charId;
        CharTid = _charTid;
        StageTid = _stageTid;
        Nickname = _nickName;
        Level = _charLevel;
        Lock = _lock;
        LastLogoutDt = _lastLogoutDt;
        DeleteDt = _deleteDt;
        Callback = _callBack;

        ClassIcon.gameObject.SetActive(CharTid != 0);
        AddIcon.SetActive(CharTid == 0 && !Lock);
        LockIcon.SetActive(Lock);
        DeleteTxt.SetActive(DeleteDt != 0);

        if (CharTid != 0)
        {
            DBLocale.TryGet(DBCharacter.Get(CharTid).CharacterTextID, out Locale_Table table);
            ClassName.text = table.Text;
            LevelTxt.text = "Lv. " + Level.ToString();
            NicknameTxt.text = Nickname;

            SetIcon(false);

            if (DeleteDt != 0)
                DeleteDateTxt.text = TimeHelper.GetRemainTime(TimeManager.NowSec - DeleteDt + DBConfig.Char_Delete_Time, "", " 남음");

            if (UIManager.Instance.Find(out UIFrameCharacterSelect _charSelect))
                _charSelect.RegisterSelectToggle(SelectBtn);
        }
        else
        {
            ClassName.text = string.Empty;
            LevelTxt.text = string.Empty;
            NicknameTxt.text = string.Empty;
        }
    }

    /// <summary>슬롯 인덱스 세팅</summary>
    /// <param name="_idx">슬롯 Idx</param>
    public void SetSlotIdx(int _idx)
	{
        SlotIndex = _idx;
	}

    /// <summary>슬롯 선택 처리 및 캐릭터 정보 세팅 </summary>
    public void SelectCharacter()
    {
        if (!UIManager.Instance.Find(out UIFrameCharacterSelect _frame))
            return;

        if (Lock)
            return;
     
        _frame.SetSelectSlot(E_CharacterSelectState.Select, SlotIndex);
        _frame.RefreshSelectSlot(E_CharacterSelectState.Select);
        _frame.ActiveDeleteButton(DeleteDt == 0);
        _frame.ActiveDeleteRollbackButton(DeleteDt != 0);

        SetIcon(true);       
        Callback?.Invoke();
    }

    /// <summary>라디오 토글 선택 처리 </summary>
    public void SelectButton()
	{
        SelectBtn.SelectToggle();
    }

    /// <summary>캐릭터 클래스 아이콘 출력</summary>
    /// <param name="_effect">Effect 아이콘인지 여부</param>
    public void SetIcon(bool _effect)
    {
        string iconType = "icon";
        if (_effect)
            iconType = "eff";

        // to do : 별도의 테이블 정보가 없어서 임시로 처리
        switch (DBCharacter.GetClassTypeByTid(CharTid))
        {
            case E_CharacterType.Knight: ClassIcon.sprite =   ZManagerUIPreset.Instance.GetSprite(iconType + "_char_class_gl"); break;
            case E_CharacterType.Assassin: ClassIcon.sprite = ZManagerUIPreset.Instance.GetSprite(iconType + "_char_class_as"); break;
            case E_CharacterType.Archer: ClassIcon.sprite =   ZManagerUIPreset.Instance.GetSprite(iconType + "_char_class_ar"); break;
            case E_CharacterType.Wizard: ClassIcon.sprite =   ZManagerUIPreset.Instance.GetSprite(iconType + "_char_class_ma"); break;
        }
    }
}