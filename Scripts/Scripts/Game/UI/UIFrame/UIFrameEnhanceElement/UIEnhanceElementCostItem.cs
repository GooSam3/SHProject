using UnityEngine;
using UnityEngine.UI;

public class UIEnhanceElementCostItem : MonoBehaviour
{
    #region Serialized Field
    [SerializeField] private Image imgItemImg;
    [SerializeField] private Text txtItemCnt;
    #endregion

    #region Public Methods
    public void Set(Sprite itemSprite, uint itemCnt)
    {
        imgItemImg.sprite = itemSprite;
        txtItemCnt.text = itemCnt.ToString("n0");
    }
    #endregion
}
