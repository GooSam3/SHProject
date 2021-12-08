using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UICharacterCreateListItem : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Image ClassIcon = null;
    [SerializeField] private Text ClassName = null;
    [SerializeField] private Text ClassInfoTxt = null;
    #endregion

    #region System Variable
    public int SlotIndex = 0;
    [SerializeField] private ZToggle SelectBtn = null;

    public string CharTextId { get; private set; } = string.Empty;
    public uint CharTid { get; private set; } = 0;
    #endregion

    /// <summary>슬롯 정보 초기화 및 초기 세팅</summary>
    /// <param name="_slotIdx"></param>
    /// <param name="_charTid"></param>
    public void Initialize(int _slotIdx, uint _charTid)
    {
        transform.localScale = Vector2.one;

        CharTid = _charTid;
        CharTextId = DBCharacter.Get(_charTid).CharacterTextID;
        SlotIndex = _slotIdx;
        
        DBLocale.TryGet(DBCharacter.Get(CharTid).CharacterTextID, out Locale_Table table);

        ClassName.text = table.Text;
        ClassInfoTxt.text = DBLocale.GetText(DBCharacter.Get(CharTid).ToolTipID).Replace("\\n", "\n");
        SetIcon(false);

        if(UIManager.Instance.Find(out UIFrameCharacterSelect _charSelect))
            _charSelect.RegisterCreateToggle(SelectBtn);
    }

    public void SelectToggleAction()
    {
        SelectBtn.SelectToggle();
    }

    /// <summary>슬롯 선택 처리 및 캐릭터 정보 세팅 </summary>
    public void SelectCharacter()
    {
        UIFrameCharacterSelect frame = UIManager.Instance.Find<UIFrameCharacterSelect>();
        frame.SetSelectSlot(E_CharacterSelectState.Create, SlotIndex + 1);
		frame.SelectCharAttribute((int)E_UnitAttributeType.Fire);
        frame.RefreshSelectSlot(E_CharacterSelectState.Create);

        // to do : 별도의 테이블 정보가 없어서 임시로 처리
        switch (DBCharacter.GetClassTypeByTid(CharTid))
        {
            case E_CharacterType.Knight: frame.ChangeClassIcon(ZManagerUIPreset.Instance.GetSprite("icon_char_class_gl_b")); break;
            case E_CharacterType.Assassin: frame.ChangeClassIcon(ZManagerUIPreset.Instance.GetSprite("icon_char_class_as_b")); break;
            case E_CharacterType.Archer: frame.ChangeClassIcon(ZManagerUIPreset.Instance.GetSprite("icon_char_class_ar_b")); break;
            case E_CharacterType.Wizard: frame.ChangeClassIcon(ZManagerUIPreset.Instance.GetSprite("icon_char_class_ma_b")); break;
        }

        SetIcon(true);
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