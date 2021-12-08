using UnityEngine.UI;
using UnityEngine;

public class MarkMarkInfoPopup : MonoBehaviour
{
    public RectTransform fitterRectTransform;

    public Text txtTitle;
    public Text txtContent;

    public void Set(string title, Color titleColor, string content)
    {
        txtTitle.color = titleColor;
        txtTitle.text = title;
        txtContent.text = content;
    }
}