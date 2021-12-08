using UnityEngine;
using UnityEngine.UI;

public class ZUIScrollAbilityListItem : CUGUIWidgetSlotItemBase
{
    [SerializeField] private Text Name;
    [SerializeField] private Text Value;

    protected override void OnUIWidgetInitialize(CUIFrameBase _UIFrameParent)
    {
        base.OnUIWidgetInitialize(_UIFrameParent);

        Name.text = "NULL";
        Value.text = "NULL";
    }

    public void SetSlot(UIAbilityData ability)
    {
        Name.text = DBLocale.GetText(DBAbility.GetAbilityName(ability.type));
        Value.text = DBAbility.ParseAbilityValue((uint)(ability.type), ability.value); 
    }
}
