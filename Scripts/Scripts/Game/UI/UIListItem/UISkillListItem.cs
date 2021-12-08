using GameDB;
using UnityEngine;
using UnityEngine.UI;
using ZNet.Data;

public class UISkillListItem : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Text Name = null;
    [SerializeField] private Image Icon = null;
    [SerializeField] private Text Style = null;
    [SerializeField] private GameObject GainAlram = null;
    [SerializeField] private GameObject SelectImg = null;
    #endregion

    #region System Variable
    public Skill_Table Skill { get; private set; }
    private UIFrameSkill FrameSkill = null;
    #endregion

    public void Initialize(Skill_Table _skill)
    {
        FrameSkill = UIManager.Instance.Find<UIFrameSkill>();

        transform.localPosition = Vector3.zero;
        transform.localScale = Vector2.one;

        Skill = _skill;

        Skill_Table table = DBSkill.Get(Skill.SkillID);

        Name.text = DBLocale.GetSkillLocale(table);
        Icon.sprite = ZManagerUIPreset.Instance.GetSprite(table.IconID);
        Style.text = table.SkillType.ToString();
        SetGain();
    }

    public void SetGain()
    {
        GainAlram.SetActive(!Me.CurCharData.HasGainSkill(Skill.SkillID));
    }
}