using UnityEngine;
using UnityEngine.UI;

public class UIInfoTextSlot : MonoBehaviour
{
    #region UI Variable
    [SerializeField] Text intoTitle;
    [SerializeField] Text infoText;
    #endregion

    #region Class Variable
    private string strDesc;
    private string value;
    private string strTitle;
    private float cellSize;

    private bool check;
    #endregion

    public void Initialize(string _strDesc, string _value, string _strTitle = "", float _cellSize = 0.0f)
    {
        strDesc = _strDesc;
        value = _value;
        strTitle = _strTitle;
        cellSize = _cellSize;

        intoTitle.text = _strTitle;
        infoText.text = strDesc + " " + value;

        check = true;
    }

    public bool GetSlotActiveCheck()
    {
        return check;
    }

    public void SetSlotActiveCheck()
    {
        check = false;
    }
}
