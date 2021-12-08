using UnityEngine;
using UnityEngine.UI;

public class UIFrameArtifactManufactureCost : MonoBehaviour
{
    public Image imgCostIcon;
    public Text txtCost;

    public void Set(
        Sprite costIconSprite
        , ulong curCostCnt
        , uint maxCostCnt
        , Color myCurrencyColor)
    {
        imgCostIcon.sprite = costIconSprite;
        txtCost.text = string.Format("<color=#{0}>{1}</color> / {2}"
            , ColorUtility.ToHtmlStringRGB(myCurrencyColor)
            , curCostCnt.ToString("n0")
            , maxCostCnt.ToString("n0"));
    }
}
