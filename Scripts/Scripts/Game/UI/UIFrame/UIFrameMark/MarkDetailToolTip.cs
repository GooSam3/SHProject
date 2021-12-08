using UnityEngine;
using UnityEngine.UI;

public class MarkDetailToolTip : MonoBehaviour
{
    public RectTransform RectTransform;
    public RectTransform fitterRectTransform;
    public Text txtTitle;
    public Text txtValue;

    public void Set(string title, Color titleColor, string value)
    {
        bool layoutUpdate = false;

        if (txtTitle.text != title)
        {
            layoutUpdate = true;
            txtTitle.text = title;
        }
        if (txtValue.text != value)
        {
            layoutUpdate = true;
            txtValue.text = value;
        }

        txtTitle.color = titleColor;
        
        if(layoutUpdate)
            LayoutRebuilder.ForceRebuildLayoutImmediate(fitterRectTransform);
    }
}
