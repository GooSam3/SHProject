using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIEnchantAbilitySlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] private Slider SliderValue;
    [SerializeField] private Text AbilityInfo;
	#endregion

	#region System Variable
	private Tweener TweenSlider;
    #endregion

    public void ResetUI()
    {
        TweenSlider?.Kill();
        TweenSlider = null;

        SliderValue.value = 0f;
        AbilityInfo.text = string.Empty;
    }

    public void Set(uint _enchantOptionTid)
    {
        if (0 >= _enchantOptionTid)
        {
            ResetUI();
            return;
        }

        if (false == DBResmelting.GetResmeltScrollOption(_enchantOptionTid, out var table))
        {
            ResetUI();
            return;
        }

        var keyPair = DBResmelting.GetResmeltingOptionGroupOrder(table);
        UpdateUI(table.AbilityActionID, keyPair.Key / (float)keyPair.Value);
    }

    private void UpdateUI(uint abilityActionId, float sliderValue)
    {
        var abilityActionTable = DBAbility.GetAction(abilityActionId);

        string abilityName = DBAbility.GetAbilityName(abilityActionTable.AbilityID_01);
        uint abilityId = (uint)abilityActionTable.AbilityID_01;
        float abilityValue = abilityActionTable.AbilityPoint_01_Min;

        if (null != TweenSlider)
            TweenSlider.Kill();

        TweenSlider = DOTween.To(() => 0, value => UpdateUI(abilityName, abilityId, abilityValue, sliderValue, value), 1f, 1.5f * sliderValue).SetEase(Ease.InQuad);

        TweenSlider.onComplete += () =>
        {
            UpdateUI(abilityName, abilityId, abilityValue, sliderValue, 1f);
        };
    }

    private void UpdateUI(string abilityText, uint abilityId, float abilityValue, float sliderValue, float factor)
    {
        AbilityInfo.text = string.Format("{0} {1}", DBLocale.GetText(abilityText), DBAbility.ParseAbilityValue(abilityId, abilityValue * factor));
        SliderValue.value = sliderValue * factor;
    }
}
