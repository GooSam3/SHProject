using GameDB;
using UnityEngine;
using UnityEngine.UI;

public class UIChangeAbilitySlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Text AbilityName;
    [SerializeField] private Text AbilityValue;
    #endregion

    #region System Variable
    #endregion

    public void Initialize()
    {
        gameObject.SetActive(false);
    }

    public void SetSlot(E_AbilityType abilityType, float value)
    {
        AbilityName.text = DBLocale.GetText(DBAbility.GetAbilityName(abilityType));
        AbilityValue.text = DBAbility.ParseAbilityValue((uint)abilityType, value);
    }
}
