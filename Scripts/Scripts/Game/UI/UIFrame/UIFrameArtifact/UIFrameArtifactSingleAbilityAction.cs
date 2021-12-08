using UnityEngine;
using UnityEngine.UI;

public class UIFrameArtifactSingleAbilityAction : MonoBehaviour
{
    public Text txtTitle;
    public Text txtValue;

    public void SetText(string title, string value)
    {
        txtTitle.text = title;
        txtValue.text = value;
    }

    public void SetColor(Color title, Color value)
    {
        txtTitle.color = title;
        txtValue.color = value;
    }
}
