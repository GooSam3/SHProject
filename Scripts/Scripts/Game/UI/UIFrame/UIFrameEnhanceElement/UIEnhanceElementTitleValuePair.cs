using UnityEngine;
using UnityEngine.UI;

public class UIEnhanceElementTitleValuePair : MonoBehaviour
{
    #region Serialized Field
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image imgHighlighted;
    [SerializeField] private Text txtTitle;
    [SerializeField] private Text txtValue;
    #endregion

    #region Properties 
    public RectTransform RectTransform { get { return rectTransform; } }
    #endregion

    #region Public Methods
    public void ActivateHighlight(Color color)
    {
        if (imgHighlighted == null)
            return;

        imgHighlighted.gameObject.SetActive(true);
        imgHighlighted.color = color;
    }

    public void DeactivateHighlight()
    {
        if (imgHighlighted == null) 
            return;

        imgHighlighted.gameObject.SetActive(false);
    }

    public void SetText(string title, string value)
    {
        txtTitle.text = title;
        txtValue.text = value;
    }

    public void SetColor(Color titleColor, Color valueColor)
    {
        txtTitle.color = titleColor;
        txtValue.color = valueColor;
    }
    #endregion
}